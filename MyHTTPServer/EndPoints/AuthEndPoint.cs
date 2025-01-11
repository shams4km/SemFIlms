using Npgsql;
using System;
using System.Net;
using System.Text;
using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using MyHTTPServer.Sessions;
using HttpServerLibrary.HttpResponce;

namespace MyHTTPServer.EndPoints;

public class AuthEndPoint : BaseEndPoint
{
    // Эндпоинт для получения страницы входа (GET /login)
    [Get("login")]
    public IHttpResponceResult AuthGet()
    {
        if (SessionStorage.IsAuthorized(Context))
        {
            return Redirect("admin-dashboard");
        }

        var file = File.ReadAllText(@"Templates/Pages/Auth/login.html");
        return Html(file);
    }


    // Эндпоинт для обработки формы входа (POST /login)
    [Post("login")]
    public IHttpResponceResult Login(string mail, string password)
    {
        if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password))
        {
            // Указываем Content-Type с UTF-8
            Context.Response.ContentType = "text/html; charset=utf-8";
            Context.Response.ContentEncoding = Encoding.UTF8;

            return Html("<p>Пожалуйста, заполните все поля</p>");
        }

        if (mail == "kemol622@gmail.com" && password == "admin")
        {
            var sessionToken = SessionStorage.GenerateNewToken();
            var userId = "admin";

            SessionStorage.SaveSession(sessionToken, userId);
            var cookie = new Cookie("session-token", sessionToken) { Expires = DateTime.Now.AddDays(1) };
            Context.Response.SetCookie(cookie);

            return Redirect("/admin-dashboard");
        }

        var file = File.ReadAllText(@"Templates/Pages/Auth/login.html");
        return Html(file);
    }

}

