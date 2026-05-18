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
    /// Verifies response action type and data from ChatbotService are preserved for the Web component router.
    /// </summary>
    [Fact]
    public async Task SendAsync_ServiceQuestion_PreservesSuggestedActionContract()
    {
        var client = new CapturingChatbotServiceClient
        {
            SuggestedActions =
            [
                new ChatbotSuggestedAction
                {
                    Text = "View All Services",
                    Label = "View All Services",
                    Action = "view_services",
                    Data = "all"
                },
                new ChatbotSuggestedAction
                {
                    Text = "Contact Us",
                    Label = "Contact Us",
                    Action = "contact",
                    Data = "general"
                }
            ]
        };
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "What manufacturing services do you offer?",
            Language = "en"
        }, CancellationToken.None);

        Assert.Equal(2, response.SuggestedActions.Count);
        Assert.Equal("View All Services", response.SuggestedActions[0].Label);
        Assert.Equal("view_services", response.SuggestedActions[0].Action);
        Assert.Equal("all", response.SuggestedActions[0].Data);
        Assert.Equal("Contact Us", response.SuggestedActions[1].Label);
        Assert.Equal("contact", response.SuggestedActions[1].Action);
        Assert.Equal("general", response.SuggestedActions[1].Data);
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
    /// Verifies account-specific questions require an authenticated customer context before reaching ChatbotService.
    /// </summary>
    [Fact]
    public async Task SendAsync_AccountSpecificQuestionWithoutSignedInContext_ReturnsSignInAction()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Can you check my order status and receipt?",
            Language = "en"
        }, CancellationToken.None);

        Assert.False(response.IsOutOfScope);
        Assert.Contains("identity verification", response.Content, StringComparison.OrdinalIgnoreCase);
        var action = Assert.Single(response.SuggestedActions);
        Assert.Equal("sign-in", action.Action);
        Assert.Contains("/auth/sign-in", action.Data, StringComparison.Ordinal);
        Assert.Null(client.InitiateRequest);
        Assert.Null(client.MessageRequest);
    }

    /// <summary>
    /// Verifies signed-in account questions can continue through the chatbot boundary with account context attached.
    /// </summary>
    [Fact]
    public async Task SendAsync_AccountSpecificQuestionWithSignedInContext_RoutesMessage()
    {
        var client = new CapturingChatbotServiceClient();
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            Message = "Can you check my order status and receipt?",
            CustomerContext = "Authentication: signed-in customer session\nName: Website Customer",
            Language = "en"
        }, CancellationToken.None);

        Assert.Equal(client.SessionId, response.SessionId);
        Assert.NotNull(client.MessageRequest);
        Assert.Contains("Authentication: signed-in customer session", client.MessageRequest.Content, StringComparison.Ordinal);
        Assert.Contains("Customer message:\nCan you check my order status and receipt?", client.MessageRequest.Content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies stale persisted website sessions are replaced instead of surfacing as assistant downtime.
    /// </summary>
    [Fact]
    public async Task SendAsync_StaleWebsiteSession_ReinitiatesAndRoutesMessage()
    {
        var staleSessionId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var client = new CapturingChatbotServiceClient
        {
            FirstSendException = new ChatbotSessionUnavailableException("Session not found")
        };
        var service = new CustomerChatbotService(client);

        var response = await service.SendAsync(new CustomerChatbotRequest
        {
            SessionId = staleSessionId,
            Message = "Can I get a price for 3D printing?",
            Language = "en"
        }, CancellationToken.None);

        Assert.Equal(client.SessionId, response.SessionId);
        Assert.Equal(2, client.SendAttempts);
        Assert.NotNull(client.InitiateRequest);
        Assert.NotNull(client.MessageRequest);
        Assert.Equal(client.SessionId, client.MessageRequest.SessionId);
        Assert.Equal("Can I get a price for 3D printing?", client.MessageRequest.Content);
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
    /// Verifies stale-session responses from ChatbotService are distinguishable from service downtime.
    /// </summary>
    [Fact]
    public async Task ChatbotServiceClient_BadRequestSessionNotFound_ThrowsSessionUnavailableException()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Session 00000000-0000-0000-0000-000000000001 not found" })
        })))
        {
            BaseAddress = new Uri("http://chatbot.test")
        };
        var client = new ChatbotServiceClient(httpClient);

        var exception = await Assert.ThrowsAsync<ChatbotSessionUnavailableException>(() => client.SendMessageAsync(new ChatbotSendMessageRequest
        {
            SessionId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Content = "Can you help with FDM?"
        }, CancellationToken.None));

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies expired-session responses from ChatbotService are distinguishable from service downtime.
    /// </summary>
    [Fact]
    public async Task ChatbotServiceClient_BadRequestSessionExpired_ThrowsSessionUnavailableException()
    {
        using var httpClient = new HttpClient(new StubHttpMessageHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new { error = "Session 00000000-0000-0000-0000-000000000001 has expired" })
        })))
        {
            BaseAddress = new Uri("http://chatbot.test")
        };
        var client = new ChatbotServiceClient(httpClient);

        var exception = await Assert.ThrowsAsync<ChatbotSessionUnavailableException>(() => client.SendMessageAsync(new ChatbotSendMessageRequest
        {
            SessionId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Content = "Can you help with FDM?"
        }, CancellationToken.None));

        Assert.Contains("expired", exception.Message, StringComparison.OrdinalIgnoreCase);
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

        public Exception? FirstSendException { get; init; }

        public List<ChatbotSuggestedAction> SuggestedActions { get; init; } = [];

        public int SendAttempts { get; private set; }

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
            SendAttempts++;
            if (SendAttempts == 1 && FirstSendException is not null)
            {
                return Task.FromException<ChatbotMessageResponse>(FirstSendException);
            }

            MessageRequest = request;
            return Task.FromResult(new ChatbotMessageResponse
            {
                MessageId = Guid.Parse("80adf440-8f28-4a4c-9ac9-a7f8ae9d5362"),
                Content = "PLA is the usual low-cost FDM starting point, while PETG is better for tougher utility parts.",
                Role = "assistant",
                Language = "en",
                CreatedAt = DateTimeOffset.UtcNow,
                SuggestedActions = SuggestedActions
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
