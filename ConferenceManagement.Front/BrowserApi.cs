using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace ConferenceManagement.Front;

public sealed class BrowserApi(IJSRuntime js, ILogger<BrowserApi> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public SessionInfo Session { get; private set; } = new();

    public async Task LoadSessionAsync() =>
        Session = await GetAsync<SessionInfo>("/api/Accounts/session");

    public async Task<T> GetAsync<T>(string path)
    {
        var result = await SendAsync("GET", path);
        return JsonSerializer.Deserialize<T>(result, JsonOptions)
            ?? throw new ApiException("Сервер вернул пустой ответ.");
    }

    public async Task SaveAsync(string method, string path, object? data = null) =>
        await SendAsync(method, path, data);

    private async Task<string> SendAsync(string method, string path, object? data = null)
    {
        var body = data is null ? null : JsonSerializer.Serialize(data, JsonOptions);
        ApiReply reply;
        try
        {
            reply = await js.InvokeAsync<ApiReply>("conferenceApi.send", method, path, body, Session.CsrfToken);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Browser request canceled: {Method} {Path}", method, path);
            throw new ApiException(ConnectionFailureMessage(method, "Не удалось дождаться ответа сервера."), ex);
        }
        catch (JSDisconnectedException ex)
        {
            logger.LogDebug(ex, "Browser disconnected during {Method} {Path}", method, path);
            throw new ApiException(ConnectionFailureMessage(method, "Связь с браузером прервалась."), ex);
        }

        if (reply.Status is >= 200 and < 300)
        {
            return reply.Body;
        }

        if (reply.Status == 403)
        {
            throw new ApiException("Недостаточно прав для этого действия.");
        }

        if (reply.Status == 429)
        {
            throw new ApiException("Слишком много попыток входа. Попробуйте через минуту.");
        }

        if (reply.Status >= 500)
        {
            throw new ApiException("Ошибка сервера. Проверьте LocalDB и применение миграций.");
        }

        var message = $"Запрос завершился ошибкой ({reply.Status}).";
        if (reply.Status == 0)
        {
            message = ConnectionFailureMessage(method, "Связь с сервером прервалась.");
        }
        else if (reply.Status == 404)
        {
            message = "Запись не найдена. Обновите список.";
        }
        else if (!string.IsNullOrWhiteSpace(reply.Body) && reply.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            using var json = JsonDocument.Parse(reply.Body);
            if (json.RootElement.ValueKind == JsonValueKind.String)
            {
                message = json.RootElement.GetString() ?? message;
            }
            else if (json.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty("errors", out var errors))
            {
                message = string.Join(" ", errors.EnumerateObject().SelectMany(x => x.Value.EnumerateArray())
                    .Select(x => x.GetString()));
            }
            else if (json.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty("title", out var title))
            {
                message = title.GetString() ?? message;
            }
        }
        else if (reply.ContentType.StartsWith("text/plain", StringComparison.OrdinalIgnoreCase))
        {
            message = reply.Body;
        }
        else if (reply.Status == 401)
        {
            message = "Войдите в систему, чтобы продолжить.";
        }

        throw new ApiException(message);
    }

    private static string ConnectionFailureMessage(string method, string reason) =>
        method == "GET"
            ? $"{reason} Повторите запрос или обновите страницу."
            : $"{reason} Проверьте результат операции перед повтором.";

    private sealed record ApiReply(int Status, string Body, string ContentType);
}

public sealed record SessionInfo(string? Login = null, bool IsOrganizer = false,
    Guid? PersonId = null, bool CanSetUp = false, string? CsrfToken = null);

public sealed class ApiException(string message, Exception? innerException = null) : Exception(message, innerException);
