using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Contact;
using System.Text.Json;

namespace Maliev.Web.Bff.Services;

/// <summary>
/// Submits customer website contact messages to the contact boundary.
/// </summary>
public interface IContactMessageService
{
    /// <summary>
    /// Submits a customer contact message.
    /// </summary>
    /// <param name="request">The customer contact request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The contact message response.</returns>
    Task<ContactMessageResponse> SubmitAsync(ContactMessageRequest request, CancellationToken cancellationToken);
}

internal sealed class ContactMessageService(IContactServiceClient contactClient, ICountryServiceClient countryClient) : IContactMessageService
{
    private static readonly Guid LegacyThailandPlaceholderCountryId = Guid.Parse("76400000-0000-0000-0000-000000000000");

    public async Task<ContactMessageResponse> SubmitAsync(ContactMessageRequest request, CancellationToken cancellationToken)
    {
        var countryId = await ResolveCountryIdAsync(request.CountryId, cancellationToken);
        var downstream = new ContactServiceCreateRequest
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim(),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            CountryId = countryId,
            ContactType = ContactServiceCreateRequest.ContactTypes.General,
            Priority = ContactServiceCreateRequest.Priorities.Medium,
            Files = request.Files.Select(MapAttachment).ToList()
        };

        return await contactClient.CreateContactMessageAsync(downstream, cancellationToken);
    }

    private async Task<Guid> ResolveCountryIdAsync(Guid requestedCountryId, CancellationToken cancellationToken)
    {
        if (requestedCountryId != Guid.Empty && requestedCountryId != LegacyThailandPlaceholderCountryId)
        {
            return requestedCountryId;
        }

        using var response = await countryClient.GetCountryByIso2Async("TH", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BackendUnavailableException("CountryService", $"CountryService returned {(int)response.StatusCode} while resolving the default contact country.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (TryGetGuid(document.RootElement, "id", "Id") is { } countryId)
        {
            return countryId;
        }

        throw new BackendUnavailableException("CountryService", "CountryService returned a country response without an id.");
    }

    private static ContactServiceFileRequest MapAttachment(ContactAttachmentDto file)
    {
        return new ContactServiceFileRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileContent = Convert.FromBase64String(file.Base64Content)
        };
    }

    private static Guid? TryGetGuid(JsonElement root, params string[] names)
    {
        foreach (var name in names)
        {
            if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String && Guid.TryParse(value.GetString(), out var id))
            {
                return id;
            }
        }

        return null;
    }
}
