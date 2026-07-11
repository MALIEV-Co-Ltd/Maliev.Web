using System.Text.RegularExpressions;

namespace Maliev.Web.Tests;

/// <summary>
/// Source-level contracts for the Web production image and pull-request validation boundary.
/// </summary>
public sealed class DeploymentReadinessSourceTests
{
    /// <summary>
    /// Verifies the production container restores with ephemeral credentials and runs as a non-root user.
    /// </summary>
    [Fact]
    public void ProductionDockerfilePublishesWebBffIntoNonRootRuntimeImage()
    {
        var dockerfilePath = RepoPath("Maliev.Web.Bff", "Dockerfile");
        var dockerIgnorePath = RepoPath(".dockerignore");

        Assert.True(File.Exists(dockerfilePath), "Expected a production Dockerfile for the Web BFF.");
        Assert.True(File.Exists(dockerIgnorePath), "Expected a root .dockerignore for the production build context.");

        var dockerfile = File.ReadAllText(dockerfilePath);
        var dockerIgnore = File.ReadAllText(dockerIgnorePath);

        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build", dockerfile, StringComparison.Ordinal);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--mount=type=secret,id=nuget_username,required=true", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--mount=type=secret,id=nuget_password,required=true", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:SharedLibraryVersion=\"1.0.81-alpha\"", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.*-alpha*", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("ARG NUGET_", dockerfile, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet publish", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--configuration Release", dockerfile, StringComparison.Ordinal);
        Assert.Contains("COPY --chown=app:app --from=build /app/publish .", dockerfile, StringComparison.Ordinal);
        Assert.Contains("USER app", dockerfile, StringComparison.Ordinal);
        Assert.Contains("EXPOSE 8080", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ASPNETCORE_HTTP_PORTS=8080", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("ASPNETCORE_URLS", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ENTRYPOINT [\"dotnet\", \"Maliev.Web.Bff.dll\"]", dockerfile, StringComparison.Ordinal);

        var restoreIndex = dockerfile.IndexOf("dotnet restore", StringComparison.Ordinal);
        var sourceCopyIndex = dockerfile.IndexOf("COPY . .", StringComparison.Ordinal);
        Assert.True(restoreIndex >= 0, "Expected a cached dependency restore layer.");
        Assert.True(sourceCopyIndex >= 0, "Expected a source copy layer ('COPY . .').");
        Assert.True(sourceCopyIndex > restoreIndex, "Source must be copied only after the dependency restore layer.");

        Assert.Contains("**/bin", dockerIgnore, StringComparison.Ordinal);
        Assert.Contains("**/obj", dockerIgnore, StringComparison.Ordinal);
        Assert.Contains("**/.git", dockerIgnore, StringComparison.Ordinal);
        Assert.Contains("**/TestResults", dockerIgnore, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies PR validation exercises Release code and the local image without publishing or deploying it.
    /// </summary>
    [Fact]
    public void PullRequestValidationBuildsTestsAndScansWithoutPublishingOrDeploying()
    {
        var workflowPath = RepoPath(".github", "workflows", "pr-validation.yml");

        Assert.True(File.Exists(workflowPath), "Expected a pull-request validation workflow.");

        var workflow = File.ReadAllText(workflowPath);

        Assert.Contains("pull_request:", workflow, StringComparison.Ordinal);
        Assert.Contains("branches: [develop]", workflow, StringComparison.Ordinal);
        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
        Assert.Contains("github.run_id", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("pull_request_target", workflow, StringComparison.Ordinal);
        Assert.Contains("permissions:", workflow, StringComparison.Ordinal);
        Assert.Contains("contents: read", workflow, StringComparison.Ordinal);
        Assert.Contains("packages: read", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("write-all", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("concurrency:", workflow, StringComparison.Ordinal);
        Assert.Contains("cancel-in-progress: true", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build Maliev.Web.slnx --configuration Release --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test Maliev.Web.slnx --configuration Release --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains("/p:SharedLibraryVersion=\"1.0.81-alpha\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.*-alpha*", workflow, StringComparison.Ordinal);
        Assert.Contains("push: false", workflow, StringComparison.Ordinal);
        Assert.Contains("load: true", workflow, StringComparison.Ordinal);
        Assert.Contains("docker image inspect", workflow, StringComparison.Ordinal);
        Assert.Contains("docker run --detach", workflow, StringComparison.Ordinal);
        Assert.Contains("/web/liveness", workflow, StringComparison.Ordinal);
        Assert.Contains("aquasecurity/trivy-action@", workflow, StringComparison.Ordinal);
        Assert.Contains("format: cyclonedx", workflow, StringComparison.Ordinal);
        Assert.Contains("severity: HIGH,CRITICAL", workflow, StringComparison.Ordinal);
        Assert.Contains("exit-code: \"1\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("ignore-unfixed: true", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("docker/login-action", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GITOPS_PAT", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("argocd", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kubectl", workflow, StringComparison.OrdinalIgnoreCase);

        var unpinnedActions = Regex.Matches(
            workflow,
            @"uses:\s+[^\s@]+@(?![0-9a-f]{40}(?:\s|$))[^\s]+",
            RegexOptions.CultureInvariant);
        Assert.Empty(unpinnedActions.Select(match => match.Value));
    }

    private static string RepoPath(params string[] pathSegments)
    {
        return Path.Combine([FindRepoRoot(), .. pathSegments]);
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

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate Maliev.Web repository root.");
    }
}
