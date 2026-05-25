using System.Runtime.CompilerServices;

namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for the authenticated customer account pages.
/// </summary>
public sealed class AccountPageSourceTests
{
    /// <summary>
    /// Verifies account pages require a validated MALIEV customer session, not only any auth cookie.
    /// </summary>
    [Fact]
    public void AccountPagesRequireValidatedCustomerSessionPolicy()
    {
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");
        var imports = ReadRepoFile("Maliev.Web.Client", "_Imports.razor");
        var sharedPolicy = ReadRepoFile("Maliev.Web.Shared", "Security", "WebAuthorizationPolicies.cs");
        var accountPages = new[]
        {
            ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor"),
            ReadRepoFile("Maliev.Web.Client", "Pages", "AccountProfile.razor"),
            ReadRepoFile("Maliev.Web.Client", "Pages", "AccountAddresses.razor"),
            ReadRepoFile("Maliev.Web.Client", "Pages", "AccountPreferences.razor"),
            ReadRepoFile("Maliev.Web.Client", "Pages", "AccountOrders.razor")
        };

        Assert.Contains("namespace Maliev.Web.Shared.Security;", sharedPolicy);
        Assert.Contains("public const string CustomerAccount", sharedPolicy);
        Assert.Contains("@using Maliev.Web.Shared.Security", imports);
        Assert.Contains("WebAuthorizationPolicies.CustomerAccount", program);
        Assert.Contains("RequireAuthenticatedUser()", program);
        Assert.Contains("RequireClaim(\"user_type\", \"customer\")", program);
        Assert.Contains("HasValidCustomerId", program);
        Assert.All(accountPages, page =>
            Assert.Contains("@attribute [Authorize(Policy = WebAuthorizationPolicies.CustomerAccount)]", page));
    }

