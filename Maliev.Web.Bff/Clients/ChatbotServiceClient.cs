using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Maliev.Web.Bff.Services;

namespace Maliev.Web.Bff.Clients;

internal interface IChatbotServiceClient
{
    Task<ChatbotSessionResponse> InitiateSessionAsync(ChatbotInitiateSessionRequest request, CancellationToken cancellationToken);

    Task<ChatbotMessageResponse> SendMessageAsync(ChatbotSendMessageRequest request, CancellationToken cancellationToken);
}

internal sealed class ChatbotServiceClient(HttpClient httpClient) : IChatbotServiceClient
{
    private static readonly JsonSerializerOptions SnakeCaseJson = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<ChatbotSessionResponse> InitiateSessionAsync(ChatbotInitiateSessionRequest request, CancellationToken cancellationToken)
    {
        return await SendAsync<ChatbotInitiateSessionRequest, ChatbotSessionResponse>(
            "/chatbot/v1/sessions/initiate",
            request,
            "initiating a chatbot session",
            "ChatbotService returned an empty session response.",
            cancellationToken);
    }

    public async Task<ChatbotMessageResponse> SendMessageAsync(ChatbotSendMessageRequest request, CancellationToken cancellationToken)
    {
        return await SendAsync<ChatbotSendMessageRequest, ChatbotMessageResponse>(
            "/chatbot/v1/messages",
            request,
            "sending a chatbot message",
            "ChatbotService returned an empty message response.",
            cancellationToken);
    }

    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        string operation,
        string emptyResponseMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(path, request, SnakeCaseJson, cancellationToken);
            await EnsureSuccessAsync(response, operation, cancellationToken);

            return await response.Content.ReadFromJsonAsync<TResponse>(SnakeCaseJson, cancellationToken)
                ?? throw new BackendUnavailableException("ChatbotService", emptyResponseMessage);
        }
        catch (BackendUnavailableException)
        {
            throw;
        }
        catch (ChatbotRateLimitException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            throw CreateUnavailableException(operation, ex);
        }
        catch (HttpRequestException ex)
        {
            throw CreateUnavailableException(operation, ex);
        }
        catch (InvalidOperationException ex)
        {
            throw CreateUnavailableException(operation, ex);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new ChatbotRateLimitException("The MALIEV assistant is receiving too many messages. Please try again shortly.");
        }

        var detail = await ReadFailureContentAsync(response, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest && IsSessionUnavailableFailure(detail))
        {
            throw new ChatbotSessionUnavailableException($"ChatbotService rejected the session while {operation}.{detail}");
        }

        throw new BackendUnavailableException(
            "ChatbotService",
            $"ChatbotService returned {(int)response.StatusCode} while {operation}.{detail}");
    }

    private static bool IsSessionUnavailableFailure(string detail)
    {
        return detail.Contains("session", StringComparison.OrdinalIgnoreCase)
            && (detail.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || detail.Contains("expired", StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<string> ReadFailureContentAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return string.IsNullOrWhiteSpace(body) ? string.Empty : $" Response: {body}";
        }
        catch (HttpRequestException)
        {
            return string.Empty;
        }
        catch (ObjectDisposedException)
        {
            return string.Empty;
        }
    }

    private static BackendUnavailableException CreateUnavailableException(string operation, Exception exception)
    {
        return new BackendUnavailableException("ChatbotService", $"ChatbotService failed while {operation}. {exception.Message}");
    }
}

internal sealed class ChatbotInitiateSessionRequest
{
    public string Channel { get; set; } = "website";

    public string? ExternalUserId { get; set; }

    public string? Language { get; set; }
}

internal sealed class ChatbotSendMessageRequest
{
    public Guid SessionId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? Language { get; set; }
}

internal sealed class ChatbotSessionResponse
{
    public Guid SessionId { get; set; }

    public string WelcomeMessage { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public DateTimeOffset ExpiresAt { get; set; }
}

internal sealed class ChatbotMessageResponse
{
    public Guid MessageId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Role { get; set; } = "assistant";

    public string Language { get; set; } = "en";

    public DateTimeOffset CreatedAt { get; set; }

    public List<ChatbotSuggestedAction> SuggestedActions { get; set; } = [];
}

internal sealed class ChatbotSuggestedAction
{
    public string Text { get; set; } = string.Empty;

    public string? Label { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Data { get; set; }
}
