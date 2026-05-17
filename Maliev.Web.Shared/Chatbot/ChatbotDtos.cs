using System.ComponentModel.DataAnnotations;

namespace Maliev.Web.Shared.Chatbot;

/// <summary>
/// Customer-facing chatbot message request.
/// </summary>
public sealed class CustomerChatbotRequest
{
    /// <summary>Gets or sets the existing conversation session ID, when one is active.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>Gets or sets the customer message.</summary>
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the preferred language code, either en or th.</summary>
    [RegularExpression("^(en|th)?$", ErrorMessage = "Language must be 'en' or 'th'.")]
    public string? Language { get; set; }
}

/// <summary>
/// Customer-facing chatbot response.
/// </summary>
public sealed class CustomerChatbotResponse
{
    /// <summary>Gets or sets the active conversation session ID.</summary>
    public Guid? SessionId { get; set; }

    /// <summary>Gets or sets the downstream assistant message ID, when available.</summary>
    public Guid? MessageId { get; set; }

    /// <summary>Gets or sets the assistant response content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the response role.</summary>
    public string Role { get; set; } = "assistant";

    /// <summary>Gets or sets the response language code.</summary>
    public string Language { get; set; } = "en";

    /// <summary>Gets or sets whether the BFF rejected the message as outside MALIEV service topics.</summary>
    public bool IsOutOfScope { get; set; }

    /// <summary>Gets or sets the response creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets suggested follow-up actions returned by the chatbot.</summary>
    public List<CustomerChatbotActionDto> SuggestedActions { get; set; } = [];
}

/// <summary>
/// Customer-facing chatbot suggested action.
/// </summary>
public sealed class CustomerChatbotActionDto
{
    /// <summary>Gets or sets the visible action label.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Gets or sets the action type.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Gets or sets optional action data.</summary>
    public string? Data { get; set; }
}
