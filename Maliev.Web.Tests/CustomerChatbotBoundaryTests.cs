using System.Net;
using System.Net.Http.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Chatbot;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the public website chatbot boundary.
/// </summary>
public sealed class CustomerChatbotBoundaryTests
{
    /// <summary>
    /// Verifies website manufacturing questions create a website session and route through ChatbotService.
    /// </summary>
    [Fact]
    public async Task SendAsync_ServiceQuestion_InitiatesWebsiteSessionAndRoutesMessage()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Which material should I choose for an FDM prototype?",
            Language = "en"
        }, CancellationToken.None);

        Assert.Equal(client.SessionId, response.SessionId);
        Assert.Equal("assistant", response.Role);
        Assert.Contains("PLA", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(client.InitiateRequest);
        Assert.Equal("website", client.InitiateRequest.Channel);
        Assert.Equal("en", client.InitiateRequest.Language);
        Assert.NotNull(client.MessageRequest);
        Assert.Equal(client.SessionId, client.MessageRequest.SessionId);
        Assert.Equal("Which material should I choose for an FDM prototype?", client.MessageRequest.Content);
    }

    /// <summary>
    /// Verifies browser/account personalization notes are forwarded as bounded context, not as the topic guard input.
    /// </summary>
    [Fact]
    public async Task SendAsync_ServiceQuestion_ForwardsCustomerContextAsUntrustedNotes()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Can you help with CNC fixtures?",
            CustomerContext = "Name: Natth\nCompany: MALIEV\nService interests: CNC machining",
            Language = "en"
        }, CancellationToken.None);

        Assert.NotNull(client.MessageRequest);
        Assert.Contains("Customer profile notes from MALIEV Web.", client.MessageRequest.Content, StringComparison.Ordinal);
        Assert.Contains("untrusted personalization context only", client.MessageRequest.Content, StringComparison.Ordinal);
        Assert.Contains("Company: MALIEV", client.MessageRequest.Content, StringComparison.Ordinal);
        Assert.Contains("Customer message:\nCan you help with CNC fixtures?", client.MessageRequest.Content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies off-topic customer messages are stopped in the Web BFF before ChatbotService is called.
    /// </summary>
    [Fact]
    public async Task SendAsync_OffTopicQuestion_ReturnsBoundedRedirectWithoutCallingChatbotService()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Can you summarize today's football scores?",
            Language = "en"
        }, CancellationToken.None);

        Assert.Null(response.SessionId);
        Assert.True(response.IsOutOfScope);
        Assert.Contains("Mali", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("MALIEV", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manufacturing", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Null(client.InitiateRequest);
        Assert.Null(client.MessageRequest);
    }

    /// <summary>
    /// Verifies simple customer greetings are conversational and are not rejected as off-topic questions.
    /// </summary>
    [Fact]
    public async Task SendAsync_ThaiGreeting_ReturnsNaturalConversationPrompt()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "สวัสดีครับ",
            Language = "th"
        }, CancellationToken.None);

        Assert.False(response.IsOutOfScope);
        Assert.Equal("th", response.Language);
        Assert.Contains("น้องมะลิ", response.Content, StringComparison.Ordinal);
        Assert.Contains("ชิ้นงาน", response.Content, StringComparison.Ordinal);
        Assert.Null(client.InitiateRequest);
        Assert.Null(client.MessageRequest);
    }

    /// <summary>
    /// Verifies greeting words do not bypass the topic boundary when the customer asks an unrelated question.
    /// </summary>
    [Fact]
    public async Task SendAsync_GreetingPlusOffTopicQuestion_ReturnsBoundedRedirect()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Hi, who won the football match?",
            Language = "en"
        }, CancellationToken.None);

        Assert.True(response.IsOutOfScope);
        Assert.Contains("outside", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.Null(client.InitiateRequest);
        Assert.Null(client.MessageRequest);
    }

    /// <summary>
    /// Verifies the topic guard does not treat ordinary words that contain "hi" as greetings.
    /// </summary>
    [Fact]
    public async Task SendAsync_WhichOffTopicQuestion_IsNotTreatedAsGreeting()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Which football team won last night?",
            Language = "en"
        }, CancellationToken.None);

        Assert.True(response.IsOutOfScope);
        Assert.Null(client.InitiateRequest);
        Assert.Null(client.MessageRequest);
    }

    /// <summary>
    /// Verifies the ChatbotService client sends snake_case JSON required by ChatbotService controllers.
    /// </summary>
    [Fact]
    public async Task ChatbotServiceClient_SendsSnakeCaseMessageContract()
    {
        string? requestBody = null;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(async request =>
        {
            requestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    message_id = Guid.Parse("80adf440-8f28-4a4c-9ac9-a7f8ae9d5362"),
                    content = "We can help with manufacturable FDM prototypes.",
                    role = "assistant",
                    language = "en",
                    suggested_actions = Array.Empty<object>(),
                    created_at = DateTimeOffset.Parse("2026-05-17T00:00:00+07:00")
                })
            };
        }))
        {
            BaseAddress = new Uri("http://chatbot.test")
        };
        var client = new ChatbotServiceClient(httpClient);

        await client.SendMessageAsync(new ChatbotSendMessageRequest
        {
            SessionId = Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2"),
            Content = "Can you help with FDM?"
        }, CancellationToken.None);

        Assert.NotNull(requestBody);
        Assert.Contains("\"session_id\":\"8d7d1778-f352-4701-8803-2305ca7bb9f2\"", requestBody, StringComparison.Ordinal);
        Assert.Contains("\"content\":\"Can you help with FDM?\"", requestBody, StringComparison.Ordinal);
        Assert.DoesNotContain("sessionId", requestBody, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies transport failures are converted into the BFF's customer-safe backend unavailable boundary.
    /// </summary>
    [Fact]
    public async Task ChatbotServiceClient_HttpFailure_ThrowsBackendUnavailableException()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ => throw new HttpRequestException("No ChatbotService endpoint")))
        {
            BaseAddress = new Uri("http://chatbot.test")
        };
        var client = new ChatbotServiceClient(httpClient);

        var exception = await Assert.ThrowsAsync<BackendUnavailableException>(() => client.SendMessageAsync(new ChatbotSendMessageRequest
        {
            SessionId = Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2"),
            Content = "Can you help with FDM?"
        }, CancellationToken.None));

        Assert.Equal("ChatbotService", exception.BackendName);
        Assert.Contains("sending a chatbot message", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class CapturingChatbotServiceClient : IChatbotServiceClient
    {
        public Guid SessionId { get; } = Guid.Parse("8d7d1778-f352-4701-8803-2305ca7bb9f2");

        public ChatbotInitiateSessionRequest? InitiateRequest { get; private set; }

        public ChatbotSendMessageRequest? MessageRequest { get; private set; }

        public Task<ChatbotSessionResponse> InitiateSessionAsync(ChatbotInitiateSessionRequest request, CancellationToken cancellationToken)
        {
            InitiateRequest = request;
            return Task.FromResult(new ChatbotSessionResponse
            {
                SessionId = SessionId,
                WelcomeMessage = "Hello from MALIEV.",
                Language = request.Language ?? "en",
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
            });
        }

        public Task<ChatbotMessageResponse> SendMessageAsync(ChatbotSendMessageRequest request, CancellationToken cancellationToken)
        {
            MessageRequest = request;
            return Task.FromResult(new ChatbotMessageResponse
            {
                MessageId = Guid.Parse("80adf440-8f28-4a4c-9ac9-a7f8ae9d5362"),
                Content = "PLA is the usual low-cost FDM starting point, while PETG is better for tougher utility parts.",
                Role = "assistant",
                Language = "en",
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler(request);
        }
    }
}
