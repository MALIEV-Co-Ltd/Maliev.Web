using Maliev.Web.Bff.Clients;
using Maliev.Web.Shared.Contact;

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

internal sealed class ContactMessageService(IContactServiceClient contactClient) : IContactMessageService
{
    public Task<ContactMessageResponse> SubmitAsync(ContactMessageRequest request, CancellationToken cancellationToken)
    {
        var downstream = new ContactServiceCreateRequest
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim(),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            CountryId = request.CountryId,
            ContactType = request.ContactType,
            Priority = "Normal",
            Files = request.Files
        };

        return contactClient.CreateContactMessageAsync(downstream, cancellationToken);
    }
}
