using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Npgsql;
using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using MyHTTPServer.Sessions;

namespace MyHTTPServer.EndPoints;

public class AdminDatabaseEndPoint : BaseEndPoint
{
    private readonly string _connectionString = "Host=localhost;Port=5432;Username=postgres;Password=19370;Database=films;Encoding=UTF8";

    // Главная страница админ-панели
    [Get("admin-dashboard")]
    public IHttpResponceResult AdminDashboard()
    {
        if (!SessionStorage.IsAdmin(Context))
        {
            Console.WriteLine("Доступ запрещён");
            return Html("<h1>Доступ запрещён</h1>");
        }

        Console.WriteLine("Пользователь авторизован. Возвращаем админ-панель.");
        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<!DOCTYPE html>");
        htmlBuilder.Append("<html lang='ru'>");
        htmlBuilder.Append("<head>");
        htmlBuilder.Append("<meta charset='UTF-8'>");
        htmlBuilder.Append("<title>Админ-панель</title>");
        htmlBuilder.Append("<style>");
        htmlBuilder.Append(@"
            body {
                font-family: Arial, sans-serif;
                background-color: #f0f8ff;
                margin: 0;
                padding: 0;
            }
            header {
                background-color: #0078D7;
                color: white;
                padding: 10px;
                text-align: center;
            }
            header h1 {
                margin: 0;
            }
            ul {
                list-style-type: none;
                padding: 0;
            }
            li {
                margin: 10px 0;
            }
            a {
                text-decoration: none;
                color: #0078D7;
                font-weight: bold;
            }
            a:hover {
                color: #005A9E;
            }
            .container {
                padding: 20px;
                max-width: 800px;
                margin: 0 auto;
            }
            .logout-btn {
                display: inline-block;
                margin-top: 20px;
                padding: 10px 20px;
                background-color: #D9534F;
                color: white;
                text-decoration: none;
                border-radius: 5px;
                font-weight: bold;
            }
            .logout-btn:hover {
                background-color: #C9302C;
            }
        ");
        htmlBuilder.Append("</style>");
        htmlBuilder.Append("</head>");
        htmlBuilder.Append("<body>");
        htmlBuilder.Append("<header>");
        htmlBuilder.Append("<h1>Админ-Панель</h1>");
        htmlBuilder.Append("</header>");
        htmlBuilder.Append("<div class='container'>");
        htmlBuilder.Append("<h2>Управление базами данных</h2>");
        htmlBuilder.Append("<ul>");
        htmlBuilder.Append("<li><a href='/admin/manage-table?table=movies'>Фильмы</a></li>");
        htmlBuilder.Append("<li><a href='/admin/manage-table?table=lectures'>Лекции</a></li>");
        htmlBuilder.Append("</ul>");

        // Кнопка выхода
        htmlBuilder.Append("<a class='logout-btn' href='/admin/logout'>Выйти</a>");
        htmlBuilder.Append("</div>");
        htmlBuilder.Append("</body>");
        htmlBuilder.Append("</html>");

        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html(htmlBuilder.ToString());
    }

    // Выход из админ-панели
    [Get("admin/logout")]
    public IHttpResponceResult Logout()
    {
        Console.WriteLine("[Logout] Выполняется выход");

        // Удаление сессионного токена
        var cookie = new Cookie("session-token", "") { Expires = DateTime.Now.AddDays(-1) };
        Context.Response.SetCookie(cookie);

        // Очистка сессии
        SessionStorage.ClearSession();

        Console.WriteLine("[Logout] Редирект на страницу входа");
        return Redirect("http://localhost:6529/login");
    }

    // Управление выбранной таблицей
    [Get("admin/manage-table")]
    public IHttpResponceResult ManageTable(string table)
    {
        if (!SessionStorage.IsAdmin(Context))
        {
            return Html("<h1>Доступ запрещён</h1>");
        }

        var rows = GetTableData(table);

        var htmlBuilder = new StringBuilder();
        htmlBuilder.Append("<!DOCTYPE html>");
        htmlBuilder.Append("<html lang='ru'>");
        htmlBuilder.Append("<head>");
        htmlBuilder.Append("<meta charset='UTF-8'>");
        htmlBuilder.Append("<title>Управление таблицей</title>");
        htmlBuilder.Append("</head>");
        htmlBuilder.Append("<body>");
        htmlBuilder.Append($"<h1>Управление таблицей: {table}</h1>");
        htmlBuilder.Append("<table border='1'>");
        htmlBuilder.Append("<tr><th>ID</th><th>Название</th><th>Год</th><th>Режиссер</th><th>Путь к картинке</th><th>Описание</th><th>Ссылка на видео</th><th>Действия</th></tr>");

        foreach (var row in rows)
        {
            htmlBuilder.Append($"<tr>");
            htmlBuilder.Append($"<td>{row[0]}</td>");
            htmlBuilder.Append($"<td>{row[1]}</td>");
            htmlBuilder.Append($"<td>{row[2]}</td>");
            htmlBuilder.Append($"<td>{row[3]}</td>");
            htmlBuilder.Append($"<td>{row[4]}</td>");
            htmlBuilder.Append($"<td>{row[5]}</td>");
            htmlBuilder.Append($"<td>{row[6]}</td>");
            htmlBuilder.Append($"<td><a href='/admin/delete-row?table={table}&id={row[0]}'>Удалить</a></td>");
            htmlBuilder.Append($"</tr>");
        }

        htmlBuilder.Append($"<form method='POST' action='/admin/add-row'>");
        htmlBuilder.Append($"<input type='hidden' name='table' value='{table}'>");
        htmlBuilder.Append("<input type='number' name='id' placeholder='ID' required>");
        htmlBuilder.Append("<input type='text' name='title' placeholder='Название' required>");
        htmlBuilder.Append("<input type='number' name='year' placeholder='Год' required>");
        htmlBuilder.Append("<input type='text' name='director' placeholder='Режиссер' required>");
        htmlBuilder.Append("<input type='text' name='image_path' placeholder='Путь к картинке' required>");
        htmlBuilder.Append("<input type='text' name='description' placeholder='Описание' required>");
        htmlBuilder.Append("<input type='text' name='video_url' placeholder='Ссылка на видео' required>");
        htmlBuilder.Append("<button type='submit'>Добавить</button>");
        htmlBuilder.Append("</form>");

        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html(htmlBuilder.ToString());
    }

    // Удаление строки
    [Get("admin/delete-row")]
    public IHttpResponceResult DeleteRow(string table, int id)
    {
        if (!SessionStorage.IsAdmin(Context))
        {
            return Html("<h1>Доступ запрещён</h1>");
        }

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                // Удаление связанных данных из movie_details или lecture_details
                if (table == "movies")
                {
                    var deleteDetailsQuery = "DELETE FROM movie_details WHERE movie_id = @id";
                    using (var command = new NpgsqlCommand(deleteDetailsQuery, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
                }
                else if (table == "lectures")
                {
                    var deleteDetailsQuery = "DELETE FROM lecture_details WHERE lecture_id = @id";
                    using (var command = new NpgsqlCommand(deleteDetailsQuery, conn))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
                }

                // Удаление строки из основной таблицы
                var deleteQuery = $"DELETE FROM {table} WHERE id = @id";
                using (var command = new NpgsqlCommand(deleteQuery, conn))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка удаления записи: {ex.Message}");
            return Html($"<h1>Ошибка: {ex.Message}</h1>");
        }

        return Redirect($"/admin/manage-table?table={table}");
    }

    // Добавление строки
    

    [Post("admin/add-row")]
public IHttpResponceResult AddRow()
{
    if (!SessionStorage.IsAdmin(Context))
    {
        return Html("<h1>Доступ запрещён</h1>");
    }

    // Чтение тела запроса
    using var reader = new StreamReader(Context.Request.InputStream, Encoding.UTF8);
    string body = reader.ReadToEnd();
    Console.WriteLine($"[DEBUG] Тело запроса: {body}");

    // Парсинг данных формы
    var formData = System.Web.HttpUtility.ParseQueryString(body);
    string table = formData["table"];
    int id = int.Parse(formData["id"]);
    string title = formData["title"];
    int year = int.Parse(formData["year"]);
    string director = formData["director"];
    string image_path = formData["image_path"];
    string description = formData["description"];
    string video_url = formData["video_url"];

    Console.WriteLine($"[INFO] Данные формы: table={table}, id={id}, title={title}, year={year}, director={director}, image_path={image_path}, description={description}, video_url={video_url}");

    // Проверка обязательных полей
    if (string.IsNullOrEmpty(title) || year <= 0 || string.IsNullOrEmpty(director))
    {
        Console.WriteLine($"[WARN] Некорректные данные: title={title}, year={year}, director={director}");
        return Html("<h1 style='color: red;'>Ошибка: Пожалуйста, заполните все поля корректно.</h1>");
    }

    try
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();

            // Начинаем транзакцию
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // Проверка и замена image_path на дефолтное значение, если оно пустое или некорректное
                    if (string.IsNullOrWhiteSpace(image_path) || !IsValidImagePath(image_path))
                    {
                        image_path = "images/logo_12.png";
                    }

                    // Вставка в основную таблицу (movies или lectures)
                    var insertMainQuery = $"INSERT INTO {table} (id, title, year, director, image_path) VALUES (@id, @title, @year, @director, @image_path)";
                    using (var command = new NpgsqlCommand(insertMainQuery, conn, transaction))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@title", title);
                        command.Parameters.AddWithValue("@year", year);
                        command.Parameters.AddWithValue("@director", director);
                        command.Parameters.AddWithValue("@image_path", image_path);
                        command.ExecuteNonQuery();
                    }

                    // Вставка в связанную таблицу (movie_details или lecture_details)
                    var detailsTable = table == "movies" ? "movie_details" : "lecture_details";
                    var foreignKeyColumn = table == "movies" ? "movie_id" : "lecture_id";

                    var insertDetailsQuery = $"INSERT INTO {detailsTable} ({foreignKeyColumn}, description, video_url) VALUES (@id, @description, @video_url)";
                    using (var command = new NpgsqlCommand(insertDetailsQuery, conn, transaction))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@description", description);
                        command.Parameters.AddWithValue("@video_url", video_url);
                        command.ExecuteNonQuery();
                    }

                    // Фиксация транзакции
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Откат транзакции в случае ошибки
                    transaction.Rollback();
                    throw new Exception($"Ошибка при добавлении данных: {ex.Message}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка добавления записи: {ex.Message}");
        return Html($"<h1 style='color: red;'>Ошибка: {ex.Message}</h1>");
    }

