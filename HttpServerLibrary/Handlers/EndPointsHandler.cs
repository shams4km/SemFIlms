using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;

namespace HttpServerLibrary.Handlers;

internal class EndPointsHandler : Handler
{
    private readonly Dictionary<string, List<(HttpMethod method, MethodInfo handler, Type endpointType)>> _routes = new();

    public EndPointsHandler()
    {
        RegisterEndpointsFromAssemblies(new[] { Assembly.GetEntryAssembly() });
    }

    public override void HandleRequest(HttpRequestContext context)
    {
        Console.WriteLine($"[INFO] Запрос получен: {context.Request.HttpMethod} {context.Request.Url}");

        var url = context.Request.Url.LocalPath.Trim('/');
        var methodType = context.Request.HttpMethod.ToUpperInvariant();

        var route = FindRoute(url, methodType);

        if (route.handler != null)
        {
            Console.WriteLine($"[INFO] Маршрут найден: {url}");
            Console.WriteLine($"[INFO] Обработчик найден: {route.handler.Name}");

            var endpointInstance = Activator.CreateInstance(route.endpointType) as BaseEndPoint;

            if (endpointInstance != null)
            {
                endpointInstance.SetContext(context);

                var parameters = GetParams(context, route.handler, route.parameters);
                var result = route.handler.Invoke(endpointInstance, parameters) as IHttpResponceResult;

                if (result != null)
                {
                    result.Execute(context.Response);
                    context.Response.Close();
                    return;
                }
            }
        }

        if (Successor != null)
        {
            Console.WriteLine($"[WARN] Маршрут не найден, передача следующему обработчику: {url}");
            Successor.HandleRequest(context);
        }
        else
        {
            Console.WriteLine($"[ERROR] Маршрут не найден: {url}");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.Close();
        }
    }

    private (MethodInfo handler, Type endpointType, Dictionary<string, string> parameters) FindRoute(string url, string methodType)
    {
        foreach (var route in _routes)
        {
            var routePattern = route.Key;
            var routeMethods = route.Value;

            if (IsRouteMatch(routePattern, url, out var parameters))
            {
                var routeHandler = routeMethods.FirstOrDefault(r =>
                    r.method.ToString().Equals(methodType, StringComparison.InvariantCultureIgnoreCase));

                if (routeHandler.handler != null)
                {
                    return (routeHandler.handler, routeHandler.endpointType, parameters);
                }
            }
        }

        return (null, null, null);
    }

    private bool IsRouteMatch(string routePattern, string url, out Dictionary<string, string> parameters)
    {
        parameters = new Dictionary<string, string>();

        var routeParts = routePattern.Split('/');
        var urlParts = url.Split('/');

        if (routeParts.Length != urlParts.Length)
        {
            return false;
        }

        for (int i = 0; i < routeParts.Length; i++)
        {
            if (routeParts[i].StartsWith("{") && routeParts[i].EndsWith("}"))
            {
                var paramName = routeParts[i].Trim('{', '}');
                parameters[paramName] = urlParts[i];
            }
            else if (routeParts[i] != urlParts[i])
            {
                return false;
            }
        }

        return true;
    }

    private void RegisterEndpointsFromAssemblies(Assembly[] assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            var endpointsTypes = assembly.GetTypes()
                .Where(t => typeof(BaseEndPoint).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var endpointType in endpointsTypes)
            {
                var methods = endpointType.GetMethods();
                foreach (var method in methods)
                {
                    var getAttribute = method.GetCustomAttribute<GetAttribute>();
                    if (getAttribute != null)
                    {
                        RegisterRoute(getAttribute.Route, HttpMethod.Get, method, endpointType);
                    }

                    var postAttribute = method.GetCustomAttribute<PostAttribute>();
                    if (postAttribute != null)
                    {
                        RegisterRoute(postAttribute.Route, HttpMethod.Post, method, endpointType);
                    }
                }
            }
        }
    }

    private void RegisterRoute(string route, HttpMethod method, MethodInfo handler, Type endpointType)
    {
        if (!_routes.ContainsKey(route))
        {
            _routes[route] = new List<(HttpMethod, MethodInfo, Type)>();
        }

        _routes[route].Add((method, handler, endpointType));
        Console.WriteLine($"[INFO] Зарегистрирован маршрут: {method} {route} в {endpointType.Name}.{handler.Name}");
    }

    private object[] GetParams(HttpRequestContext context, MethodInfo handler, Dictionary<string, string> routeParameters)
    {
        var parameters = handler.GetParameters();
        var result = new List<object>();

        foreach (var parameter in parameters)
        {
            if (routeParameters.ContainsKey(parameter.Name))
            {
                result.Add(Convert.ChangeType(routeParameters[parameter.Name], parameter.ParameterType));
            }
            else if (context.Request.HttpMethod == "GET")
            {
                var value = context.Request.QueryString[parameter.Name];
                if (value != null)
                {
                    result.Add(Convert.ChangeType(value, parameter.ParameterType));
                }
                else
                {
                    result.Add(parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null);
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(context.Request.InputStream);
                string body = reader.ReadToEnd();
                var data = HttpUtility.ParseQueryString(body);

                if (data[parameter.Name] != null)
                {
                    result.Add(Convert.ChangeType(data[parameter.Name], parameter.ParameterType));
                }
                else
                {
                    result.Add(parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType) : null);
                }
            }
        }

        return result.ToArray();
    }
}