using System.Diagnostics;
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
        var dockerfile = ReadRepoFile("Maliev.Web.Bff", "Dockerfile");

        Assert.Contains("branches: [develop]", workflow, StringComparison.Ordinal);
        Assert.Contains("cancel-in-progress: true", workflow, StringComparison.Ordinal);
        Assert.Contains("timeout-minutes:", workflow, StringComparison.Ordinal);
        Assert.Contains("id-token: write", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_DEVELOPMENT_WORKLOAD_IDENTITY_PROVIDER", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_DEVELOPMENT_SERVICE_ACCOUNT", workflow, StringComparison.Ordinal);
        Assert.Contains("maliev-web-artifact-dev/maliev-web", workflow, StringComparison.Ordinal);
        Assert.Contains("gcloud artifacts repositories describe maliev-web-artifact-dev", workflow, StringComparison.Ordinal);
        Assert.Contains("--format='value(immutableTags)'", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$immutable_tags\" = \"True\"", workflow, StringComparison.Ordinal);
        Assert.Contains("dev-${GITHUB_SHA::12}", workflow, StringComparison.Ordinal);
        Assert.Contains("outputs: type=image,name=${{ env.IMAGE }},push-by-digest=true,name-canonical=true,push=true", workflow, StringComparison.Ordinal);
        Assert.Contains("context: https://github.com/MALIEV-Co-Ltd/Maliev.Web.git#${{ github.sha }}", workflow, StringComparison.Ordinal);
        Assert.Contains("secret-files:", workflow, StringComparison.Ordinal);
        Assert.Contains("ci_packages=", workflow, StringComparison.Ordinal);
        Assert.Contains("GIT_AUTH_TOKEN=${{ github.token }}", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("tags: ${{ env.IMAGE }}:${{ steps.image.outputs.tag }}", workflow, StringComparison.Ordinal);
        Assert.Contains("dependency_restore_stage=restore-attested", workflow, StringComparison.Ordinal);
        Assert.Contains("shared_library_version=${{ needs.build-and-test.outputs.service-defaults-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("messaging_contracts_version=${{ needs.build-and-test.outputs.messaging-contracts-version }}", workflow, StringComparison.Ordinal);
        Assert.Contains("app_version=0.0.0-dev.${{ github.run_number }}", workflow, StringComparison.Ordinal);
        Assert.Contains("commit_sha=${{ github.sha }}", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("image_digest=", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("provenance: mode=max,version=v1", workflow, StringComparison.Ordinal);
        Assert.Contains("builder-id=${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}", workflow, StringComparison.Ordinal);
        Assert.Contains("sbom: true", workflow, StringComparison.Ordinal);
        Assert.Contains("severity CRITICAL,HIGH", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/ensure-web-image-tag.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/verify-web-image-attestations.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.True(
            workflow.IndexOf("gcloud artifacts repositories describe maliev-web-artifact-dev", StringComparison.Ordinal) <
            workflow.IndexOf("scripts/ensure-web-image-tag.sh", StringComparison.Ordinal));
        Assert.DoesNotContain("maliev-website-artifact-dev", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("credentials_json", workflow, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("FROM build-base AS restore-attested", dockerfile, StringComparison.Ordinal);
        Assert.Contains("id=ci_packages,required=true", dockerfile, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies release tags promote the verified development digest to staging without rebuilding.
    /// </summary>
    [Fact]
    public void StagingPromotesExactDevelopmentDigestWithoutRebuild()
    {
        var workflow = ReadRepoFile(".github", "workflows", "ci-staging.yml");

        Assert.Contains("tags: [release/v*.*.*]", workflow, StringComparison.Ordinal);
        Assert.Contains("group: web-staging-promotion", workflow, StringComparison.Ordinal);
        Assert.Contains("cancel-in-progress: true", workflow, StringComparison.Ordinal);
        Assert.Contains("environment: staging", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_STAGING_WORKLOAD_IDENTITY_PROVIDER", workflow, StringComparison.Ordinal);
        Assert.Contains("GCP_STAGING_SERVICE_ACCOUNT", workflow, StringComparison.Ordinal);
        Assert.Contains("maliev-web-artifact-dev/maliev-web", workflow, StringComparison.Ordinal);
        Assert.Contains("maliev-web-artifact-staging/maliev-web", workflow, StringComparison.Ordinal);
        Assert.Contains("gcloud artifacts repositories describe maliev-web-artifact-staging", workflow, StringComparison.Ordinal);
        Assert.Contains("--format='value(immutableTags)'", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$immutable_tags\" = \"True\"", workflow, StringComparison.Ordinal);
        Assert.Contains("version=${GITHUB_REF_NAME#release/v}", workflow, StringComparison.Ordinal);
        Assert.Contains("source_tag=dev-${GITHUB_SHA::12}", workflow, StringComparison.Ordinal);
        Assert.Contains("org.opencontainers.image.revision", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/verify-web-image-attestations.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/ensure-web-image-tag.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.True(
            workflow.IndexOf("gcloud artifacts repositories describe maliev-web-artifact-staging", StringComparison.Ordinal) <
            workflow.IndexOf("scripts/ensure-web-image-tag.sh", StringComparison.Ordinal));
        Assert.DoesNotContain("maliev-website-artifact-staging", workflow, StringComparison.Ordinal);
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
        Assert.Contains("maliev-web-artifact-staging/maliev-web", workflow, StringComparison.Ordinal);
        Assert.Contains("maliev-web-artifact-prod/maliev-web", workflow, StringComparison.Ordinal);
        Assert.Contains("gcloud artifacts repositories describe maliev-web-artifact-prod", workflow, StringComparison.Ordinal);
        Assert.Contains("--format='value(immutableTags)'", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$immutable_tags\" = \"True\"", workflow, StringComparison.Ordinal);
        Assert.Contains("staging_digest=", workflow, StringComparison.Ordinal);
        Assert.Contains("test \"$staging_digest\" = \"$APPROVED_DIGEST\"", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/verify-web-image-attestations.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/ensure-web-image-tag.sh", workflow, StringComparison.Ordinal);
        Assert.Contains("scripts/update-web-gitops-overlay.sh", workflow, StringComparison.Ordinal);
        Assert.True(
            workflow.IndexOf("gcloud artifacts repositories describe maliev-web-artifact-prod", StringComparison.Ordinal) <
            workflow.IndexOf("scripts/ensure-web-image-tag.sh", StringComparison.Ordinal));
        Assert.DoesNotContain("maliev-website-artifact-prod", workflow, StringComparison.Ordinal);
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
        Assert.Contains("kustomize edit remove patch --path build-metadata-patch.yaml", script, StringComparison.Ordinal);
        Assert.Contains("kustomize edit add patch --path build-metadata-patch.yaml", script, StringComparison.Ordinal);
        Assert.DoesNotContain("sed -i", script, StringComparison.Ordinal);
        Assert.DoesNotContain("argocd/environments", script, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies immutable tag creation distinguishes absence from lookup failures and never overwrites conflicts.
    /// </summary>
    [Fact]
    public void TagAndAttestationScriptsFailClosedAndVerifyAttestationPayloads()
    {
        var tagScript = ReadRepoFile("scripts", "ensure-web-image-tag.sh");
        var attestationScript = ReadRepoFile("scripts", "verify-web-image-attestations.sh");

        Assert.Contains("MANIFEST_UNKNOWN", tagScript, StringComparison.Ordinal);
        Assert.Contains("already resolves to the requested digest", tagScript, StringComparison.Ordinal);
        Assert.Contains("Refusing to overwrite immutable tag", tagScript, StringComparison.Ordinal);
        Assert.Contains("crane copy", tagScript, StringComparison.Ordinal);
        Assert.Contains("crane blob", attestationScript, StringComparison.Ordinal);
        Assert.Contains("vnd.docker.reference.digest", attestationScript, StringComparison.Ordinal);
        Assert.Contains("non_attestation_subjects", attestationScript, StringComparison.Ordinal);
        Assert.Contains("orphan attestation subject", attestationScript, StringComparison.Ordinal);
        Assert.Contains(".subject", attestationScript, StringComparison.Ordinal);
        Assert.Contains("https://spdx.dev/Document", attestationScript, StringComparison.Ordinal);
        Assert.Contains("SPDXID", attestationScript, StringComparison.Ordinal);
        Assert.Contains("spdxVersion", attestationScript, StringComparison.Ordinal);
        Assert.Contains("dataLicense", attestationScript, StringComparison.Ordinal);
        Assert.Contains("documentNamespace", attestationScript, StringComparison.Ordinal);
        Assert.Contains("https://slsa.dev/provenance/v1", attestationScript, StringComparison.Ordinal);
        Assert.Contains("buildDefinition", attestationScript, StringComparison.Ordinal);
        Assert.Contains("buildType", attestationScript, StringComparison.Ordinal);
        Assert.Contains("externalParameters", attestationScript, StringComparison.Ordinal);
        Assert.Contains("internalParameters", attestationScript, StringComparison.Ordinal);
        Assert.Contains("resolvedDependencies", attestationScript, StringComparison.Ordinal);
        Assert.Contains("sha1", attestationScript, StringComparison.Ordinal);
        Assert.Contains("runDetails", attestationScript, StringComparison.Ordinal);
        Assert.Contains("invocationId", attestationScript, StringComparison.Ordinal);
        Assert.Contains("source_revision", attestationScript, StringComparison.Ordinal);
        Assert.Contains("expected_source_uri", attestationScript, StringComparison.Ordinal);
    }

    /// <summary>
    /// Runs behavioral shell fixtures for immutable tags, attestation payloads, and Kustomize variants.
    /// </summary>
    [Fact]
    public void SupplyChainShellContractsPassBehavioralFixtures()
    {
        var repoRoot = FindRepoRoot();
        var scriptPath = Path.Combine(repoRoot, "scripts", "tests", "immutable-web-supply-chain.test.sh");
        Assert.True(File.Exists(scriptPath), $"Expected shell contract test '{scriptPath}'.");

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "bash",
            ArgumentList = { "scripts/tests/immutable-web-supply-chain.test.sh" },
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        });
        Assert.NotNull(process);
        var standardOutput = process.StandardOutput.ReadToEnd();
        var standardError = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.True(
            process.ExitCode == 0,
            $"Shell contract fixtures failed with exit code {process.ExitCode}.\nstdout:\n{standardOutput}\nstderr:\n{standardError}");
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
            Assert.Contains("version: v0.20.6", workflow, StringComparison.Ordinal);
            Assert.Contains("kustomize-version: 5.7.1", workflow, StringComparison.Ordinal);
            Assert.Contains("Concurrent GitOps update already published the same overlay", workflow, StringComparison.Ordinal);
            Assert.Contains("git diff --quiet \"origin/$branch\" HEAD", workflow, StringComparison.Ordinal);
            Assert.Contains("git fetch origin \"+$branch:refs/remotes/origin/$branch\"", workflow, StringComparison.Ordinal);
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
