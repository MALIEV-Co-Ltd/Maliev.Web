using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Maliev.Web.Shared.Localization;

namespace Maliev.Web.Shared.Quotes;

/// <summary>
/// Customer-visible process option used by the instant quote surface.
/// </summary>
public sealed class ServiceProcessDto
{
    /// <summary>Gets or sets the downstream manufacturing process identifier.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the process code used by downstream pricing and geometry services.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized process name.</summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>Gets or sets the localized process summary.</summary>
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Gets or sets whether the process can return instant pricing.</summary>
    public bool SupportsInstantQuote { get; set; }
}

/// <summary>
/// Customer-visible material option.
/// </summary>
public sealed class MaterialOptionDto
{
    /// <summary>Gets or sets the downstream material identifier.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the material code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the process code this material belongs to.</summary>
    public string ProcessCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized material name.</summary>
    public LocalizedText Name { get; set; } = new();
}

/// <summary>
/// Customer-visible surface finish option.
/// </summary>
public sealed class SurfaceFinishOptionDto
{
    /// <summary>Gets or sets the downstream surface finish identifier.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the surface finish code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the process code this surface finish belongs to.</summary>
    public string ProcessCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized surface finish name.</summary>
    public LocalizedText Name { get; set; } = new();

    /// <summary>Gets or sets the localized surface finish summary.</summary>
    public LocalizedText Summary { get; set; } = new();

    /// <summary>Gets or sets the surface roughness Ra value in micrometers when applicable.</summary>
    public decimal? RaValueUm { get; set; }

    /// <summary>Gets or sets the additional cost percentage.</summary>
    public decimal AdditionalCostPercent { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }

    /// <summary>Gets or sets the material identifiers this finish is compatible with.</summary>
    public List<Guid> CompatibleMaterialIds { get; set; } = [];
}

/// <summary>
/// Customer-visible process configuration option.
/// </summary>
public sealed class ProcessConfigOptionDto
{
    /// <summary>Gets or sets the downstream option identifier.</summary>
    public Guid? Id { get; set; }

    /// <summary>Gets or sets the process code this option belongs to.</summary>
    public string ProcessCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the machine-readable configuration key.</summary>
    public string ConfigKey { get; set; } = string.Empty;

    /// <summary>Gets or sets the localized option label.</summary>
    public LocalizedText Label { get; set; } = new();

    /// <summary>Gets or sets the control type, such as dropdown, toggle, number, or text.</summary>
    public string ConfigType { get; set; } = string.Empty;

    /// <summary>Gets or sets the default value represented as a string.</summary>
    public string? DefaultValue { get; set; }

    /// <summary>Gets or sets the JSON-encoded dropdown options.</summary>
    public string? OptionsJson { get; set; }

    /// <summary>Gets or sets the display unit suffix.</summary>
    public string? Unit { get; set; }

    /// <summary>Gets or sets the localized helper text.</summary>
    public LocalizedText HelpText { get; set; } = new();

    /// <summary>Gets or sets whether the option is required.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Reference data needed to configure a quote.
/// </summary>
public sealed class QuoteReferenceDataDto
{
    /// <summary>Gets or sets manufacturing process options.</summary>
    public List<ServiceProcessDto> Processes { get; set; } = [];

    /// <summary>Gets or sets material options.</summary>
    public List<MaterialOptionDto> Materials { get; set; } = [];

    /// <summary>Gets or sets surface finish options.</summary>
    public List<SurfaceFinishOptionDto> SurfaceFinishes { get; set; } = [];

    /// <summary>Gets or sets process configuration options.</summary>
    public List<ProcessConfigOptionDto> ProcessOptions { get; set; } = [];

