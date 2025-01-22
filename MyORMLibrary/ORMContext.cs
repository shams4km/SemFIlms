using Npgsql;
using System;
using System.Collections.Generic;

public class ORMContext
{
    private readonly string _connectionString;

    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    // Получить все фильмы
    public IEnumerable<Movie> GetAllMovies()
    {
        var result = new List<Movie>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT id, title, year, director, image_path FROM movies";
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
                        Director = reader.GetString(3),
                        ImagePath = reader.GetString(4)
                    };
                    result.Add(movie);
                }
            }
        }

        return result;
    }

    // Получить все лекции
    public IEnumerable<Lecture> GetAllLectures()
    {
        var result = new List<Lecture>();

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();
            var query = "SELECT id, title, year, director, image_path FROM lectures";
            var command = new NpgsqlCommand(query, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var lecture = new Lecture
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Year = reader.GetInt32(2),
                        Director = reader.GetString(3),
                        ImagePath = reader.GetString(4)
                    };
                    result.Add(lecture);
                }
            }
        }

        return result;
    }

    // Получить детали фильма по ID
    public MovieDetails GetMovieDetailsById(int movieId)
    {
        MovieDetails movieDetails = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            // Запрос для получения основной информации о фильме и его деталях
            var query = @"
                SELECT 
                    m.id, 
                    m.title, 
                    m.year, 
                    m.director, 
                    m.image_path, 
                    md.description, 
                    md.video_url
                FROM 
                    movies m
                JOIN 
                    movie_details md ON m.id = md.movie_id
                WHERE 
                    m.id = @id;";

            var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", movieId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    movieDetails = new MovieDetails
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Year = reader.GetInt32(2),
                        Director = reader.GetString(3),
                        ImagePath = reader.GetString(4),
                        Description = reader.GetString(5),
                        VideoUrl = reader.GetString(6)
                    };
                }
                else
                {
                    Console.WriteLine($"Фильм с id {movieId} не найден.");
                }
            }
        }

        return movieDetails;
    }

    // Получить детали лекции по ID
    public LectureDetails GetLectureDetailsById(int lectureId)
    {
        LectureDetails lectureDetails = null;

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            connection.Open();

            // Запрос для получения основной информации о лекции и её деталях
            var query = @"
                SELECT 
                    l.id, 
                    l.title, 
                    l.year, 
                    l.director, 
                    l.image_path, 
                    ld.description, 
                    ld.video_url
                FROM 
                    lectures l
                JOIN 
                    lecture_details ld ON l.id = ld.lecture_id
                WHERE 
                    l.id = @id;";

            var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", lectureId);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    lectureDetails = new LectureDetails
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Year = reader.GetInt32(2),
                        Director = reader.GetString(3),
                        ImagePath = reader.GetString(4),
                        Description = reader.GetString(5),
                        VideoUrl = reader.GetString(6)
                    };
                }
                else
                {
                    Console.WriteLine($"Лекция с id {lectureId} не найдена.");
                }
            }
        }

        return lectureDetails;
    }
}

// Модель для фильма
public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Director { get; set; }
    public string ImagePath { get; set; }
}

// Модель для деталей фильма
public class MovieDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Director { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
    public string VideoUrl { get; set; }
}

// Модель для лекции
public class Lecture
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Director { get; set; }
    public string ImagePath { get; set; }
}

// Модель для деталей лекции
public class LectureDetails
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int Year { get; set; }
    public string Director { get; set; }
    public string ImagePath { get; set; }
    public string Description { get; set; }
    public string VideoUrl { get; set; }
}