using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
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
    public IHttpResponceResult Login()
    {
        // Чтение тела запроса
        using var reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine($"[DEBUG] Тело запроса: {body}");

        // Парсинг данных формы
        var formData = HttpUtility.ParseQueryString(body);
        string mail = formData["mail"];
        string password = formData["password"];

        Console.WriteLine($"[INFO] Попытка входа: mail={mail}, password={password}");

        if (string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password))
        {
            Console.WriteLine($"[WARN] Пустые поля: mail={mail}, password={password}");
            Context.Response.ContentType = "text/html; charset=utf-8";
            Context.Response.ContentEncoding = Encoding.UTF8;
            return Html("<p>Пожалуйста, заполните все поля</p>");
        }

        if (mail == "kemol622@gmail.com" && password == "admin")
        {
            Console.WriteLine($"[INFO] Успешный вход: mail={mail}");
            var sessionToken = SessionStorage.GenerateNewToken();
            var userId = "admin";

            SessionStorage.SaveSession(sessionToken, userId);
            var cookie = new Cookie("session-token", sessionToken) { Expires = DateTime.Now.AddDays(1) };
            Context.Response.SetCookie(cookie);

            return Redirect("/admin-dashboard");
        }

        Console.WriteLine($"[WARN] Неверный логин или пароль: mail={mail}");
        var file = File.ReadAllText(@"Templates/Pages/Auth/login.html");
        return Html(file);
    }
}