using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Shared.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.Web.Tests;

/// <summary>
/// Contract tests for customer-account BFF failure handling.
/// </summary>
public sealed class AccountControllerBoundaryTests
{
    /// <summary>
    /// Verifies CustomerService timeouts are returned as customer-safe account problem details.
    /// </summary>
    [Fact]
    public async Task GetProfile_CustomerServiceTimeout_ReturnsSanitizedUnavailableProblem()
    {
        var customerId = Guid.Parse("39543cbf-f925-4b1c-a723-2402f4f60a5f");
        var controller = new AccountController(new TimeoutCustomerServiceClient(), new FakeCountryServiceClient(), new FakeBusinessRegistryClient())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("customer_id", customerId.ToString()),
                        new Claim(ClaimTypes.Email, "customer@example.com")
                    ], "Test"))
                }
            }
        };

        var result = await controller.GetProfile(CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);
        Assert.Equal("Account service unavailable", problem.Title);
        Assert.Equal("We could not load your account details right now. Please try again in a moment.", problem.Detail);
        Assert.DoesNotContain("HttpClient.Timeout", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies stale customer cookies cannot render an empty account profile when CustomerService no longer has the customer.
    /// </summary>
    [Fact]
    public async Task GetProfile_CustomerServiceNotFound_ReturnsUnauthorizedSessionProblem()
    {
        var customerId = Guid.Parse("39543cbf-f925-4b1c-a723-2402f4f60a5f");
        var controller = new AccountController(new MissingCustomerServiceClient(), new FakeCountryServiceClient(), new FakeBusinessRegistryClient())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("customer_id", customerId.ToString()),
                        new Claim(ClaimTypes.Email, "customer@example.com")
                    ], "Test"))
                }
            }
        };

        var result = await controller.GetProfile(CancellationToken.None);

        var objectResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal("Customer session invalid", problem.Title);
        Assert.Contains("Sign in again", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies account profile saves can create a company record and link it to the signed-in customer.
    /// </summary>
    [Fact]
    public async Task UpdateProfile_NewCompanyDetails_CreatesCompanyAndLinksCustomer()
    {
        var customerId = Guid.Parse("39543cbf-f925-4b1c-a723-2402f4f60a5f");
        var companyId = Guid.Parse("857a8ccf-d4ce-41b0-90e8-771f0f8762ab");
        var customerClient = new CapturingCustomerServiceClient(customerId, companyId);
        var controller = CreateController(customerId, customerClient);

        var result = await controller.UpdateProfile(new CustomerAccountProfileUpdateRequest
        {
            FirstName = "Natthapol",
            LastName = "Vanasrivilai",
            Email = "natthapol@example.com",
            Mobile = "+66810000000",
            CompanyName = "MALIEV Co., Ltd.",
            CompanyVatNumber = "0125561001573",
            CompanyRegistrationNumber = "0105560000000",
            CompanyContactEmail = "billing@maliev.com",
            CompanyContactPhone = "+6620000000",
            PreferredLanguage = "en",
            Timezone = "Asia/Bangkok",
            Version = 12
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var profile = Assert.IsType<CustomerAccountProfileDto>(ok.Value);
        Assert.Equal(companyId, profile.CompanyId);
        Assert.Equal("MALIEV Co., Ltd.", profile.CompanyName);
        Assert.Equal("0125561001573", profile.CompanyVatNumber);
        Assert.Equal(4U, profile.CompanyVersion);

        var createdCompany = SerializeToElement(customerClient.CreatedCompanyRequest!);
        Assert.Equal("MALIEV Co., Ltd.", createdCompany.GetProperty("name").GetString());
        Assert.Equal("0125561001573", createdCompany.GetProperty("vatNumber").GetString());
        Assert.Equal("0105560000000", createdCompany.GetProperty("registrationNumber").GetString());
        Assert.Equal("billing@maliev.com", createdCompany.GetProperty("contactEmail").GetString());
        Assert.Equal("+6620000000", createdCompany.GetProperty("contactPhone").GetString());

        var updatedCustomer = SerializeToElement(customerClient.UpdatedCustomerRequest!);
        Assert.Equal(companyId, updatedCustomer.GetProperty("companyId").GetGuid());
        Assert.Equal(12U, updatedCustomer.GetProperty("xmin").GetUInt32());
    }

    /// <summary>
    /// Verifies the BFF does not create anonymous company records from tax fields without a company name.
    /// </summary>
    [Fact]
    public async Task UpdateProfile_CompanyTaxWithoutCompanyName_ReturnsBadRequest()
    {
        var customerId = Guid.Parse("39543cbf-f925-4b1c-a723-2402f4f60a5f");
        var controller = CreateController(customerId, new CapturingCustomerServiceClient(customerId, Guid.NewGuid()));

        var result = await controller.UpdateProfile(new CustomerAccountProfileUpdateRequest
        {
            FirstName = "Natthapol",
            LastName = "Vanasrivilai",
            Email = "natthapol@example.com",
            CompanyVatNumber = "0125561001573",
            PreferredLanguage = "en",
            Timezone = "Asia/Bangkok",
            Version = 12
        }, CancellationToken.None);

        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Company name required", problem.Title);
    }

    private static AccountController CreateController(Guid customerId, ICustomerServiceClient customerClient)
    {
        return new AccountController(customerClient, new FakeCountryServiceClient(), new FakeBusinessRegistryClient())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("customer_id", customerId.ToString()),
                        new Claim(ClaimTypes.Email, "customer@example.com")
                    ], "Test"))
                }
            }
        };
    }

    private static JsonElement SerializeToElement(object value)
    {
        return JsonSerializer.SerializeToElement(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static HttpResponseMessage JsonResponse(object value, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(value)
        };
    }

    private sealed class CapturingCustomerServiceClient(Guid customerId, Guid companyId) : ICustomerServiceClient
    {
        public object? CreatedCompanyRequest { get; private set; }

        public object? UpdatedCustomerRequest { get; private set; }

        public Task<HttpResponseMessage> GetCustomerAsync(Guid requestedCustomerId, CancellationToken cancellationToken)
        {
            Assert.Equal(customerId, requestedCustomerId);
            return Task.FromResult(JsonResponse(new
            {
                id = customerId,
                firstName = "Natthapol",
                lastName = "Vanasrivilai",
                email = "natthapol@example.com",
                mobile = "+66810000000",
                segment = "Retail",
                tier = "Bronze",
                preferredLanguage = "en",
                timezone = "Asia/Bangkok",
                xmin = 12U
            }));
        }

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid requestedCustomerId, object request, CancellationToken cancellationToken)
        {
            Assert.Equal(customerId, requestedCustomerId);
            UpdatedCustomerRequest = request;
            return Task.FromResult(JsonResponse(new
            {
                id = customerId,
                firstName = "Natthapol",
                lastName = "Vanasrivilai",
                email = "natthapol@example.com",
                mobile = "+66810000000",
                companyId,
                companyName = "MALIEV Co., Ltd.",
                segment = "Retail",
                tier = "Bronze",
                preferredLanguage = "en",
                timezone = "Asia/Bangkok",
                xmin = 13U
            }));
        }

        public Task<HttpResponseMessage> GetCompanyAsync(Guid requestedCompanyId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken)
        {
            CreatedCompanyRequest = request;
            return Task.FromResult(JsonResponse(new
            {
                id = companyId,
                name = "MALIEV Co., Ltd.",
                vatNumber = "0125561001573",
                registrationNumber = "0105560000000",
                contactEmail = "billing@maliev.com",
                contactPhone = "+6620000000",
                segment = "Retail",
                tier = "Bronze",
                xmin = 4U
            }, HttpStatusCode.Created));
        }

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid requestedCompanyId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid requestedCustomerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class TimeoutCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing.");
        }

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class MissingCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCompanyAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCompanyAsync(Guid companyId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> RegisterCustomerAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> CreateCustomerAddressAsync(object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> UpdateCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> DeleteCustomerAddressAsync(Guid addressId, object request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCustomerByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class FakeCountryServiceClient : ICountryServiceClient
    {
        public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    private sealed class FakeBusinessRegistryClient : IBusinessRegistryClient
    {
        public Task<IReadOnlyList<BusinessRegistryCompanyDto>> SearchAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<BusinessRegistryCompanyDto>>([]);
    }
}
