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
    // Главная страница админ-панели
    // Страница админ-панели
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
    htmlBuilder.Append("<h1>Админ-Панель </h1>");
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

// Новый эндпоинт для выхода
    [Get("admin/logout")]
    public IHttpResponceResult Logout()
    {
        // Логирование для проверки
        Console.WriteLine("[Logout] Выполняется выход");

        // Удаление сессионного токена (удаление cookie)
        var cookie = new Cookie("session-token", "") { Expires = DateTime.Now.AddDays(-1) };  // Устанавливаем истёкший срок действия
        Context.Response.SetCookie(cookie);  // Устанавливаем cookie с истекшим сроком

        // Также можно очистить данные сессии вручную, если это нужно:
        SessionStorage.ClearSession(); // Допустим, такой метод очищает все сессии, если он существует

        // Логируем успешный выход
        Console.WriteLine("[Logout] Редирект на страницу входа");

        // Перенаправляем пользователя на страницу входа
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
        htmlBuilder.Append("<tr><th>ID</th><th>Название</th><th>Путь</th><th>Год</th><th>Жанр</th><th>Действия</th></tr>");

        foreach (var row in rows)
        {
            htmlBuilder.Append($"<tr>");
            htmlBuilder.Append($"<td>{row[0]}</td>");
            htmlBuilder.Append($"<td>{row[1]}</td>");
            htmlBuilder.Append($"<td>{row[2]}</td>");
            htmlBuilder.Append($"<td>{row[3]}</td>");
            htmlBuilder.Append($"<td>{row[4]}</td>");
            htmlBuilder.Append($"<td><a href='/admin/delete-row?table={table}&id={row[0]}'>Удалить</a></td>");
            htmlBuilder.Append($"</tr>");
        }

        htmlBuilder.Append($"<form method='POST' action='/admin/add-row'>");
        htmlBuilder.Append($"<input type='hidden' name='table' value='{table}'>"); // Передаём таблицу
        htmlBuilder.Append("<input type='number' name='id' placeholder='Номер' required>");
        htmlBuilder.Append("<input type='text' name='title' placeholder='Название' required>");
        htmlBuilder.Append("<input type='text' name='image_path' placeholder='Описание' required>");
        htmlBuilder.Append("<input type='text' name='year' placeholder='Год' required>");
        htmlBuilder.Append("<input type='text' name='genre' placeholder='Жанр' required>");
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

        var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=19370;Database=postgres;Encoding=UTF8";
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            var command = new NpgsqlCommand($"DELETE FROM {table} WHERE id = @id", conn);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        return Redirect($"/admin/manage-table?table={table}");
    }

  [Post("admin/add-row")]
public IHttpResponceResult AddRow(string table, string title, string image_path, string genre, string year, int id)
{
    if (!SessionStorage.IsAdmin(Context))
    {
        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html("<h1>Доступ запрещён</h1>");
    }

    var allowedTables = new[] { "movies", "lectures" };
    if (!allowedTables.Contains(table))
    {
        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html("<h1>Недопустимая таблица</h1>");
    }

    // Проверка и конвертация года
    if (!int.TryParse(year, out int yearValue))
    {
        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html("<h1>Ошибка: Неверный формат года</h1>");
    }

    var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=19370;Database=postgres;Encoding=UTF8";

    try
    {
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();

            // Используем именованные параметры (@paramName)
            var sql = $"INSERT INTO {table} (id, title, image_path, year, genre) VALUES (@id, @title, @image_path, @year, @genre)";
            using (var command = new NpgsqlCommand(sql, conn))
            {
                // Добавляем параметры с именами
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@image_path", string.IsNullOrEmpty(image_path) ? DBNull.Value : image_path);
                command.Parameters.AddWithValue("@year", yearValue);
                command.Parameters.AddWithValue("@genre", genre);

                // Выполняем запрос
                command.ExecuteNonQuery();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка добавления записи в таблицу {table}: {ex.Message}");
        Context.Response.ContentType = "text/html; charset=utf-8";
        return Html($"<h1>Ошибка: {ex.Message}</h1>");
    }

    // Перенаправление после успешного добавления
    return Redirect($"/admin/manage-table?table={table}");
}






    // Получение данных из таблицы
    private List<List<object>> GetTableData(string table)
    {
        var rows = new List<List<object>>();

        var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=19370;Database=postgres;Encoding=UTF8";
        using (var conn = new NpgsqlConnection(connectionString))
        {
            conn.Open();
            var command = new NpgsqlCommand($"SELECT * FROM {table}", conn);
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

        return rows;
    }
}
