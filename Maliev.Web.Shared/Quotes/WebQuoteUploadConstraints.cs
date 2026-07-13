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

    /// <summary>Maximum signed handoff token length accepted by QuoteEngine.</summary>
    public const int MaxQuoteEngineHandoffTokenLength = 20_000;

    /// <summary>Maximum parts accepted by one public estimate request.</summary>
    public const int MaxPartsPerEstimate = 20;

    /// <summary>Maximum completed files resolved for one signed handoff.</summary>
    public const int MaxFilesPerHandoff = 20;

    /// <summary>Maximum browser file-name length accepted at the public boundary.</summary>
    public const int MaxFileNameLength = 255;

    /// <summary>CAD and 3D file extensions accepted by the public website quote handoff.</summary>
    public static IReadOnlyList<string> SupportedCadExtensions { get; } =
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

    /// <summary>Drawing and office document extensions accepted as supplemental Make Studio context.</summary>
    public static IReadOnlyList<string> SupplementalDocumentExtensions { get; } =
    [
        "pdf",
        "dxf",
        "dwg"
    ];

    /// <summary>Image extensions accepted as supplemental Make Studio context.</summary>
    public static IReadOnlyList<string> SupplementalImageExtensions { get; } =
    [
        "jpg",
        "jpeg",
        "png",
        "webp",
        "heic"
    ];

    /// <summary>Archive extensions accepted for bundled quote attachments.</summary>
    public static IReadOnlyList<string> SupplementalArchiveExtensions { get; } =
    [
        "zip"
    ];

    /// <summary>All file extensions accepted by the public website quote handoff.</summary>
    public static IReadOnlyList<string> SupportedExtensions { get; } =
        SupportedCadExtensions
            .Concat(SupplementalDocumentExtensions)
            .Concat(SupplementalImageExtensions)
            .Concat(SupplementalArchiveExtensions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static readonly HashSet<string> SupportedExtensionLookup = new(SupportedExtensions, StringComparer.OrdinalIgnoreCase);

    /// <summary>Human-readable extension list for upload validation messages.</summary>
    public static string SupportedExtensionLabel => string.Join(", ", SupportedExtensions.Select(extension => extension.ToUpperInvariant()));

    /// <summary>Determines whether the supplied file name has a supported Make Studio attachment extension.</summary>
    public static bool IsSupportedFileName(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.');
        return !string.IsNullOrWhiteSpace(extension) && SupportedExtensionLookup.Contains(extension);
    }
}
