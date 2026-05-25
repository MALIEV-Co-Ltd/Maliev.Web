namespace Maliev.Web.Tests;

/// <summary>
/// Source-level regression tests for public site navigation scroll behavior.
/// </summary>
public sealed class NavigationScrollSourceTests
{
    /// <summary>
    /// Verifies route changes reset the viewport instantly before focused page headings can inherit smooth scrolling.
    /// </summary>
    [Fact]
    public void InternalRouteNavigationResetsViewportWithoutSmoothBackscroll()
    {
        var layout = ReadRepoFile("Maliev.Web.Client", "Layout", "MainLayout.razor");
        var scrollScript = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "maliev-scroll.js");

        Assert.Contains("@inject NavigationManager Navigation", layout, StringComparison.Ordinal);
        Assert.Contains("Navigation.LocationChanged += OnLocationChanged;", layout, StringComparison.Ordinal);
        Assert.Contains("Navigation.LocationChanged -= OnLocationChanged;", layout, StringComparison.Ordinal);
        Assert.Contains("malievScroll.scrollToTopForNavigation", layout, StringComparison.Ordinal);
        Assert.Contains("private void OnLocationChanged(object? sender, LocationChangedEventArgs args)", layout, StringComparison.Ordinal);

        Assert.Contains("scrollToTopForNavigation: function ()", scrollScript, StringComparison.Ordinal);
        Assert.Contains("window.clearTimeout(window.__malievNavigationScrollRestoreId)", scrollScript, StringComparison.Ordinal);
        Assert.Contains("root.style.scrollBehavior = 'auto';", scrollScript, StringComparison.Ordinal);
        Assert.Contains("body.style.scrollBehavior = 'auto';", scrollScript, StringComparison.Ordinal);
        Assert.Contains("window.scrollTo({ left: 0, top: 0, behavior: 'auto' });", scrollScript, StringComparison.Ordinal);
        Assert.Contains("window.__malievNavigationScrollState", scrollScript, StringComparison.Ordinal);
        Assert.Contains("window.setTimeout(restoreScrollBehavior, 800)", scrollScript, StringComparison.Ordinal);
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

            var siblingCandidate = Path.Combine(directory.FullName, "Maliev.Web");
            if (File.Exists(Path.Combine(siblingCandidate, "Maliev.Web.slnx")))
            {
                return siblingCandidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