    /// <summary>Gets or sets supported lead-time codes.</summary>
    public List<string> LeadTimeCodes { get; set; } = [];
}

/// <summary>
/// A browser file selected for quote analysis.
/// </summary>
public sealed class QuoteFileDraftDto
{
    /// <summary>Gets or sets the browser file name.</summary>
    [MaxLength(WebQuoteUploadConstraints.MaxFileNameLength)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the browser file size in bytes.</summary>
    [Range(1, WebQuoteUploadConstraints.MaxFileSizeBytes)]
    public long SizeBytes { get; set; }

    /// <summary>Gets or sets the content type reported by the browser.</summary>
    [MaxLength(128)]
    public string ContentType { get; set; } = "application/octet-stream";
}

/// <summary>
/// A part being quoted.
/// </summary>
public sealed class QuotePartDraftDto : IValidatableObject
{
    /// <summary>Gets or sets the temporary part id.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the completed UploadService file identifier.</summary>
    public Guid? FileId { get; set; }

    /// <summary>Gets or sets the UploadService upload session identifier.</summary>
    [MaxLength(128)]
    public string? UploadId { get; set; }

    /// <summary>Gets or sets the short-lived Web upload capability retained only by this browser.</summary>
    [MaxLength(2_048)]
    public string? UploadCapability { get; set; }

    /// <summary>Gets or sets the storage path assigned by UploadService.</summary>
    [MaxLength(512)]
    public string? StoragePath { get; set; }

    /// <summary>Gets or sets the browser file identity.</summary>
    [Required]
    public QuoteFileDraftDto File { get; set; } = new();

    /// <summary>Gets or sets the downstream manufacturing process identifier.</summary>
    public Guid? ManufacturingProcessId { get; set; }

    /// <summary>Gets or sets the selected process code.</summary>
    [MaxLength(64)]
    public string ProcessCode { get; set; } = "FDM";

    /// <summary>Gets or sets the downstream material identifier.</summary>
    public Guid? MaterialId { get; set; }

    /// <summary>Gets or sets the selected material code.</summary>
    [MaxLength(64)]
    public string MaterialCode { get; set; } = "PLA";

    /// <summary>Gets or sets the downstream surface finish identifier.</summary>
    public Guid? SurfaceFinishId { get; set; }

    /// <summary>Gets or sets the selected surface finish code.</summary>
    [MaxLength(64)]
    public string SurfaceFinishCode { get; set; } = string.Empty;

    /// <summary>Gets or sets selected process-specific option values keyed by MaterialService config key.</summary>
    [MaxLength(32)]
    public Dictionary<string, string> ProcessOptionValues { get; set; } = [];

    /// <summary>Gets or sets the requested quantity.</summary>
    [Range(1, 100_000)]
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the analyzed model volume in cubic centimeters.</summary>
    public decimal EstimatedVolumeCc { get; set; }

    /// <summary>Gets or sets server-resolved geometry volume; never accepted from browser JSON.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeVolumeCc { get; set; }

    /// <summary>Gets or sets server-resolved support volume; never accepted from browser JSON.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeSupportVolumeCc { get; set; }

    /// <summary>Gets or sets server-resolved surface area; never accepted from browser JSON.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeSurfaceAreaCm2 { get; set; }

    /// <summary>Gets or sets server-resolved X extent in millimeters.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeBoundingBoxX { get; set; }

    /// <summary>Gets or sets server-resolved Y extent in millimeters.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeBoundingBoxY { get; set; }

    /// <summary>Gets or sets server-resolved Z extent in millimeters.</summary>
    [JsonIgnore]
    public decimal? AuthoritativeBoundingBoxZ { get; set; }

    /// <summary>Gets or sets server-resolved mesh manifold state.</summary>
    [JsonIgnore]
    public bool? AuthoritativeIsManifold { get; set; }

    /// <summary>Gets or sets server-resolved triangle count.</summary>
    [JsonIgnore]
    public int? AuthoritativeTriangleCount { get; set; }

