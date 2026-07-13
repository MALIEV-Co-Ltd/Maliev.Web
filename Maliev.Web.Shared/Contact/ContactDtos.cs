using System.ComponentModel.DataAnnotations;

namespace Maliev.Web.Shared.Contact;

/// <summary>
/// Customer-facing contact form request.
/// </summary>
public sealed class ContactMessageRequest : IValidatableObject
{
    /// <summary>Maximum number of files accepted by one contact message.</summary>
    public const int MaxAttachmentCount = 5;

    /// <summary>Maximum decoded size of one contact attachment.</summary>
    public const int MaxAttachmentBytes = 10 * 1024 * 1024;

    /// <summary>Maximum base64 text length for one attachment.</summary>
    public const int MaxAttachmentBase64Length = ((MaxAttachmentBytes + 2) / 3) * 4;
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
    [MaxLength(MaxAttachmentCount)]
    public List<ContactAttachmentDto> Files { get; set; } = [];

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        long totalBytes = 0;
        if (Files is null)
        {
            yield return new ValidationResult("Attachments must be a collection.", [nameof(Files)]);
            yield break;
        }

        foreach (var file in Files)
        {
            if (file is null || file.Base64Content is null)
            {
                yield return new ValidationResult("Attachment content is required.", [nameof(Files)]);
                continue;
            }

            long decodedLength;
            try
            {
                decodedLength = Convert.FromBase64String(file.Base64Content).LongLength;
            }
            catch (FormatException)
            {
                decodedLength = -1;
            }

            if (decodedLength < 0)
            {
                yield return new ValidationResult("Attachment content must be valid base64.", [nameof(Files)]);
                continue;
            }

            if (decodedLength > MaxAttachmentBytes)
            {
                yield return new ValidationResult("Each attachment must be 10 MB or smaller.", [nameof(Files)]);
                continue;
            }

            totalBytes += decodedLength;
        }

        if (totalBytes > (long)MaxAttachmentCount * MaxAttachmentBytes)
        {
            yield return new ValidationResult("The total attachment size exceeds 50 MB.", [nameof(Files)]);
        }
    }
}

/// <summary>
/// Customer-facing contact attachment payload.
/// </summary>
public sealed class ContactAttachmentDto
{
    /// <summary>Gets or sets the file name.</summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the content type.</summary>
    [MaxLength(128)]
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Gets or sets base64 encoded file content.</summary>
    [Required]
    [MaxLength(ContactMessageRequest.MaxAttachmentBase64Length)]
    public string Base64Content { get; set; } = string.Empty;
}

/// <summary>
/// Contact form submission response.
/// </summary>
public sealed record ContactMessageResponse(string MessageId, string Status)
{
    /// <summary>Gets the customer-safe contact request reference.</summary>
    public string PublicReference { get; init; } = string.Empty;
}
