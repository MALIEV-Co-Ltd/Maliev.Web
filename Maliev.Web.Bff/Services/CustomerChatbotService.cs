using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Chatbot;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Routes customer website chatbot messages through the MALIEV chatbot boundary.
/// </summary>
public interface ICustomerChatbotService
{
    /// <summary>
    /// Sends a customer chatbot message.
    /// </summary>
    /// <param name="request">The customer chatbot request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The assistant response.</returns>
    Task<CustomerChatbotResponse> SendAsync(CustomerChatbotRequest request, CancellationToken cancellationToken);
}

internal sealed class CustomerChatbotService(IChatbotServiceClient chatbotClient) : ICustomerChatbotService
{
    private static readonly string[] AllowedTopicTerms =
    [
        "3d print", "3d printing", "additive", "fdm", "sla", "sls", "mjf", "resin", "filament",
        "cnc", "machining", "milling", "turning", "aluminum", "aluminium", "steel", "metal",
        "3d scan", "3d scanning", "scan", "reverse engineering", "inspection", "deviation", "cad",
        "dfm", "design", "prototype", "rapid prototyping", "manufacturing", "part", "fixture", "jig",
        "tooling", "mold", "mould", "molding", "injection", "pneumatic", "silicone", "urethane", "casting",
        "material", "pla", "petg", "abs", "asa", "nylon", "pa12", "tpu", "pp", "pc", "peek",
        "quote", "quotation", "price", "pricing", "cost", "order", "checkout", "lead time", "delivery",
        "shipping", "refund", "warranty", "file", "stl", "step", "stp", "iges", "obj", "3mf",
        "tolerance", "finish", "surface", "strength", "heat", "chemical", "contact", "phone", "address",
        "line official", "service", "shop", "machine", "pimm", "mali", "what can you do", "who are you", "your name",
        "ผลิต", "พิมพ์", "ปริ้น", "ซีเอ็นซี", "กัด", "กลึง", "สแกน", "ออกแบบ", "วัสดุ", "ต้นแบบ",
        "ชิ้นงาน", "อะไหล่", "แม่พิมพ์", "หล่อ", "ซิลิโคน", "ยูรีเทน", "เครื่องฉีด", "ลม", "ราคา",
        "ใบเสนอราคา", "สั่งซื้อ", "จัดส่ง", "คืนเงิน", "รับประกัน", "ติดต่อ", "ที่อยู่", "โทร", "ไฟล์", "มะลิ", "น้องมะลิ"
    ];

    private static readonly string[] GreetingTerms =
    [
        "hi", "hello", "hey", "good morning", "good afternoon", "good evening", "สวัสดี", "หวัดดี"
    ];

    public async Task<CustomerChatbotResponse> SendAsync(CustomerChatbotRequest request, CancellationToken cancellationToken)
    {
        var message = request.Message.Trim();
        var language = NormalizeLanguage(request.Language, message);

        if (!IsAllowedCustomerTopic(message))
        {
            return CreateOutOfScopeResponse(request.SessionId, language);
        }

        var sessionId = request.SessionId;
        if (!sessionId.HasValue || sessionId.Value == Guid.Empty)
        {
            var session = await chatbotClient.InitiateSessionAsync(new ChatbotInitiateSessionRequest
            {
                Channel = "website",
                Language = language
            }, cancellationToken);
            sessionId = session.SessionId;
            language = NormalizeLanguage(session.Language, message);
        }

        var chatbotResponse = await chatbotClient.SendMessageAsync(new ChatbotSendMessageRequest
        {
            SessionId = sessionId.Value,
            Content = message
        }, cancellationToken);

        return new CustomerChatbotResponse
        {
            SessionId = sessionId,
            MessageId = chatbotResponse.MessageId,
            Content = string.IsNullOrWhiteSpace(chatbotResponse.Content) ? FallbackAnswer(language) : chatbotResponse.Content,
            Role = string.IsNullOrWhiteSpace(chatbotResponse.Role) ? "assistant" : chatbotResponse.Role,
            Language = NormalizeLanguage(chatbotResponse.Language, message),
            CreatedAt = chatbotResponse.CreatedAt == default ? DateTimeOffset.UtcNow : chatbotResponse.CreatedAt,
            SuggestedActions = chatbotResponse.SuggestedActions
                .Select(action => new CustomerChatbotActionDto
                {
                    Label = string.IsNullOrWhiteSpace(action.Label) ? action.Text : action.Label!,
                    Action = action.Action,
                    Data = action.Data
                })
                .Where(action => !string.IsNullOrWhiteSpace(action.Label))
                .ToList()
        };
    }

