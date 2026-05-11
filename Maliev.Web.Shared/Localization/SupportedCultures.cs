using System.Globalization;

namespace Maliev.Web.Shared.Localization;

/// <summary>
/// Defines the public cultures supported by the MALIEV customer website.
/// </summary>
public static class SupportedCultures
{
    /// <summary>Gets the English fallback culture.</summary>
    public const string DefaultCulture = "en-US";

    /// <summary>Gets the Thai culture.</summary>
    public const string ThaiCulture = "th-TH";

    /// <summary>Gets every supported culture name.</summary>
    public static readonly IReadOnlyList<string> Names = [DefaultCulture, ThaiCulture];

    /// <summary>
    /// Normalizes a supplied culture name to one of the public culture names.
    /// </summary>
    /// <param name="cultureName">The culture name from the browser, account, cookie, or geolocation signal.</param>
    /// <returns>A supported culture name.</returns>
    public static string Normalize(string? cultureName)
    {
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return DefaultCulture;
        }

        if (cultureName.StartsWith("th", StringComparison.OrdinalIgnoreCase))
        {
            return ThaiCulture;
        }

        return DefaultCulture;
    }

    /// <summary>
    /// Applies a supported culture to the current execution context.
    /// </summary>
    /// <param name="cultureName">The culture to apply.</param>
    /// <returns>The normalized culture that was applied.</returns>
    public static string Apply(string? cultureName)
    {
        var normalized = Normalize(cultureName);
        var culture = CultureInfo.GetCultureInfo(normalized);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        return normalized;
    }
}
