using System.Text;
using HttpServerLibrary;
using HttpServerLibrary.Attributes;
using HttpServerLibrary.HttpResponce;

namespace MyHTTPServer.EndPoints;

public class indexLectureEndpoint : BaseEndPoint
{
    [Get("index_lectures")]
    public IHttpResponceResult GetIndex()
    {
        // Чтение основного HTML-шаблона
        var filePath = @"Templates/Pages/Dashboard/index_lectures.html";
        var fileContent = File.ReadAllText(filePath);

        // Получение данных из базы
        var ormContext = new ORMContext("Host=localhost; Port=5432; Username=postgres; Password=19370; Database=postgres");
        var moviesList = ormContext.GetAllMovies();
        var lecturesList = ormContext.GetAllLectures();

        // Генерация HTML для фильмов
        var moviesHtml = new StringBuilder();
        foreach (var movie in moviesList)
        {
            moviesHtml.Append($@"
                <div class='t-feed__grid-col t-col t-col_4'>
                    <div class='movie-card'>
                        <a href='/assets/movies/{movie.Id}'>
                            <img src='{movie.ImagePath}' alt='{movie.Title}' width = '268' height = '148.88'/>
                        </a>
                        <div class='series-card-description'>
                            <h3>{movie.Title}</h3>
                            <h6>{movie.Genre} | {movie.Year}</h6>
                        </div>
                    </div>
                </div>
            ");
        }

        // Генерация HTML для лекций
        var lecturesHtml = new StringBuilder();
        foreach (var lecture in lecturesList)
        {
            lecturesHtml.Append($@"
                <div class='t-feed__grid-col t-col t-col_4'>
                    <div class='lecture-card'>
                        <a href='/assets/lectures/{lecture.Id}'>
                            <img src='{lecture.ImagePath}' alt='{lecture.Title}' width = '268' height = '148.88'/>
                        </a>
                        <div class='series-card-description'>
                            <h3>{lecture.Title}</h3>
                            <h6>{lecture.Genre} | {lecture.Year}</h6>
                        </div>
                    </div>
                </div>
            ");
        }

        // Замена плейсхолдеров в шаблоне
        fileContent = fileContent.Replace("{{MOVIES_CARDS}}", moviesHtml.ToString());
        fileContent = fileContent.Replace("{{LECTURES_CARDS}}", lecturesHtml.ToString());

        return Html(fileContent);
    }
}