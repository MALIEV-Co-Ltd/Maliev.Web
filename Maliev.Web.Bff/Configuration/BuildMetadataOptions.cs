using System.Text.RegularExpressions;

namespace Maliev.Web.Bff.Configuration;

/// <summary>
/// Non-secret immutable metadata for the running Web build.
/// </summary>
public sealed partial class BuildMetadataOptions
{
    /// <summary>Configuration section containing build metadata.</summary>
    public const string SectionName = "BuildMetadata";

    /// <summary>Gets or sets the SemVer assigned when the image was built.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the full source commit SHA used to build the image.</summary>
    public string CommitSha { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional immutable image digest supplied by the runtime deployment.</summary>
    public string? ImageDigest { get; set; }

    /// <summary>Returns whether required image build metadata has a valid public wire shape.</summary>
    public static bool HasValidRequiredValues(BuildMetadataOptions options) =>
        SemanticVersionPattern().IsMatch(options.Version) &&
        CommitShaPattern().IsMatch(options.CommitSha);

    /// <summary>Returns whether an optional runtime image digest is absent or valid.</summary>
    public static bool HasValidOptionalImageDigest(BuildMetadataOptions options) =>
        string.IsNullOrWhiteSpace(options.ImageDigest) ||
        ImageDigestPattern().IsMatch(options.ImageDigest);

    /// <summary>Returns a valid runtime image digest, or <see langword="null"/> when none is available.</summary>
    public string? GetSafeImageDigest() =>
        !string.IsNullOrWhiteSpace(ImageDigest) && ImageDigestPattern().IsMatch(ImageDigest)
            ? ImageDigest
            : null;

    [GeneratedRegex(@"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$")]
    private static partial Regex SemanticVersionPattern();

    [GeneratedRegex("^[0-9a-f]{40}$")]
    private static partial Regex CommitShaPattern();

    [GeneratedRegex("^sha256:[0-9a-f]{64}$")]
    private static partial Regex ImageDigestPattern();
}
