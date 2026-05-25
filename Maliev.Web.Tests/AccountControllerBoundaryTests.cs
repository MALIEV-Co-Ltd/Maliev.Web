using System.Net;
using System.Security.Claims;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Controllers;
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
        var controller = new AccountController(new TimeoutCustomerServiceClient(), new FakeCountryServiceClient())
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
        var controller = new AccountController(new MissingCustomerServiceClient(), new FakeCountryServiceClient())
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

    private sealed class TimeoutCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken)
        {
            throw new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing.");
        }

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
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
    }

    private sealed class MissingCustomerServiceClient : ICustomerServiceClient
    {
        public Task<HttpResponseMessage> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));

        public Task<HttpResponseMessage> UpdateCustomerAsync(Guid customerId, object request, CancellationToken cancellationToken) =>
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
    }

    private sealed class FakeCountryServiceClient : ICountryServiceClient
    {
        public Task<HttpResponseMessage> GetCountryByIso2Async(string iso2, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        public Task<HttpResponseMessage> GetCountriesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}
