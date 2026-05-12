using Maliev.Web.Shared.Localization;
using Microsoft.JSInterop;
using System.Globalization;

namespace Maliev.Web.Client.Services;

/// <summary>
/// Stores browser-level culture and theme preferences for the current interactive circuit.
/// </summary>
/// <param name="jsRuntime">The JavaScript runtime used to read and persist browser preferences.</param>
public sealed class PreferenceService(IJSRuntime jsRuntime)
{
    internal const string LightTheme = "light";
    internal const string DarkTheme = "dark";

    internal string Culture { get; private set; } = SupportedCultures.Normalize(null);
    internal string Theme { get; private set; } = LightTheme;
    internal event Action? Changed;

    internal async Task InitializeAsync()
    {
        try
        {
            Culture = await jsRuntime.InvokeAsync<string>("malievCulture.resolveCulture", SupportedCultures.DefaultCulture);
            Theme = await jsRuntime.InvokeAsync<string>("malievCulture.resolveTheme", (string?)null);
        }
        catch (InvalidOperationException)
        {
            Culture = CultureInfo.CurrentUICulture.Name;
            Theme = LightTheme;
        }
        catch (JSException)
        {
            Culture = CultureInfo.CurrentUICulture.Name;
            Theme = LightTheme;
        }

        Culture = SupportedCultures.Apply(Culture);
        Theme = NormalizeTheme(Theme);
    }

    internal async Task SetCultureAsync(string culture)
    {
        var normalizedCulture = SupportedCultures.Apply(culture);
        var hasChanged = !string.Equals(Culture, normalizedCulture, StringComparison.Ordinal);
        Culture = normalizedCulture;
        await jsRuntime.InvokeVoidAsync("malievCulture.setCulture", Culture);

        if (hasChanged)
        {
            Changed?.Invoke();
        }
    }

    internal async Task SetThemeAsync(string theme)
    {
        var normalizedTheme = NormalizeTheme(theme);
        var hasChanged = !string.Equals(Theme, normalizedTheme, StringComparison.Ordinal);
        Theme = normalizedTheme;
        await jsRuntime.InvokeVoidAsync("malievCulture.setTheme", Theme);

        if (hasChanged)
        {
            Changed?.Invoke();
        }
    }

    internal static string NormalizeTheme(string? theme)
    {
        return string.Equals(theme, DarkTheme, StringComparison.OrdinalIgnoreCase) ? DarkTheme : LightTheme;
    }
}
