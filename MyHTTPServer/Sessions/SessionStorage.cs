using HttpServerLibrary;

namespace MyHTTPServer.Sessions;

public static class SessionStorage
{
    private static readonly Dictionary<string, string> _sessions = new Dictionary<string, string>();

    // Сохранение токена и его соответствующего ID пользователя
    public static void SaveSession(string token, string userId)
    {
        _sessions[token] = userId;
        Console.WriteLine($"[SessionStorage] Session saved: Token = {token}, UserId = {userId}");
    }

    // Генерация нового токена
    public static string GenerateNewToken()
    {
        return Guid.NewGuid().ToString();  // Генерация уникального токена
    }

    // Проверка токена
    public static bool ValidateToken(string token)
    {
        var isValid = _sessions.ContainsKey(token);
        Console.WriteLine($"[SessionStorage] Token validation: Token = {token}, IsValid = {isValid}");
        return isValid;
    }

    // Получение ID пользователя по токену
    public static string GetUserId(string token)
    {
        if (_sessions.TryGetValue(token, out var userId))
        {
            Console.WriteLine($"[SessionStorage] UserId retrieved: Token = {token}, UserId = {userId}");
            return userId;
        }

        Console.WriteLine($"[SessionStorage] UserId retrieval failed: Token = {token}");
        return null;
    }

    // Проверка авторизации
    public static bool IsAuthorized(HttpRequestContext context)
    {
        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Name == "session-token");
        if (cookie != null)
        {
            Console.WriteLine($"[SessionStorage] Cookie found: Name = session-token, Value = {cookie.Value}");
            return ValidateToken(cookie.Value);
        }

        Console.WriteLine("[SessionStorage] No session-token cookie found in request");
        return false;
    }

    // Проверка прав администратора
    public static bool IsAdmin(HttpRequestContext context)
    {
        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Name == "session-token");
        if (cookie != null)
        {
            var userId = GetUserId(cookie.Value);
            return userId == "admin";  // Проверка, является ли пользователь администратором
        }

        return false;
    }
    
    public static void ClearSession()
    {
        _sessions.Clear();  // Очищаем все сессии
        Console.WriteLine("[SessionStorage] Все сессии очищены");
    }


}
