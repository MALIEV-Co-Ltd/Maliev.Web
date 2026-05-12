using Maliev.Web.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Maliev.Web.Client;

/// <summary>
/// Base component that refreshes preference-backed UI when language or theme settings change.
/// </summary>
public abstract class PreferenceAwareComponentBase : ComponentBase, IDisposable
{
    /// <summary>Gets the shared browser preference state.</summary>
    [Inject]
    protected PreferenceService Preferences { get; set; } = default!;

    private bool _disposed;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Preferences.Changed += OnPreferencesChanged;
    }

    private void OnPreferencesChanged()
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
        _disposed = true;
    }
}
