namespace Maliev.Web.Tests;

/// <summary>
/// Source-level tests for the public manufacturing gizmo runtime.
/// </summary>
public sealed class ManufacturingGizmoRuntimeSourceTests
{
    /// <summary>
    /// Verifies the Babylon runtime is shared across global script loading and Blazor module imports.
    /// </summary>
    [Fact]
    public void ManufacturingGizmoSharesRuntimeAcrossModuleInstances()
    {
        var source = ReadRepoFile("Maliev.Web.Bff", "wwwroot", "js", "manufacturing-gizmo.js");

        Assert.Contains("globalThis.__malievManufacturingGizmo", source);
        Assert.Contains("instances: new WeakMap()", source);
        Assert.Contains("sharedRuntime.instances", source);
        Assert.Contains("sharedRuntime.babylonRuntime", source);
        Assert.Contains("sharedRuntime.babylonLoadersRuntime", source);
        Assert.DoesNotContain("const instances = new WeakMap();", source);
        Assert.DoesNotContain("let babylonRuntime;", source);
        Assert.DoesNotContain("let babylonLoadersRuntime;", source);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var root = FindRepoRoot();
        return File.ReadAllText(Path.Combine([root, .. pathSegments]));
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Maliev.Web.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate Maliev.Web repository root.");
    }
}
