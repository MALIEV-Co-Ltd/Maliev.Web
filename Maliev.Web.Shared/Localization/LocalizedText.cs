namespace Maliev.Web.Shared.Localization;

/// <summary>
/// Carries English and Thai text for customer-facing catalog and service content.
/// </summary>
public sealed class LocalizedText
{
    /// <summary>Gets or sets the English value.</summary>
    public string En { get; set; } = string.Empty;

    /// <summary>Gets or sets the Thai value.</summary>
    public string Th { get; set; } = string.Empty;

    /// <summary>
    /// Selects text for the requested culture.
    /// </summary>
    /// <param name="cultureName">The culture name.</param>
    /// <returns>The localized text.</returns>
    public string For(string? cultureName)
    {
        return SupportedCultures.Normalize(cultureName) == SupportedCultures.ThaiCulture && !string.IsNullOrWhiteSpace(Th)
            ? Th
            : En;
    }
}
