using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using System;
using System.IO;
using HttpServerLibrary;

namespace MyHTTPServer.EndPoints
{
    public class MovieEndpoint : BaseEndPoint
    {
        private readonly ORMContext _context;

        public MovieEndpoint()
        {
            var connectionString = "Host=localhost; Port=5432; Username=postgres; Password=19370; Database=films";
            _context = new ORMContext(connectionString);
        }

        [Get("assets/movies/{id}")]
        public IHttpResponceResult GetMovie(int id)
        {
            Console.WriteLine($"[INFO] Вызов метода GetMovie с ID: {id}");

            try
            {
                Console.WriteLine($"[INFO] Получение данных о фильме с ID: {id}");
                var movieDetails = _context.GetMovieDetailsById(id);

                if (movieDetails == null)
                {
                    Console.WriteLine($"[WARN] Фильм с ID {id} не найден");
                    return Html("<h1>Фильм не найден</h1>");
                }

                Console.WriteLine($"[INFO] Данные о фильме получены: {movieDetails.Title}");

                // Используем абсолютный путь к шаблону
                var baseDirectory = AppContext.BaseDirectory; // Корневая папка приложения
                var filePath = Path.Combine(baseDirectory, "public", "ltr", "assets", "movies", "movie.html");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[ERROR] Шаблон не найден: {filePath}");
                    return Html("<h1>Шаблон не найден</h1>");
                }

                Console.WriteLine($"[INFO] Чтение шаблона: {filePath}");
                var fileContent = File.ReadAllText(filePath);

                // Замена плейсхолдеров на реальные данные
                fileContent = fileContent
                    .Replace("{{title}}", movieDetails.Title)
                    .Replace("{{description}}", movieDetails.Description)
                    .Replace("{{year}}", movieDetails.Year.ToString())
                    .Replace("{{director}}", movieDetails.Director)
                    .Replace("{{image_path}}", movieDetails.ImagePath)
                    .Replace("{{video_url}}", movieDetails.VideoUrl);

                Console.WriteLine($"[INFO] Шаблон успешно обработан");
                return Html(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в GetMovie: {ex.Message}");
                return Html("<h1>Произошла ошибка при обработке запроса</h1>");
            }
        }
    }
}