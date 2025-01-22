using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;
using System;
using System.IO;
using HttpServerLibrary;

namespace MyHTTPServer.EndPoints
{
    public class LectureEndpoint : BaseEndPoint
    {
        private readonly ORMContext _context;

        public LectureEndpoint()
        {
            var connectionString = "Host=localhost; Port=5432; Username=postgres; Password=19370; Database=films";
            _context = new ORMContext(connectionString);
        }

        [Get("assets/lectures/{id}")]
        public IHttpResponceResult GetLecture(int id)
        {
            Console.WriteLine($"[INFO] Вызов метода GetLecture с ID: {id}");

            try
            {
                Console.WriteLine($"[INFO] Получение данных о фильме с ID: {id}");
                var lectureDetails = _context.GetLectureDetailsById(id);

                if (lectureDetails == null)
                {
                    Console.WriteLine($"[WARN] Фильм с ID {id} не найден");
                    return Html("<h1>Фильм не найден</h1>");
                }

                Console.WriteLine($"[INFO] Данные о фильме получены: {lectureDetails.Title}");

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
                    .Replace("{{title}}", lectureDetails.Title)
                    .Replace("{{description}}", lectureDetails.Description)
                    .Replace("{{year}}", lectureDetails.Year.ToString())
                    .Replace("{{director}}", lectureDetails.Director)
                    .Replace("{{image_path}}", lectureDetails.ImagePath)
                    .Replace("{{video_url}}", lectureDetails.VideoUrl);

                Console.WriteLine($"[INFO] Шаблон успешно обработан");
                return Html(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в GetLecture: {ex.Message}");
                return Html("<h1>Произошла ошибка при обработке запроса</h1>");
            }
        }
    }
}