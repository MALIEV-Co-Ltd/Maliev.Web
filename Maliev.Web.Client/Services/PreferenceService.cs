using Maliev.Web.Shared.Localization;
using Microsoft.JSInterop;

namespace Maliev.Web.Client.Services;

internal sealed class PreferenceService(IJSRuntime jsRuntime)
{
    internal string Culture { get; private set; } = SupportedCultures.Normalize(null);

    internal async Task InitializeAsync()
    {
        Culture = await jsRuntime.InvokeAsync<string>("malievCulture.resolveCulture", SupportedCultures.DefaultCulture);
        Culture = SupportedCultures.Apply(Culture);
    }

    internal async Task SetCultureAsync(string culture)
    {
        Culture = SupportedCultures.Apply(culture);
        await jsRuntime.InvokeVoidAsync("malievCulture.setCulture", Culture);
    }
}