    /// <summary>Gets or sets whether the customer acknowledged DFM warnings.</summary>
    public bool DfmAcknowledged { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ProcessOptionValues is null || ProcessOptionValues.Any(option =>
                string.IsNullOrWhiteSpace(option.Key) ||
                option.Key.Length > 80 ||
                option.Value is null ||
                option.Value.Length > 400))
        {
            yield return new ValidationResult(
                "Process option keys and values exceed the supported quote limits.",
                [nameof(ProcessOptionValues)]);
        }
    }
}

/// <summary>
/// Request for a quote estimate.
/// </summary>
public sealed class QuoteEstimateRequest
{
    /// <summary>Gets or sets the authenticated customer id when the quote is attached to an account.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the requested currency code.</summary>
    [MaxLength(8)]
    public string CurrencyCode { get; set; } = "THB";

    /// <summary>Gets or sets the requested culture.</summary>
    [MaxLength(16)]
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;

    /// <summary>Gets or sets the lead-time code.</summary>
    [MaxLength(64)]
    public string LeadTimeCode { get; set; } = "STANDARD";

    /// <summary>Gets or sets the part drafts to estimate.</summary>
    [Required]
    [MaxLength(WebQuoteUploadConstraints.MaxPartsPerEstimate)]
    public List<QuotePartDraftDto> Parts { get; set; } = [];
}

/// <summary>
/// One estimated quote line.
/// </summary>
public sealed class QuoteLineEstimateDto
{
    /// <summary>Gets or sets the part id.</summary>
    public Guid PartId { get; set; }

    /// <summary>Gets or sets the display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the base unit price before bulk discount.</summary>
    public decimal BaseUnitPrice { get; set; }

    /// <summary>Gets or sets the final unit price after bulk discount.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets the explicit bulk discount amount.</summary>
    public decimal BulkDiscountAmount { get; set; }

    /// <summary>Gets or sets the line total.</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets any customer-visible DFM warning.</summary>
    public string? Warning { get; set; }
}

/// <summary>
/// Response returned by the Web BFF estimate endpoint.
/// </summary>
public sealed class QuoteEstimateResponse
{
    /// <summary>Gets or sets quote estimate lines.</summary>
    public List<QuoteLineEstimateDto> Lines { get; set; } = [];

    /// <summary>Gets or sets the quote subtotal.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Gets or sets the explicit bulk discount total.</summary>
    public decimal BulkDiscountTotal { get; set; }

    /// <summary>Gets or sets the estimated tax.</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Gets or sets the quote total.</summary>
    public decimal Total { get; set; }

    /// <summary>Gets or sets whether manual review is required.</summary>
    public bool RequiresManualReview { get; set; }

    /// <summary>Gets or sets the pricing source description.</summary>
    public string PricingSource { get; set; } = string.Empty;
}

/// <summary>
/// Request to initiate a resumable upload through the Web BFF.
/// </summary>
public sealed class WebUploadInitiationRequest
{
    /// <summary>Gets or sets the browser file name.</summary>
    [Required]
    [MaxLength(WebQuoteUploadConstraints.MaxFileNameLength)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the browser content type.</summary>
    [MaxLength(128)]
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Gets or sets the browser file size.</summary>
    public long FileSize { get; set; }

    /// <summary>Gets or sets the anonymous quote session id.</summary>
    public Guid QuoteSessionId { get; set; }
}

/// <summary>
/// Response returned after upload initiation.
/// </summary>
public sealed class WebUploadInitiationResponse
{
    /// <summary>Gets or sets the upload id.</summary>
    public string UploadId { get; set; } = string.Empty;

    /// <summary>Gets or sets the resumable upload proxy URL.</summary>
    public string ProxyUploadUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the storage path assigned by the upload boundary.</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the short-lived bearer proof required for this upload lifecycle.</summary>
    public string UploadCapability { get; set; } = string.Empty;
}

/// <summary>
/// Response returned after UploadService confirms a resumable upload.
/// </summary>
public sealed class WebUploadCompleteResponse
{
    /// <summary>Gets or sets the upload id.</summary>
    public string UploadId { get; set; } = string.Empty;

