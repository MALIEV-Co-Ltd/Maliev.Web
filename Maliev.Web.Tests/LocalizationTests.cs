using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Tests;

/// <summary>
/// Tests language normalization rules for the customer website.
/// </summary>
public sealed class LocalizationTests
{
    /// <summary>
    /// Verifies Thai browser, geolocation, or account signals normalize to th-TH.
    /// </summary>
    [Theory]
    [InlineData("th", "th-TH")]
    [InlineData("th-TH", "th-TH")]
    [InlineData("en-US", "en-US")]
    [InlineData("", "en-US")]
    public void Normalize_CultureSignal_ReturnsSupportedCulture(string input, string expected)
    {
        Assert.Equal(expected, SupportedCultures.Normalize(input));
    }

    /// <summary>
    /// Verifies per-user Blazor Server preferences do not mutate process-wide culture defaults.
    /// </summary>
    [Fact]
    public void PreferenceService_DoesNotApplyCultureProcessWide()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Services", "PreferenceService.cs");
        var cultures = ReadRepoFile("Maliev.Web.Shared", "Localization", "SupportedCultures.cs");

        Assert.Contains("SupportedCultures.Apply(Culture)", source);
        Assert.Contains("SupportedCultures.Apply(culture)", source);
        Assert.DoesNotContain("DefaultThreadCurrentCulture", cultures);
        Assert.DoesNotContain("DefaultThreadCurrentUICulture", cultures);
    }

    /// <summary>
    /// Verifies the public language switch always shows one English and one Thai target.
    /// </summary>
    [Fact]
    public void LanguageSwitch_UsesStableEnglishAndThaiLabels()
    {
        var source = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");

        Assert.Contains(">EN</button>", source);
        Assert.Contains(">TH</button>", source);
        Assert.Contains("@Text(\"Language\", \"ภาษา\")", source);
        Assert.DoesNotContain("@Text(\"EN\"", source);
        Assert.DoesNotContain("@L[\"English\"]", source);
        Assert.DoesNotContain("@L[\"Thai\"]", source);
    }

    /// <summary>
    /// Verifies interactive landing labels use the same preference-backed bilingual text path as the rest of the page.
    /// </summary>
    [Fact]
    public void LandingChrome_DoesNotMixResourceCultureWithPreferenceCulture()
    {
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var home = ReadRepoFile("Maliev.Web.Client", "Pages", "Home.razor");

        Assert.False(layout.Contains("IStringLocalizer<SharedResources>", StringComparison.Ordinal));
        Assert.False(home.Contains("IStringLocalizer<SharedResources>", StringComparison.Ordinal));
        Assert.False(layout.Contains("L[\"", StringComparison.Ordinal));
        Assert.False(home.Contains("L[\"", StringComparison.Ordinal));
        Assert.Contains("@Text(\"Get part price\", \"ดูราคาชิ้นงาน\")", layout);
        Assert.Contains("@Text(\"Get part price\", \"ดูราคาชิ้นงาน\")", home);
    }

    /// <summary>
    /// Verifies the site theme preference is stored per browser and reflected on the document root.
    /// </summary>
    [Fact]
    public void ThemePreference_UsesDocumentRootAndStableToggleLabels()
    {
        var service = ReadRepoFile("Maliev.Web.Client", "Services", "PreferenceService.cs");
        var script = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-culture.js");
        var app = ReadRepoFile("Maliev.Web.Bff", "Components", "App.razor");
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");

        Assert.Contains("LightTheme = \"light\"", service);
        Assert.Contains("DarkTheme = \"dark\"", service);
        Assert.Contains("malievCulture.resolveTheme", service);
        Assert.Contains("malievCulture.setTheme", service);
        Assert.Contains("NormalizeTheme", service);
        Assert.Contains("maliev.theme", script);
        Assert.Contains("applyDocumentTheme", script);
        Assert.Contains("document.documentElement.dataset.theme", script);
        Assert.Contains("document.documentElement.style.colorScheme", script);
        Assert.Contains("data-theme=\"light\"", app);
        Assert.Contains("maliev.theme", app);
        Assert.Contains("theme-toggle-button", layout);
        Assert.Contains("OnClick=\"ToggleThemeAsync\"", layout);
        Assert.Contains("Icons.Material.Filled.LightMode", layout);
        Assert.Contains("Icons.Material.Filled.DarkMode", layout);
        Assert.Contains("Switch to dark theme", layout);
        Assert.Contains("Switch to light theme", layout);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
