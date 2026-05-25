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
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");

        Assert.Contains("account-profile-card", account);
        Assert.Contains("account-avatar", account);
        Assert.Contains("AvatarInitials", account);
        Assert.Contains("CustomerTierLabel", account);
        Assert.Contains("CustomerSegmentLabel", account);
        Assert.Contains("@Text(\"Customer tier\", \"ระดับลูกค้า\")", account);
        Assert.Contains("@Text(\"Change email\", \"เปลี่ยนอีเมล\")", account);
        Assert.Contains("@Text(\"Reset password\", \"รีเซ็ตรหัสผ่าน\")", account);
        Assert.Contains("ResetPasswordHref", account);
        Assert.DoesNotContain("@Text(\"Status\", \"สถานะ\")", account);
        Assert.DoesNotContain("_profile?.Status", account);

        Assert.Contains("public string Tier { get; set; } = string.Empty;", dtos);
        Assert.Contains("public string Segment { get; set; } = string.Empty;", dtos);
        Assert.DoesNotContain("Gets or sets the customer status.", dtos);
        Assert.Contains("Tier = GetString(root, \"tier\", \"Tier\") ?? string.Empty", controller);
        Assert.Contains("Segment = GetString(root, \"segment\", \"Segment\") ?? string.Empty", controller);
        Assert.DoesNotContain("Status = GetString(root, \"status\", \"Status\")", controller);

        Assert.Contains("[SupplyParameterFromQuery]\n    public string? Email", forgotPassword);
        Assert.Contains("value=\"@Email\"", forgotPassword);

        Assert.Contains(".account-profile-card", styles);
        Assert.Contains(".account-avatar", styles);
        Assert.Contains(".account-membership-pill", styles);
        Assert.Contains(".account-quick-actions", styles);
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
