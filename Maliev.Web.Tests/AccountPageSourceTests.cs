namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for the authenticated customer account pages.
/// </summary>
public sealed class AccountPageSourceTests
{
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
        Assert.Contains("AvatarInitials", account);
        Assert.Contains("CustomerTierLabel", account);
        Assert.Contains("CustomerSegmentLabel", account);
        Assert.Contains("NdaStatusLabel", account);
        Assert.Contains("NdaCardCopy", account);
        Assert.Contains("SiteContent.QuoteNdasUrl", account);
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
        Assert.DoesNotContain("Gets or sets the customer status.", dtos);
        Assert.Contains("Tier = GetString(root, \"tier\", \"Tier\") ?? string.Empty", controller);
        Assert.Contains("Segment = GetString(root, \"segment\", \"Segment\") ?? string.Empty", controller);
        Assert.Contains("NdaStatus = GetString(root, \"ndaStatus\", \"NDAStatus\") ?? string.Empty", controller);
        Assert.DoesNotContain("Status = GetString(root, \"status\", \"Status\")", controller);

        Assert.Contains("QuoteNdasUrl => $\"{QuoteEngineUrl}/ndas\"", siteContent);

        Assert.Contains("[SupplyParameterFromQuery]\n    public string? Email", forgotPassword);
        Assert.Contains("value=\"@Email\"", forgotPassword);

        Assert.Contains(".account-profile-card", styles);
        Assert.Contains(".account-avatar", styles);
        Assert.Contains(".account-membership-pill", styles);
        Assert.Contains(".account-quick-actions", styles);
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

        Assert.Contains(".account-form-header", styles);
        Assert.Contains(".account-form-alert", styles);
        Assert.Contains(".field-validation-error", styles);
        Assert.Contains(".email-change-prompt", styles);
        Assert.Contains(".language-dropdown", styles);
        Assert.Contains(".language-dropdown-trigger", styles);
        Assert.Contains(".language-dropdown-item", styles);
        Assert.Contains(".timezone-select", styles);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot()
    {
        foreach (var startDirectory in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
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
