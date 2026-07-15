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
        var directoryBuildPropsPath = RepoPath("Directory.Build.props");
        var bffProjectPath = RepoPath("Maliev.Web.Bff", "Maliev.Web.Bff.csproj");

        Assert.True(File.Exists(dockerfilePath), "Expected a production Dockerfile for the Web BFF.");
        Assert.True(File.Exists(dockerIgnorePath), "Expected a root .dockerignore for the production build context.");

        var dockerfile = File.ReadAllText(dockerfilePath);
        var dockerIgnore = File.ReadAllText(dockerIgnorePath);
        var directoryBuildProps = File.ReadAllText(directoryBuildPropsPath);
        var bffProject = File.ReadAllText(bffProjectPath);

        Assert.Contains("FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build", dockerfile, StringComparison.Ordinal);
        Assert.Contains("FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--mount=type=secret,id=nuget_username,required=true", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--mount=type=secret,id=nuget_password,required=true", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ARG dependency_restore_stage=restore-private", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ARG shared_library_version", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ARG messaging_contracts_version", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ARG app_version", dockerfile, StringComparison.Ordinal);
        Assert.Contains("ARG commit_sha", dockerfile, StringComparison.Ordinal);
        Assert.Contains(
            "${shared_library_version:?shared_library_version build argument is required}",
            dockerfile,
            StringComparison.Ordinal);
        Assert.Contains(
            "${messaging_contracts_version:?messaging_contracts_version build argument is required}",
            dockerfile,
            StringComparison.Ordinal);
        Assert.Contains("FROM ${dependency_restore_stage} AS build", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--configfile \"NuGet.PRValidation.Config\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:SharedLibraryVersion=\"$shared_library_version\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:MessagingContractsVersion=\"$messaging_contracts_version\"", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.81-alpha", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.*-alpha*", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("ARG NUGET_", dockerfile, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dotnet publish", dockerfile, StringComparison.Ordinal);
        Assert.Contains("--configuration Release", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:Version=\"$app_version\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:SourceRevisionId=\"$commit_sha\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("/p:ContinuousIntegrationBuild=true", dockerfile, StringComparison.Ordinal);
        Assert.Contains("COPY --chown=app:app --from=build /app/publish .", dockerfile, StringComparison.Ordinal);
        Assert.Contains("org.opencontainers.image.version=\"$app_version\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("org.opencontainers.image.revision=\"$commit_sha\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("org.opencontainers.image.source=\"https://github.com/MALIEV-Co-Ltd/Maliev.Web\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("BuildMetadata__Version=\"$app_version\"", dockerfile, StringComparison.Ordinal);
        Assert.Contains("BuildMetadata__CommitSha=\"$commit_sha\"", dockerfile, StringComparison.Ordinal);
        Assert.DoesNotContain("ARG image_digest", dockerfile, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("BuildMetadata__ImageDigest", dockerfile, StringComparison.Ordinal);
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
        Assert.Contains("!.ci-packages/*.nupkg", dockerIgnore, StringComparison.Ordinal);
        Assert.Contains(
            "<MessagingContractsVersion Condition=\"'$(MessagingContractsVersion)' == ''\">$(SharedLibraryVersion)</MessagingContractsVersion>",
            directoryBuildProps,
            StringComparison.Ordinal);
        Assert.Contains(
            "<PackageReference Include=\"Maliev.MessagingContracts\" Version=\"$(MessagingContractsVersion)\" />",
            bffProject,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies PR validation exercises Release code and the local image without publishing, deploying,
    /// or granting untrusted pull requests access to private organization packages.
    /// </summary>
    [Fact]
    public void PullRequestValidationBuildsTestsAndScansWithoutPublishingOrDeploying()
    {
        var workflowPath = RepoPath(".github", "workflows", "pr-validation.yml");
        var ciNuGetConfigPath = RepoPath("NuGet.PRValidation.Config");
        var packageScriptPath = RepoPath("scripts", "prepare-web-ci-packages.sh");

        Assert.True(File.Exists(workflowPath), "Expected a pull-request validation workflow.");
        Assert.True(File.Exists(ciNuGetConfigPath), "Expected a credential-free PR validation NuGet configuration.");
        Assert.True(File.Exists(packageScriptPath), "Expected an exact dependency package preparation script.");

        var workflow = File.ReadAllText(workflowPath);
        var ciNuGetConfig = File.ReadAllText(ciNuGetConfigPath);
        var packageScript = File.ReadAllText(packageScriptPath);

        Assert.Contains("pull_request:", workflow, StringComparison.Ordinal);
        Assert.Contains("branches: [develop]", workflow, StringComparison.Ordinal);
        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
        Assert.Contains("github.run_id", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("pull_request_target", workflow, StringComparison.Ordinal);
        Assert.Contains("permissions:", workflow, StringComparison.Ordinal);
        Assert.Contains("contents: read", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("packages: read", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("github.token", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NUGET_USERNAME", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("NUGET_PASSWORD", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("nuget.pkg.github.com", ciNuGetConfig, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("packageSourceCredentials", ciNuGetConfig, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<add key=\"maliev-ci\" value=\".ci-packages\" />", ciNuGetConfig, StringComparison.Ordinal);
        Assert.Contains("<packageSource key=\"nuget.org\">", ciNuGetConfig, StringComparison.Ordinal);
        Assert.Contains("<package pattern=\"*\" />", ciNuGetConfig, StringComparison.Ordinal);
        Assert.Contains("<packageSource key=\"maliev-ci\">", ciNuGetConfig, StringComparison.Ordinal);
        Assert.Contains("<package pattern=\"Maliev.*\" />", ciNuGetConfig, StringComparison.Ordinal);
        Assert.Contains("--configfile \"$ci_nuget_config\"", packageScript, StringComparison.Ordinal);
        Assert.Contains("dotnet restore \"$generator_project\" --configfile \"$ci_nuget_config\"", packageScript, StringComparison.Ordinal);
        Assert.Contains("dotnet run --project tools/Generator/Generator.csproj --configuration Release --no-restore", packageScript, StringComparison.Ordinal);
        Assert.DoesNotContain("--source", packageScript, StringComparison.Ordinal);
        Assert.Contains("MALIEV-Co-Ltd/Maliev.Aspire", workflow, StringComparison.Ordinal);
        Assert.Contains("ref: 7121d57705fc1eff6c7ebb6a69e33e9c26ebfccc", workflow, StringComparison.Ordinal);
        Assert.Contains("MALIEV-Co-Ltd/Maliev.MessagingContracts", workflow, StringComparison.Ordinal);
        Assert.Contains("ref: d4836f135d1cf311b2a490d9ba03809ff295e854", workflow, StringComparison.Ordinal);
        Assert.Contains("prepare-web-ci-packages.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("id: package-versions", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging-contracts-version: ${{ steps.package-versions.outputs.messaging-contracts-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("service-defaults-version: ${{ steps.package-versions.outputs.service-defaults-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging-contracts-version=$messaging_version", packageScript, StringComparison.Ordinal);
        Assert.Contains("service-defaults-version=$service_defaults_version", packageScript, StringComparison.Ordinal);
        Assert.Contains("$GITHUB_OUTPUT", packageScript, StringComparison.Ordinal);
        Assert.Contains("web-ci-packages", workflow, StringComparison.Ordinal);
        Assert.Contains("include-hidden-files: true", workflow, StringComparison.Ordinal);
        Assert.Contains("overwrite: true", workflow, StringComparison.Ordinal);
        Assert.Contains("--configfile NuGet.PRValidation.Config", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("--source", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("write-all", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("concurrency:", workflow, StringComparison.Ordinal);
        Assert.Contains("cancel-in-progress: true", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet build Maliev.Web.slnx --configuration Release --no-restore", workflow, StringComparison.Ordinal);
        Assert.Contains("dotnet test Maliev.Web.slnx --configuration Release --no-build", workflow, StringComparison.Ordinal);
        Assert.Contains("SHARED_LIBRARY_VERSION: ${{ needs.dependency-packages.outputs.service-defaults-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("MESSAGING_CONTRACTS_VERSION: ${{ needs.dependency-packages.outputs.messaging-contracts-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("/p:SharedLibraryVersion=\"${SHARED_LIBRARY_VERSION}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("/p:MessagingContractsVersion=\"${MESSAGING_CONTRACTS_VERSION}\"", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.81-alpha", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("1.0.*-alpha*", workflow, StringComparison.Ordinal);
        Assert.Contains("push: false", workflow, StringComparison.Ordinal);
        Assert.Contains("load: true", workflow, StringComparison.Ordinal);
        Assert.Contains("dependency_restore_stage=restore-local", workflow, StringComparison.Ordinal);
        Assert.Contains("shared_library_version=${{ needs.dependency-packages.outputs.service-defaults-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging_contracts_version=${{ needs.dependency-packages.outputs.messaging-contracts-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("app_version=0.0.0-pr.${{ github.run_number }}", workflow, StringComparison.Ordinal);
        Assert.Contains("commit_sha=${{ github.sha }}", workflow, StringComparison.Ordinal);
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