    private static bool IsAllowedCustomerTopic(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        var normalized = message.Trim().ToLowerInvariant();
        if (normalized.Length <= 48 && GreetingTerms.Any(term => IsGreeting(normalized, term)))
        {
            return true;
        }

        return AllowedTopicTerms.Any(term => ContainsAllowedTerm(normalized, term));
    }

    private static bool IsGreeting(string normalizedMessage, string greeting)
    {
        return normalizedMessage.Equals(greeting, StringComparison.Ordinal)
            || normalizedMessage.StartsWith($"{greeting} ", StringComparison.Ordinal)
            || normalizedMessage.StartsWith($"{greeting},", StringComparison.Ordinal)
            || normalizedMessage.StartsWith($"{greeting}!", StringComparison.Ordinal);
    }

    private static bool ContainsAllowedTerm(string normalizedMessage, string term)
    {
        if (term.Any(ch => ch >= '\u0E00' && ch <= '\u0E7F'))
        {
            return normalizedMessage.Contains(term, StringComparison.Ordinal);
        }

        if (term.Length <= 3 && term.All(char.IsLetterOrDigit))
        {
            return normalizedMessage
                .Split([' ', '\t', '\r', '\n', '.', ',', '?', '!', '/', '\\', '-', '_', ':', ';', '(', ')', '[', ']'], StringSplitOptions.RemoveEmptyEntries)
                .Any(token => token.Equals(term, StringComparison.Ordinal));
        }

        return normalizedMessage.Contains(term, StringComparison.Ordinal);
    }

    private static string NormalizeLanguage(string? language, string message)
    {
        if (string.Equals(language, "th", StringComparison.OrdinalIgnoreCase))
        {
            return "th";
        }

        return ContainsThai(message) ? "th" : "en";
    }

    private static bool ContainsThai(string text)
    {
        return text.Any(ch => ch >= '\u0E00' && ch <= '\u0E7F');
    }

    private static CustomerChatbotResponse CreateOutOfScopeResponse(Guid? sessionId, string language)
    {
        return new CustomerChatbotResponse
        {
            SessionId = sessionId,
            Content = language == "th"
                ? "น้องมะลิช่วยตอบได้เฉพาะเรื่องบริการของ MALIEV เช่น งานผลิตชิ้นส่วน วัสดุ 3D printing, CNC, 3D scanning, งานหล่อ ใบเสนอราคา คำสั่งซื้อ และการจัดส่งค่ะ"
                : "Mali can help with MALIEV manufacturing and service-related topics only: custom parts, materials, 3D printing, CNC machining, 3D scanning, molding, quotations, orders, and delivery.",
            Role = "assistant",
            Language = language,
            IsOutOfScope = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    private static string FallbackAnswer(string language)
    {
        return language == "th"
            ? "ตอนนี้น้องมะลิยังตอบไม่ได้ครบถ้วน กรุณาถามเกี่ยวกับบริการของ MALIEV อีกครั้ง หรือติดต่อทีมงานเพื่อให้ช่วยตรวจไฟล์ค่ะ"
            : "Mali could not generate a complete answer right now. Please ask another MALIEV service question or contact the team for file review.";
    }
}
