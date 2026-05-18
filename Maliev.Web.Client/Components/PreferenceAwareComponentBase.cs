using Maliev.Web.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Maliev.Web.Client;

/// <summary>
/// Base component that refreshes preference-backed UI when language or theme settings change.
/// </summary>
public abstract class PreferenceAwareComponentBase : ComponentBase, IDisposable
{
    /// <summary>Gets the shared browser preference state.</summary>
    [Inject]
    protected PreferenceService Preferences { get; set; } = default!;

    /// <summary>Gets the navigation manager used to refresh shared route-backed components.</summary>
    [Inject]
    protected NavigationManager PageNavigation { get; set; } = default!;

    private bool _disposed;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Preferences.Changed += OnPreferencesChanged;
        PageNavigation.LocationChanged += OnLocationChanged;
    }

    private void OnPreferencesChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _ = InvokeAsync(StateHasChanged);
    }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Preferences.Changed -= OnPreferencesChanged;
        PageNavigation.LocationChanged -= OnLocationChanged;
        _disposed = true;
    }
}
