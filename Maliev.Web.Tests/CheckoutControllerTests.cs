using System.Reflection;
using System.Security.Claims;
using Maliev.Web.Bff.Controllers;
using Maliev.Web.Bff.Services;
using Maliev.Web.Shared.Commerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Maliev.Web.Tests;

/// <summary>
/// Unit tests for customer checkout controller behavior.
/// </summary>
public sealed class CheckoutControllerTests
{
    /// <summary>
    /// Verifies anonymous checkout fallback returns to the checkout route after authentication.
    /// </summary>
    [Fact]
    public async Task CreateDraftForm_AnonymousCheckout_RedirectsBackToCheckoutAfterSignIn()
    {
        var controller = new CheckoutController(
            new SignInRequiredCheckoutDraftService(),
            new FakeHostEnvironment());

        var result = await controller.CreateDraftForm(
            new CheckoutController.CheckoutDraftForm
            {
                Culture = "en-US",
                ItemsJson = "[]"
            },
            CancellationToken.None);

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/auth/sign-in?returnUrl=%2Fcheckout", redirect.Url);
    }

    /// <summary>
    /// Verifies checkout validation failures return a customer-correctable 400 response.
    /// </summary>
    [Fact]
    public async Task CreateDraft_InvalidCheckoutDetails_ReturnsBadRequestProblem()
    {
        var controller = new CheckoutController(
            new InvalidCheckoutDraftService(),
            new FakeHostEnvironment());

        var result = await controller.CreateDraft(new CheckoutDraftRequest(), CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Checkout details required", problem.Title);
        Assert.Contains("terms", problem.Detail, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class SignInRequiredCheckoutDraftService : ICheckoutDraftService
    {
        public Task<CheckoutDraftResponse> CreateDraftAsync(
            CheckoutDraftRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var exceptionType = typeof(ICheckoutDraftService).Assembly.GetType(
                "Maliev.Web.Bff.Services.CheckoutRequiresSignInException",
                throwOnError: true)!;
            var exception = (Exception)Activator.CreateInstance(
                exceptionType,
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: [],
                culture: null)!;

            return Task.FromException<CheckoutDraftResponse>(exception);
        }
    }

    private sealed class InvalidCheckoutDraftService : ICheckoutDraftService
    {
        public Task<CheckoutDraftResponse> CreateDraftAsync(
            CheckoutDraftRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            return Task.FromException<CheckoutDraftResponse>(
                new CheckoutValidationException("Accept MALIEV checkout terms before continuing checkout."));
        }
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";

        public string ApplicationName { get; set; } = "Maliev.Web.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
