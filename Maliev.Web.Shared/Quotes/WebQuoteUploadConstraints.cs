namespace Maliev.Web.Shared.Quotes;

/// <summary>
/// Public website quote upload constraints enforced before UploadService session allocation.
/// </summary>
public static class WebQuoteUploadConstraints
{
    /// <summary>Maximum customer quote upload size accepted by the public website.</summary>
    public const long MaxFileSizeBytes = 200L * 1024 * 1024;

    /// <summary>Maximum customer quote upload size in megabytes for user-facing problem details.</summary>
    public const int MaxFileSizeMegabytes = 200;

    /// <summary>CAD file extensions accepted by the public website quote handoff.</summary>
    public static IReadOnlyList<string> SupportedExtensions { get; } =
    [
        "stl",
        "step",
        "stp",
        "3mf",
        "obj",
        "igs",
        "iges",
        "gltf",
        "glb",
        "ply",
        "off",
        "amf",
        "wrl",
        "x3d",
        "x_t",
        "x_b",
        "sat",
        "sab",
        "sldprt",
        "sldasm",
        "prt",
        "asm",
        "catpart",
        "catproduct",
        "jt",
        "3dxml",
        "3dm",
        "brep"
    ];

    private static readonly HashSet<string> SupportedExtensionLookup = new(SupportedExtensions, StringComparer.OrdinalIgnoreCase);

    /// <summary>Human-readable extension list for upload validation messages.</summary>
    public static string SupportedExtensionLabel => string.Join(", ", SupportedExtensions.Select(extension => extension.ToUpperInvariant()));

    /// <summary>Determines whether the supplied file name has a supported CAD extension.</summary>
    public static bool IsSupportedFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.');
        return !string.IsNullOrWhiteSpace(extension) && SupportedExtensionLookup.Contains(extension);
    }
}
