using System.Net;
using HttpServerLibrary.Configuration;


namespace HttpServerLibrary.Handlers;

/// <summary>
/// Handler for serving static files from a directory
/// </summary>
public class StaticFilesHandler : Handler
{
    private readonly string _staticDirectoryPath =
        $"{Directory.GetCurrentDirectory()}/{AppConfig.StaticDirectoryPath}";

    public override async void HandleRequest(HttpRequestContext context)
    {
        Console.WriteLine("Handling request");

        bool isGet = context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
        string[]? arr = context.Request.Url?.AbsolutePath.Split('.');
        bool isFile = arr?.Length >= 2;

        if (isGet && isFile)
        {
            try
            {
                string? relativePath = context.Request.Url?.AbsolutePath.Trim('/');
                string filePath = Path.Combine(_staticDirectoryPath, 
                    string.IsNullOrEmpty(relativePath) ? "index.html" : relativePath);

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"File not found: {filePath}");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    byte[] notFoundMessage = System.Text.Encoding.UTF8.GetBytes("404 Not Found");
                    await context.Response.OutputStream.WriteAsync(notFoundMessage, 0, notFoundMessage.Length);
                    context.Response.OutputStream.Close();
                    return;
                }

                byte[] responseFile = File.ReadAllBytes(filePath);
                context.Response.ContentType = GetContentType(Path.GetExtension(filePath));
                context.Response.ContentLength64 = responseFile.Length;

                await context.Response.OutputStream.WriteAsync(responseFile, 0, responseFile.Length);
                context.Response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling static file: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                byte[] errorMessage = System.Text.Encoding.UTF8.GetBytes("500 Internal Server Error");
                await context.Response.OutputStream.WriteAsync(errorMessage, 0, errorMessage.Length);
                context.Response.OutputStream.Close();
            }
        }
        else if (Successor != null)
        {
            Console.WriteLine("Switching to next handler");
            Successor.HandleRequest(context);
        }
    }

    private static string GetContentType(string? extension)
    {
        if (extension == null)
        {
            throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
        }

        return extension.ToLower() switch
        {
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}
