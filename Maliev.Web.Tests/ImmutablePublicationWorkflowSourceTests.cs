using System.Text.RegularExpressions;

namespace Maliev.Web.Tests;

/// <summary>
/// Source-level contracts for immutable image publication and promotion workflows.
/// </summary>
public sealed class ImmutablePublicationWorkflowSourceTests
{
    private static readonly string[] RequiredWorkflowNames =
    [
        "_build-and-test.yml",
        "ci-develop.yml",
        "ci-staging.yml",
        "ci-main.yml",
        "pr-validation.yml",
        "promote-production.yml"
    ];

    /// <summary>
    /// Verifies the reusable workflow reconstructs exact public-source packages without credentials.
    /// </summary>
    [Fact]
    public void ReusableBuildUsesExactCredentialFreeDependencyPackages()
    {
        var workflow = ReadRepoFile(".github", "workflows", "_build-and-test.yml");

        Assert.Contains("MALIEV-Co-Ltd/Maliev.MessagingContracts", workflow, StringComparison.Ordinal);
        Assert.Contains("d4836f135d1cf311b2a490d9ba03809ff295e854", workflow, StringComparison.Ordinal);
        Assert.Contains("MALIEV-Co-Ltd/Maliev.Aspire", workflow, StringComparison.Ordinal);
        Assert.Contains("7121d57705fc1eff6c7ebb6a69e33e9c26ebfccc", workflow, StringComparison.Ordinal);
        Assert.Contains("bash scripts/prepare-web-ci-packages.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("name: web-ci-packages", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging-contracts-version", workflow, StringComparison.Ordinal);
        Assert.Contains("service-defaults-version", workflow, StringComparison.Ordinal);
        Assert.Contains("--configfile NuGet.PRValidation.Config", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("secrets:", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("GITOPS_PAT", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("NUGET_PASSWORD", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("NUGET_USERNAME", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("packages: read", workflow, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies trusted develop pushes publish and verify a fully attested immutable image.
    /// </summary>
    [Fact]
    public void DevelopPublishesDiagnosticSemVerAndVerifiesDigestScanSbomAndProvenance()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci-develop.yml");

        Assert.Contains("branches: [develop]", workflow, StringComparison.Ordinal);
        Assert.Contains("cancel-in-progress: true", workflow, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes:", workflow, StringComparison.Ordinal);
        Assert.Contains("id-token: write", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_DEVELOPMENT_WORKLOAD_IDENTITY_PROVIDER", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_DEVELOPMENT_SERVICE_ACCOUNT", workflow, StringComparison.Ordinal);
        Assert.Contains("dev-${GITHUB_SHA::12}", workflow, StringComparison.Ordinal);
        Assert.Contains("dependency_restore_stage=restore-local", workflow, StringComparison.Ordinal);
        Assert.Contains("shared_library_version=${{ needs.build-and-test.outputs.service-defaults-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging_contracts_version=${{ needs.build-and-test.outputs.messaging-contracts-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("app_version=0.0.0-dev.${{ github.run_number }}", workflow, StringComparison.Ordinal);
        Assert.Contains("commit_sha=${{ github.sha }}", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("image_digest=", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("provenance: mode=max,version=v1", workflow, StringComparison.Ordinal);
        Assert.Contains("sbom: true", workflow, StringComparison.Ordinal);
        Assert.Contains("severity CRITICAL,HIGH", workflow, StringComparison.Ordinal);
        Assert.Contains("https://spdx.dev/Document", workflow, StringComparison.Ordinal);
        Assert.Contains("https://slsa.dev/provenance/v1", workflow, StringComparison.Ordinal);
        Assert.Contains("crane digest", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("credentials_json", workflow, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies release tags promote the verified development digest to staging without rebuilding.
    /// </summary>
    [Fact]
    public void StagingPromotesExactDevelopmentDigestWithoutRebuild()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci-staging.yml");

        Assert.Contains("tags: [release/v*.*.*]", workflow, StringComparison.Ordinal);
        Assert.Contains("environment: staging", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_STAGING_WORKLOAD_IDENTITY_PROVIDER", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_STAGING_SERVICE_ACCOUNT", workflow, StringComparison.Ordinal);
        Assert.Contains("version=${GITHUB_REF_NAME#release/v}", workflow, StringComparison.Ordinal);
        Assert.Contains("source_tag=dev-${GITHUB_SHA::12}", workflow, StringComparison.Ordinal);
        Assert.Contains("org.opencontainers.image.revision", workflow, StringComparison.Ordinal);
        Assert.Contains("https://spdx.dev/Document", workflow, StringComparison.Ordinal);
        Assert.Contains("https://slsa.dev/provenance/v1", workflow, StringComparison.Ordinal);
        Assert.Contains("crane copy \"${DEVELOPMENT_IMAGE}@${source_digest}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$promoted_digest\" = \"$source_digest\"", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("docker/build-push-action", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("docker build", workflow, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies production promotion is manual, main-only, approval-gated, and digest preserving.
    /// </summary>
    [Fact]
    public void ProductionPromotesExplicitApprovedStagingDigestFromMainOnly()
    {
        var workflow = ReadRepoFile(".github", "workflows", "promote-production.yml");

        Assert.Contains("workflow_dispatch:", workflow, StringComparison.Ordinal);
        Assert.Contains("version:", workflow, StringComparison.Ordinal);
        Assert.Contains("digest:", workflow, StringComparison.Ordinal);
        Assert.Contains("if: github.ref == 'refs/heads/main'", workflow, StringComparison.Ordinal);
        Assert.Contains("environment: production", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_PRODUCTION_WORKLOAD_IDENTITY_PROVIDER", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_PRODUCTION_SERVICE_ACCOUNT", workflow, StringComparison.Ordinal);
        Assert.Contains("staging_digest=", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$staging_digest\" = \"$APPROVED_DIGEST\"", workflow, StringComparison.Ordinal);
        Assert.Contains("crane copy \"${STAGING_IMAGE}@${APPROVED_DIGEST}\"", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$(crane digest", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("docker/build-push-action", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("docker build", workflow, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies the GitOps updater is constrained to Web overlays and enforces rendered digest parity.
    /// </summary>
    [Fact]
    public void GitOpsUpdaterGuardsScopeAndRenderedImageMetadataParity()
    {
        var script = ReadRepoFile("scripts", "update-web-gitops-overlay.sh");

        Assert.Contains("development|staging|production", script, StringComparison.Ordinal);
        Assert.Contains("3-apps/maliev-web/overlays/${environment}", script, StringComparison.Ordinal);
        Assert.Contains("build-metadata-patch.yaml", script, StringComparison.Ordinal);
        Assert.Contains("BuildMetadata__ImageDigest", script, StringComparison.Ordinal);
        Assert.Contains("sha256:[0-9a-f]{64}", script, StringComparison.Ordinal);
        Assert.Contains("kustomize build", script, StringComparison.Ordinal);
        Assert.Contains("Refusing change outside", script, StringComparison.Ordinal);
        Assert.Contains("rendered image digest", script, StringComparison.Ordinal);
        Assert.Contains("rendered BuildMetadata__ImageDigest", script, StringComparison.Ordinal);
        Assert.DoesNotContain("argocd/environments", script, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies all workflow actions are immutable and GitOps PRs retain the disabled-application boundary.
    /// </summary>
    [Fact]
    public void WorkflowsPinActionsAndNeverEnableApplications()
    {
        foreach (var workflowName in RequiredWorkflowNames)
        {
            var workflow = ReadRepoFile(".github", "workflows", workflowName);
            var unpinnedActions = Regex.Matches(
                workflow,
                @"uses:\s+[^\s@]+@(?![0-9a-f]{40}(?:\s|$))[^\s]+",
                RegexOptions.CultureInvariant);

            Assert.Empty(unpinnedActions.Select(match => $"{workflowName}: {match.Value}"));
            Assert.DoesNotContain("write-all", workflow, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("argocd/environments", workflow, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("kubectl", workflow, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("enable application", workflow, StringComparison.OrdinalIgnoreCase);
        }

        foreach (var workflowName in new[] { "ci-develop.yml", "ci-staging.yml", "promote-production.yml" })
        {
            var workflow = ReadRepoFile(".github", "workflows", workflowName);
            Assert.Contains("The ArgoCD Application remains disabled", workflow, StringComparison.Ordinal);
            Assert.Equal(2, Regex.Matches(workflow, "GITOPS_PAT", RegexOptions.CultureInvariant).Count);
        }
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var path = Path.Combine([FindRepoRoot(), .. pathSegments]);
        Assert.True(File.Exists(path), $"Expected repository file '{path}'.");
        return File.ReadAllText(path);
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