    return Redirect($"/admin/manage-table?table={table}");
}

    // Метод для проверки корректности пути к картинке
    private bool IsValidImagePath(string imagePath)
    {
        // Пример проверки: путь должен начинаться с "images/" и заканчиваться на ".png", ".jpg" или ".jpeg"
        return !string.IsNullOrWhiteSpace(imagePath) &&
               imagePath.StartsWith("images/", StringComparison.OrdinalIgnoreCase) &&
               (imagePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                imagePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                imagePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
    }

    // Получение данных из таблицы
    private List<List<object>> GetTableData(string table)
    {
        var rows = new List<List<object>>();

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                // Запрос для получения данных из основной и связанной таблиц
                var query = $@"
                    SELECT 
                        m.id, 
                        m.title, 
                        m.year, 
                        m.director, 
                        m.image_path, 
                        md.description, 
                        md.video_url
                    FROM 
                        {table} m
                    LEFT JOIN 
                        {(table == "movies" ? "movie_details" : "lecture_details")} md 
                    ON 
                        m.id = md.{(table == "movies" ? "movie_id" : "lecture_id")}";

                Console.WriteLine($"[DEBUG] SQL-запрос: {query}");

                using (var command = new NpgsqlCommand(query, conn))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var row = new List<object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.GetValue(i));
                        }
                        rows.Add(row);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка получения данных: {ex.Message}");
        }

        return rows;
    }
}