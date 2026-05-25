using System.Net.Http.Json;
using System.Text.Json;
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
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/contact/v1/contacts", request, cancellationToken);
        }
        catch (Exception ex) when (IsUnavailableFailure(ex, cancellationToken))
        {
            throw new BackendUnavailableException("ContactService", "ContactService did not respond while creating the contact message.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new BackendUnavailableException("ContactService", $"ContactService returned {(int)response.StatusCode} while creating a contact message.");
            }

            var created = await response.Content.ReadFromJsonAsync<ContactServiceCreateResponse>(cancellationToken)
                ?? throw new BackendUnavailableException("ContactService", "ContactService returned an empty contact response.");
            return new ContactMessageResponse(created.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), MapStatus(created.Status))
            {
                PublicReference = FormatPublicReference(created.Id)
            };
        }
    }

    private static bool IsUnavailableFailure(Exception exception, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return exception is HttpRequestException
            || exception is TimeoutException
            || exception is TaskCanceledException
            || exception.GetType().FullName == "Polly.Timeout.TimeoutRejectedException";
    }

    private static string MapStatus(JsonElement status)
    {
        if (status.ValueKind == JsonValueKind.Number && status.TryGetInt32(out var value))
        {
            return value switch
            {
                0 => "Received",
                1 => "In progress",
                2 => "Resolved",
                3 => "Closed",
                _ => value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            };
        }

        if (status.ValueKind == JsonValueKind.String)
        {
            return status.GetString() switch
            {
                "New" => "Received",
                { Length: > 0 } text => text,
                _ => "Received"
            };
        }

        return "Received";
    }

    private static string FormatPublicReference(int contactId)
    {
        return FormattableString.Invariant($"MLV-C-{contactId:000000}");
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

    public int ContactType { get; set; } = ContactTypes.General;

    public int Priority { get; set; } = Priorities.Medium;

    public List<ContactServiceFileRequest> Files { get; set; } = [];

    internal static class ContactTypes
    {
        public const int General = 0;
    }

    internal static class Priorities
    {
        public const int Medium = 1;
    }
}

internal sealed class ContactServiceFileRequest
{
    public string FileName { get; set; } = string.Empty;

    public byte[] FileContent { get; set; } = [];

    public string? ContentType { get; set; }
}

internal sealed class ContactServiceCreateResponse
{
    public int Id { get; set; }

    public JsonElement Status { get; set; }
}
