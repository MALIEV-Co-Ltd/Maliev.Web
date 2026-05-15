using System.ComponentModel.DataAnnotations;

namespace Maliev.Web.Shared.Contact;

/// <summary>
/// Customer-facing contact form request.
/// </summary>
public sealed class ContactMessageRequest
{
    /// <summary>Gets or sets the sender name.</summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the sender email.</summary>
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional phone number.</summary>
    [MaxLength(80)]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional company name.</summary>
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    /// <summary>Gets or sets the message subject.</summary>
    [Required]
    [MaxLength(240)]
    public string Subject { get; set; } = string.Empty;

    /// <summary>Gets or sets the message body.</summary>
    [Required]
    [MaxLength(5000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the contact type.</summary>
    [MaxLength(80)]
    public string ContactType { get; set; } = "General";

    /// <summary>Gets or sets the selected country id.</summary>
    public Guid CountryId { get; set; }

    /// <summary>Gets or sets uploaded contact attachments.</summary>
    public List<ContactAttachmentDto> Files { get; set; } = [];
}

/// <summary>
/// Customer-facing contact attachment payload.
/// </summary>
public sealed class ContactAttachmentDto
{
    /// <summary>Gets or sets the file name.</summary>
    [Required]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the content type.</summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Gets or sets base64 encoded file content.</summary>
    [Required]
    public string Base64Content { get; set; } = string.Empty;
}

/// <summary>
/// Contact form submission response.
/// </summary>
public sealed record ContactMessageResponse(string MessageId, string Status);
