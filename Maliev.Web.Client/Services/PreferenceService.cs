using Maliev.Web.Shared.Localization;
using Microsoft.JSInterop;
using System.Globalization;

namespace Maliev.Web.Client.Services;

internal sealed class PreferenceService(IJSRuntime jsRuntime)
{
    internal const string LightTheme = "light";
    internal const string DarkTheme = "dark";

    internal string Culture { get; private set; } = SupportedCultures.Normalize(null);
    internal string Theme { get; private set; } = LightTheme;

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
        Culture = SupportedCultures.Apply(culture);
        await jsRuntime.InvokeVoidAsync("malievCulture.setCulture", Culture);
    }

    internal async Task SetThemeAsync(string theme)
    {
        Theme = NormalizeTheme(theme);
        await jsRuntime.InvokeVoidAsync("malievCulture.setTheme", Theme);
    }

    internal static string NormalizeTheme(string? theme)
    {
        return string.Equals(theme, DarkTheme, StringComparison.OrdinalIgnoreCase) ? DarkTheme : LightTheme;
    }
}
