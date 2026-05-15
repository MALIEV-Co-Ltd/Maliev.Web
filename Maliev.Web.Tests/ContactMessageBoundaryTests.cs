using System.Net;
using System.Net.Http.Json;
using System.Text;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Contact;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for the Web BFF contact-message boundary.
/// </summary>
public sealed class ContactMessageBoundaryTests
{
    /// <summary>
    /// Verifies the public contact form payload is translated into the ContactService contract.
    /// </summary>
    [Fact]
    public async Task SubmitAsync_DefaultWebsiteContact_MapsToContactServiceContract()
    {
        var thailandId = Guid.Parse("60f7ba70-8e45-49a5-b8a3-9d78ceda60c9");
        var contactClient = new CapturingContactServiceClient();
        var countryClient = new FakeCountryServiceClient(thailandId);
        var service = new ContactMessageService(contactClient, countryClient);

        var response = await service.SubmitAsync(new ContactMessageRequest
        {
            FullName = " Website Customer ",
            Email = " customer@example.com ",
            PhoneNumber = " +66 2 000 0000 ",
            Company = " MALIEV Buyer ",
            Subject = " Manufacturing question ",
            Message = " Can MALIEV review this project? ",
            ContactType = "Website",
            Files =
            [
                new ContactAttachmentDto
                {
                    FileName = "brief.txt",
                    ContentType = "text/plain",
                    Base64Content = Convert.ToBase64String(Encoding.UTF8.GetBytes("hello"))
                }
            ]
        }, CancellationToken.None);

        Assert.Equal("42", response.MessageId);
        Assert.NotNull(contactClient.Request);
        Assert.Equal("Website Customer", contactClient.Request.FullName);
        Assert.Equal("customer@example.com", contactClient.Request.Email);
        Assert.Equal("+66 2 000 0000", contactClient.Request.PhoneNumber);
        Assert.Equal("MALIEV Buyer", contactClient.Request.Company);
        Assert.Equal("Manufacturing question", contactClient.Request.Subject);
        Assert.Equal("Can MALIEV review this project?", contactClient.Request.Message);
        Assert.Equal(thailandId, contactClient.Request.CountryId);
        Assert.Equal(ContactServiceCreateRequest.ContactTypes.General, contactClient.Request.ContactType);
        Assert.Equal(ContactServiceCreateRequest.Priorities.Medium, contactClient.Request.Priority);

        var file = Assert.Single(contactClient.Request.Files);
        Assert.Equal("brief.txt", file.FileName);
        Assert.Equal("text/plain", file.ContentType);
        Assert.True(file.FileContent.SequenceEqual(Encoding.UTF8.GetBytes("hello")));
    }

    /// <summary>
    /// Verifies the ContactService client maps ContactService's integer id and enum status into a customer-safe response.
    /// </summary>
    [Fact]
    public async Task CreateContactMessageAsync_ServiceCreatedResponse_MapsReferenceAndStatus()
    {
        string? requestBody = null;
        using var httpClient = new HttpClient(new StubHttpMessageHandler(async request =>
        {
            requestBody = request.Content is null ? null : await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = JsonContent.Create(new { id = 42, status = 0 })
            };
        }))
        {
            BaseAddress = new Uri("http://contact.test")
        };
        var client = new ContactServiceClient(httpClient);

        var response = await client.CreateContactMessageAsync(new ContactServiceCreateRequest
        {
            FullName = "Website Customer",
            Email = "customer@example.com",
            Subject = "Manufacturing question",
            Message = "Can MALIEV review this project?",
            CountryId = Guid.Parse("60f7ba70-8e45-49a5-b8a3-9d78ceda60c9")
        }, CancellationToken.None);

        Assert.Equal("42", response.MessageId);
        Assert.Equal("Received", response.Status);
        Assert.NotNull(requestBody);
        Assert.Contains("\"contactType\":0", requestBody, StringComparison.Ordinal);
        Assert.Contains("\"priority\":1", requestBody, StringComparison.Ordinal);
    }

    private sealed class CapturingContactServiceClient : IContactServiceClient
    {
        public ContactServiceCreateRequest? Request { get; private set; }

        public Task<ContactMessageResponse> CreateContactMessageAsync(ContactServiceCreateRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new ContactMessageResponse("42", "Received"));
        }
    }

    private sealed class FakeCountryServiceClient(Guid countryId) : ICountryServiceClient
    {
        public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken)
        {
            Assert.Equal("TH", iso2);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { id = countryId, iso2 = "TH", isActive = true })
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