    /// <summary>
    /// Verifies the account overview hides internal lifecycle status and surfaces useful customer actions.
    /// </summary>
    [Fact]
    public void AccountOverviewShowsCustomerUsefulIdentityActionsWithoutInternalStatus()
    {
        var account = ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor");
        var forgotPassword = ReadRepoFile("Maliev.Web.Client", "Pages", "AuthForgotPassword.razor");
        var dtos = ReadRepoFile("Maliev.Web.Shared", "Account", "AccountDtos.cs");
        var controller = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AccountController.cs");
        var siteContent = ReadRepoFile("Maliev.Web.Client", "Content", "SiteContent.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("account-profile-card", account);
        Assert.Contains("account-avatar", account);
        Assert.Contains("account-avatar-image", account);
        Assert.Contains("AvatarInitials", account);
        Assert.Contains("_profile?.ProfileImageUrl", account);
        Assert.Contains("CustomerTierLabel", account);
        Assert.Contains("CustomerSegmentLabel", account);
        Assert.Contains("NdaStatusLabel", account);
        Assert.Contains("NdaCardCopy", account);
        Assert.Contains("QuoteEngineNdasHref", account);
        Assert.Contains("/auth/quote-engine?returnUrl=/ndas", account);
        Assert.Contains("@Text(\"NDA agreement\", \"ข้อตกลง NDA\")", account);
        Assert.Contains("@Text(\"View NDA agreement\", \"ดูข้อตกลง NDA\")", account);
        Assert.Contains("@Text(\"Customer tier\", \"ระดับลูกค้า\")", account);
        Assert.Contains("@Text(\"Change email\", \"เปลี่ยนอีเมล\")", account);
        Assert.Contains("@Text(\"Reset password\", \"รีเซ็ตรหัสผ่าน\")", account);
        Assert.Contains("ResetPasswordHref", account);
        Assert.DoesNotContain("@Text(\"Status\", \"สถานะ\")", account);
        Assert.DoesNotContain("_profile?.Status", account);

        Assert.Contains("public string Tier { get; set; } = string.Empty;", dtos);
        Assert.Contains("public string Segment { get; set; } = string.Empty;", dtos);
        Assert.Contains("public string NdaStatus { get; set; } = string.Empty;", dtos);
        Assert.Contains("public string? ProfileImageUrl { get; set; }", dtos);
        Assert.DoesNotContain("Gets or sets the customer status.", dtos);
        Assert.Contains("Tier = GetString(root, \"tier\", \"Tier\") ?? string.Empty", controller);
        Assert.Contains("Segment = GetString(root, \"segment\", \"Segment\") ?? string.Empty", controller);
        Assert.Contains("NdaStatus = GetString(root, \"ndaStatus\", \"NDAStatus\") ?? string.Empty", controller);
        Assert.Contains("ProfileImageUrl = GetString(root, \"profileImageUrl\", \"profile_image_url\", \"ProfileImageUrl\")", controller);
        Assert.DoesNotContain("Status = GetString(root, \"status\", \"Status\")", controller);

        Assert.Contains("QuoteNdasUrl => $\"{QuoteEngineUrl}/ndas\"", siteContent);

        Assert.Contains("[SupplyParameterFromQuery]\n    public string? Email", forgotPassword);
        Assert.Contains("value=\"@Email\"", forgotPassword);

        Assert.Contains(".account-profile-card", styles);
        Assert.Contains(".account-avatar", styles);
        Assert.Contains(".account-avatar-image", styles);
        Assert.Contains(".account-membership-pill", styles);
        Assert.Contains(".account-quick-actions", styles);
    }

    /// <summary>
    /// Verifies Google customer sign-in captures the Google picture URL and sends it to the customer profile contract.
    /// </summary>
    [Fact]
    public void GoogleCustomerSignInPersistsProfileImageUrl()
    {
        var authController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AuthController.cs");
        var program = ReadRepoFile("Maliev.Web.Bff", "Program.cs");

        Assert.Contains("options.ClaimActions.MapJsonKey(\"picture\", \"picture\")", program);
        Assert.Contains("GetExternalProfileImageUrl(external.Principal)", authController);
        Assert.Contains("profile_image_url = profileImageUrl", authController);
        Assert.Contains("new Claim(\"profile_image_url\", user.ProfileImageUrl)", authController);
        Assert.Contains("[JsonPropertyName(\"profile_image_url\")]", authController);
    }

    /// <summary>
    /// Verifies Web routes QuoteEngine account links through a signed session handoff instead of sending customers as anonymous users.
    /// </summary>
    [Fact]
    public void AccountNdaLinkUsesQuoteEngineSessionHandoff()
    {
        var account = ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor");
        var authController = ReadRepoFile("Maliev.Web.Bff", "Controllers", "AuthController.cs");

        Assert.Contains("QuoteEngineNdasHref", account, StringComparison.Ordinal);
        Assert.Contains("/auth/quote-engine?returnUrl=/ndas", account, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"quote-engine\")]", authController, StringComparison.Ordinal);
        Assert.Contains("CustomerSessionHandoffToken", authController, StringComparison.Ordinal);
        Assert.Contains("/auth/web-handoff", authController, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies invalid account sessions are redirected to sign-in instead of rendering a blank customer profile.
    /// </summary>
    [Fact]
    public void AccountOverviewRedirectsInvalidAccountSessionFailuresToSignIn()
    {
        var account = ReadRepoFile("Maliev.Web.Client", "Pages", "Account.razor");

        Assert.Contains("@inject NavigationManager Navigation", account);
        Assert.Contains("catch (MalievApiException ex) when (IsAccountAccessFailure(ex.StatusCode))", account);
        Assert.Contains("RedirectToSignIn();", account);
        Assert.Contains("Navigation.NavigateTo($\"/auth/sign-in?returnUrl={Uri.EscapeDataString(returnUrl)}\", forceLoad: true);", account);
        Assert.Contains("statusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden or System.Net.HttpStatusCode.NotFound", account);
    }

    /// <summary>
    /// Verifies the editable profile form uses customer-facing validation and constrained selectors.
    /// </summary>
    [Fact]
    public void AccountProfileUsesValidatedCustomerFriendlyControls()
    {
        var profile = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountProfile.razor");
        var dtos = ReadRepoFile("Maliev.Web.Shared", "Account", "AccountDtos.cs");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("<DataAnnotationsValidator />", profile);
        Assert.Contains("novalidate", profile);
        Assert.Contains("OnInvalidSubmit=\"HandleInvalidSubmit\"", profile);
        Assert.Contains("class=\"account-form-header\"", profile);
        Assert.Contains("class=\"auth-status account-form-alert\"", profile);
        Assert.Contains("class=\"auth-error account-form-alert\"", profile);
        Assert.Contains("class=\"account-profile-fields\"", profile);
        Assert.Contains("class=\"account-profile-field\"", profile);
        Assert.Contains("class=\"account-form-section\"", profile);
        Assert.Contains("@Text(\"Company details\", \"ข้อมูลบริษัท\")", profile);
        Assert.Contains("@bind-Value=\"_form.CompanyName\"", profile);
        Assert.Contains("@bind-Value=\"_form.CompanyVatNumber\"", profile);
        Assert.Contains("@bind-Value=\"_form.CompanyRegistrationNumber\"", profile);
        Assert.Contains("@bind-Value=\"_form.CompanyContactEmail\"", profile);
        Assert.Contains("@bind-Value=\"_form.CompanyContactPhone\"", profile);
        Assert.Contains("class=\"button primary account-profile-save\"", profile);
        Assert.DoesNotContain("class=\"success-message\"", profile);

        Assert.Contains("id=\"profile-email\"", profile);
        Assert.Contains("required", profile);
        Assert.Contains("autocomplete=\"email\"", profile);
        Assert.Contains("@bind-Value:event=\"oninput\"", profile);
        Assert.Contains("ValidationMessage For=\"@(() => _form.Email)\"", profile);
        Assert.Contains("@Text(\"Need to change it?\", \"ต้องการเปลี่ยนใช่ไหม\")", profile);
        Assert.Contains("@Text(\"Change email\", \"เปลี่ยนอีเมล\")", profile);

        Assert.Contains("class=\"language-dropdown\"", profile);
        Assert.Contains("class=\"language-dropdown-trigger\"", profile);
        Assert.Contains("class=\"language-dropdown-menu\"", profile);
        Assert.Contains("LanguageOptions", profile);
        Assert.Contains("ToggleLanguageDropdown", profile);
        Assert.Contains("SelectLanguage", profile);
        Assert.DoesNotContain("<InputSelect @bind-Value=\"_form.PreferredLanguage\">", profile);

        Assert.Contains("class=\"timezone-select\"", profile);
        Assert.Contains("TimezoneOptions", profile);
        Assert.Contains("<InputSelect class=\"timezone-select\" @bind-Value=\"_form.Timezone\">", profile);
        Assert.DoesNotContain("<InputText @bind-Value=\"_form.Timezone\" />", profile);

        Assert.Contains("using System.ComponentModel.DataAnnotations;", dtos);
        Assert.Contains("[Required]", dtos);
        Assert.Contains("[EmailAddress]", dtos);
        Assert.Contains("[StringLength(320)]", dtos);
        Assert.Contains("public string? CompanyName { get; set; }", dtos);
        Assert.Contains("public string? CompanyVatNumber { get; set; }", dtos);
        Assert.Contains("public string? CompanyRegistrationNumber { get; set; }", dtos);
        Assert.Contains("public string? CompanyContactEmail { get; set; }", dtos);
        Assert.Contains("public string? CompanyContactPhone { get; set; }", dtos);

        Assert.Contains(".account-form-header", styles);
        Assert.Contains(".account-form-section", styles);
        Assert.Contains(".account-form-section-head", styles);
        Assert.Contains(".account-form-alert", styles);
        Assert.Contains(".account-profile-fields", styles);
        Assert.Contains(".account-profile-save", styles);
        Assert.Contains(".field-validation-error", styles);
        Assert.Contains(".email-change-prompt", styles);
        Assert.Contains(".language-dropdown", styles);
        Assert.Contains(".language-dropdown-trigger", styles);
        Assert.Contains(".language-dropdown-item", styles);
        Assert.Contains(".timezone-select", styles);
    }

    /// <summary>
    /// Verifies the borderless shop-order empty state keeps enough space from the account navigation.
    /// </summary>
    [Fact]
    public void AccountOrdersEmptyStateKeepsSpaceFromNavigation()
    {
        var orders = ReadRepoFile("Maliev.Web.Client", "Pages", "AccountOrders.razor");
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("class=\"empty-state compact account-orders-empty\"", orders);
        Assert.Contains(".account-orders-empty", styles);
        Assert.Contains("padding-inline-start: clamp(20px, 3vw, 36px);", styles);
        Assert.Contains("@media (max-width: 960px)", styles);
        Assert.Contains("padding-inline-start: 0;", styles);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot([CallerFilePath] string sourceFilePath = "")
    {
        var sourceDirectory = Path.GetDirectoryName(sourceFilePath);
        foreach (var startDirectory in new[] { sourceDirectory, AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            if (string.IsNullOrWhiteSpace(startDirectory))
            {
                continue;
            }

            var directory = new DirectoryInfo(startDirectory);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
                {
                    return directory.FullName;
                }

                var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
                if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
                {
                    return siblingCandidate;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
