using Npgsql;

public class ORMContext
{
    private readonly string _connectionString;

    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    

    public IEnumerable<Movie> GetAllMovies()
    {
        var result = new List<Movie>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT id, title, year, genre, image_path FROM movies";
            var command = new NpgsqlCommand(query, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var movie = new Movie
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Year = reader.GetInt32(2),
                        Genre = reader.GetString(3),
                        ImagePath = reader.GetString(4)
                    };
                    result.Add(movie);
                }
            }
        }

        return result;
    }

    // Метод для получения всех фильмов из категории Lectures
    public IEnumerable<Movie> GetAllLectures()
    {
        var result = new List<Movie>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT id, title, year, genre, image_path FROM lectures";
            var command = new NpgsqlCommand(query, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var movie = new Movie
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Year = reader.GetInt32(2),
                        Genre = reader.GetString(3),
                        ImagePath = reader.GetString(4)
                    };
                    result.Add(movie);
                }
            }
        }

        return result;
    }

    

    
    
    public MovieDetails GetMovieDetailsById(int movieId)
{
    MovieDetails movieDetails = null;

    using (var connection = new NpgsqlConnection(_connectionString))
    {
        connection.Open();
        
        // Запрос для получения основной информации о фильме
        var query = "SELECT title, description, release_year, genre, image_path FROM movies_details WHERE id = @id";
        var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@id", movieId);

        using (var reader = command.ExecuteReader())
        {
            if (reader.Read())
            {
                movieDetails = new MovieDetails
                {
                    Title = reader.GetString(0),
                    Description = reader.GetString(1),
                    ReleaseYear = reader.GetInt32(2),
                    Genre = reader.GetString(3),
                    ImagePath = reader.GetString(4),
                    Actors = new List<Actor>()
                };
            }
        }
    }

    // Получаем актеров для фильма
    if (movieDetails != null)
    {
        var actorsQuery = "SELECT name, role FROM actors WHERE movie_id = @Id";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand(actorsQuery, connection);
            command.Parameters.AddWithValue("@Id", movieId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var actor = new Actor
                    {
                        Name = reader.GetString(0),
                        Role = reader.GetString(1)
                    };
                    movieDetails.Actors.Add(actor);
                    Console.WriteLine($"Actor: {actor.Name}, Role: {actor.Role}");
                }
            }
        }

        if (movieDetails.Actors.Count == 0)
        {
            Console.WriteLine($"Нет актеров для фильма с id {movieId}");
        }
    }



    
    return movieDetails;  // Если фильма нет, вернется null
}
}

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Genre { get; set; }
    public string ImagePath { get; set; }
}

public class MovieDetails
{
    public string Title { get; set; }
    public string Description { get; set; }
    public int ReleaseYear { get; set; }
    public string Genre { get; set; }
    public string ImagePath { get; set; }
    public string TrailerUrl { get; set; }
    public List<Actor> Actors { get; set; }
}

public class Actor
{
    public string Name { get; set; }
    public string Role { get; set; }
}