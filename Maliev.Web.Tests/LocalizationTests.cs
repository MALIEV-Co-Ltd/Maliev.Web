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
}
