using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;

namespace MyHTTPServer.EndPoints;

public class ErrorEndpoint : BaseEndPoint
{
    [Get("404")]
    public IHttpResponceResult PageNotFound()
    {
        var filePath = @"Templates/Pages/Errors/404.html";
        var fileContent = File.Exists(filePath)
            ? File.ReadAllText(filePath)
            : "<h1>404 - Page Not Found</h1>";

        return Html(fileContent);
    }
}