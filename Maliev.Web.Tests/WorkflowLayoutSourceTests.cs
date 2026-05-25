namespace Maliev.Web.Tests;

/// <summary>
/// Source-level regression tests for public workflow layout behavior.
/// </summary>
public sealed class WorkflowLayoutSourceTests
{
    /// <summary>
    /// Verifies workflow steps keep a full-width inner track so shorter localized copy does not shift left-column cards.
    /// </summary>
    [Fact]
    public void WorkflowStepsUseFullWidthTrackForConsistentColumnAlignment()
    {
        var styles = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "app.css");
        var workflowStepStyles = ReadStyleRule(styles, ".workflow-step");

        Assert.Contains("display: grid;", workflowStepStyles, StringComparison.Ordinal);
        Assert.Contains("width: 100%;", workflowStepStyles, StringComparison.Ordinal);
        Assert.Contains("grid-template-columns: minmax(0, 1fr);", workflowStepStyles, StringComparison.Ordinal);
        Assert.Contains("justify-items: start;", workflowStepStyles, StringComparison.Ordinal);
        Assert.DoesNotContain("justify-content: center;", workflowStepStyles, StringComparison.Ordinal);
    }

    private static string ReadStyleRule(string styles, string selector)
    {
        var start = styles.IndexOf($"{selector} {{", StringComparison.Ordinal);
        Assert.True(start >= 0, $"Expected to find style rule for {selector}.");

        var end = styles.IndexOf("\n}", start, StringComparison.Ordinal);
        Assert.True(end > start, $"Expected to find the end of style rule for {selector}.");

        return styles[start..(end + 2)];
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
