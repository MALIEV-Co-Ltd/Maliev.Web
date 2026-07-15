namespace Maliev.Web.Shared.Deployment;

/// <summary>
/// Safe immutable metadata describing the running Web build.
/// </summary>
/// <param name="Version">The image build's semantic version.</param>
/// <param name="CommitSha">The full source commit SHA used to build the image.</param>
/// <param name="ImageDigest">The optional runtime-supplied immutable image digest.</param>
public sealed record BuildMetadataResponse(
    string Version,
    string CommitSha,
    string? ImageDigest);