    /// <summary>Gets or sets the parsed file id when UploadService uses a GUID upload id.</summary>
    public Guid? FileId { get; set; }

    /// <summary>Gets or sets the uploaded file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the storage path assigned by UploadService.</summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the current UploadService status.</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Request to sign completed Web uploads for QuoteEngine handoff.
/// </summary>
public sealed class WebUploadHandoffTokenRequest
{
    /// <summary>Gets or sets the anonymous quote session id.</summary>
    public Guid QuoteSessionId { get; set; }

    /// <summary>Gets or sets the completed files to hand off.</summary>
    [Required]
    public List<WebUploadHandoffFileDto> Files { get; set; } = [];
}

/// <summary>
/// Completed Web upload metadata included in a signed QuoteEngine handoff.
/// </summary>
public sealed class WebUploadHandoffFileDto
{
    /// <summary>Gets or sets the UploadService upload id.</summary>
    [Required]
    [MaxLength(128)]
    public string UploadId { get; set; } = string.Empty;

    /// <summary>Gets or sets the short-lived Web upload proof; it is removed from the QuoteEngine token.</summary>
    [MaxLength(2_048)]
    public string? UploadCapability { get; set; }

    /// <summary>Gets or sets the UploadService file id when available.</summary>
    public Guid? FileId { get; set; }

    /// <summary>Gets or sets the browser file name.</summary>
    [Required]
    [MaxLength(WebQuoteUploadConstraints.MaxFileNameLength)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the UploadService storage path.</summary>
    [Required]
    [MaxLength(512)]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the browser content type.</summary>
    [MaxLength(128)]
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>Gets or sets the browser file size.</summary>
    [Range(1, WebQuoteUploadConstraints.MaxFileSizeBytes)]
    public long FileSizeBytes { get; set; }

    /// <summary>Gets or sets the UploadService status.</summary>
    [MaxLength(32)]
    public string Status { get; set; } = "Completed";
}

/// <summary>
/// Signed token that QuoteEngine can verify before importing Web uploads.
/// </summary>
public sealed class WebUploadHandoffTokenResponse
{
    /// <summary>Gets or sets the signed handoff token.</summary>
    public string HandoffToken { get; set; } = string.Empty;
}

/// <summary>
/// Upload analysis status returned to the quote page watchdog.
/// </summary>
public sealed class WebAnalysisStatusResponse
{
    /// <summary>Gets or sets the upload id.</summary>
    public string UploadId { get; set; } = string.Empty;

    /// <summary>Gets or sets the current status.</summary>
    public string Status { get; set; } = "Queued";

    /// <summary>Gets or sets whether the status is terminal.</summary>
    public bool IsTerminal { get; set; }

    /// <summary>Gets or sets an optional customer-visible message.</summary>
    public string? Message { get; set; }

    /// <summary>Gets or sets the canonical UploadService file id for server-side correlation.</summary>
    [JsonIgnore]
    public string? AuthoritativeFileId { get; set; }

    /// <summary>Gets or sets the canonical UploadService storage path for ownership checks.</summary>
    [JsonIgnore]
    public string? CanonicalStoragePath { get; set; }

    /// <summary>Gets or sets the canonical UploadService size for ownership checks.</summary>
    [JsonIgnore]
    public long? CanonicalFileSizeBytes { get; set; }

    /// <summary>Gets or sets the authoritative analyzed volume in cubic centimeters.</summary>
    public decimal? VolumeCm3 { get; set; }

    /// <summary>Gets or sets the authoritative X extent in millimeters.</summary>
    public decimal? BoundingBoxX { get; set; }

    /// <summary>Gets or sets the authoritative Y extent in millimeters.</summary>
    public decimal? BoundingBoxY { get; set; }

    /// <summary>Gets or sets the authoritative Z extent in millimeters.</summary>
    public decimal? BoundingBoxZ { get; set; }
}
