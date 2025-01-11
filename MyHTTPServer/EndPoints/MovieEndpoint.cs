using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;

namespace MyHTTPServer.EndPoints
{
    
    public class MovieEndpoint : BaseEndPoint
    {
        private readonly ORMContext _context;

        // Конструктор с параметром для передачи ORMContext
        public MovieEndpoint()
        {
            var connectionString = "Host=localhost; Port=5432; Username=postgres; Password=19370; Database=postgres"; // Укажите вашу строку подключения
            _context = new ORMContext(connectionString);  // Создаем ORMContext с параметром
        }

        [Get("movie")]
        public IHttpResponceResult GetMovie(int id) // Получаем id из запроса
        {
            var movieDetails = _context.GetMovieDetailsById(id); // Используем переданный id
            if (movieDetails == null)
            {
                return Html("<h1>Фильм не найден</h1>");
            }


            var filePath = @"Templates/Pages/Movie/movie.html";
            var fileContent = File.ReadAllText(filePath);

            // Вставляем данные в шаблон
            fileContent = fileContent.Replace("{{title}}", movieDetails.Title)
                .Replace("{{description}}", movieDetails.Description)
                .Replace("{{release_year}}", movieDetails.ReleaseYear.ToString())
                .Replace("{{genre}}", movieDetails.Genre);

            // Форматируем список актеров
            var actorsHtml = string.Empty;
            foreach (var actor in movieDetails.Actors)
            {
                actorsHtml += $"<div>{actor.Name} - {actor.Role}</div>";
            }

            // Если актеры не найдены, можно вывести сообщение или оставить поле пустым
            if (string.IsNullOrEmpty(actorsHtml))
            {
                actorsHtml = "<p>Нет информации об актерах.</p>";
            }

            // Вставляем актеров в шаблон
            fileContent = fileContent.Replace("{{actors}}", actorsHtml);
            

            return Html(fileContent);
        }

    }

}