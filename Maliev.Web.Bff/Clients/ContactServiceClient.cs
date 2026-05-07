using System.Net.Http.Json;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Contact;

namespace Maliev.Web.Bff.Clients;

internal interface IContactServiceClient
{
    Task<ContactMessageResponse> CreateContactMessageAsync(ContactServiceCreateRequest request, CancellationToken cancellationToken);
}

internal sealed class ContactServiceClient(HttpClient httpClient) : IContactServiceClient
{
    public async Task<ContactMessageResponse> CreateContactMessageAsync(ContactServiceCreateRequest request, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("/contact/v1/contacts", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new BackendUnavailableException("ContactService", $"ContactService returned {(int)response.StatusCode} while creating a contact message.");
        }

        var created = await response.Content.ReadFromJsonAsync<ContactServiceCreateResponse>(cancellationToken)
            ?? throw new BackendUnavailableException("ContactService", "ContactService returned an empty contact response.");
        return new ContactMessageResponse(created.Id, created.Status);
    }
}

internal sealed class ContactServiceCreateRequest
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Company { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public Guid CountryId { get; set; }

    public string ContactType { get; set; } = "General";

    public string Priority { get; set; } = "Normal";

    public List<ContactAttachmentDto> Files { get; set; } = [];
}

internal sealed class ContactServiceCreateResponse
{
    public Guid Id { get; set; }

    public string Status { get; set; } = "Received";
}
