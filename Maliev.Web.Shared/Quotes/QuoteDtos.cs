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
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the browser file size in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Gets or sets the content type reported by the browser.</summary>
    public string ContentType { get; set; } = "application/octet-stream";
}

/// <summary>
/// A part being quoted.
/// </summary>
public sealed class QuotePartDraftDto
{
    /// <summary>Gets or sets the temporary part id.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the completed UploadService file identifier.</summary>
    public Guid? FileId { get; set; }

    /// <summary>Gets or sets the UploadService upload session identifier.</summary>
    public string? UploadId { get; set; }

    /// <summary>Gets or sets the storage path assigned by UploadService.</summary>
    public string? StoragePath { get; set; }

    /// <summary>Gets or sets the browser file identity.</summary>
    public QuoteFileDraftDto File { get; set; } = new();

    /// <summary>Gets or sets the downstream manufacturing process identifier.</summary>
    public Guid? ManufacturingProcessId { get; set; }

    /// <summary>Gets or sets the selected process code.</summary>
    public string ProcessCode { get; set; } = "FDM";

    /// <summary>Gets or sets the downstream material identifier.</summary>
    public Guid? MaterialId { get; set; }

    /// <summary>Gets or sets the selected material code.</summary>
    public string MaterialCode { get; set; } = "PLA";

    /// <summary>Gets or sets the downstream surface finish identifier.</summary>
    public Guid? SurfaceFinishId { get; set; }

    /// <summary>Gets or sets the selected surface finish code.</summary>
    public string SurfaceFinishCode { get; set; } = string.Empty;

    /// <summary>Gets or sets selected process-specific option values keyed by MaterialService config key.</summary>
    public Dictionary<string, string> ProcessOptionValues { get; set; } = [];

    /// <summary>Gets or sets the requested quantity.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>Gets or sets the analyzed model volume in cubic centimeters.</summary>
    public decimal EstimatedVolumeCc { get; set; }

    /// <summary>Gets or sets whether the customer acknowledged DFM warnings.</summary>
    public bool DfmAcknowledged { get; set; }
}

/// <summary>
/// Request for a quote estimate.
/// </summary>
public sealed class QuoteEstimateRequest
{
    /// <summary>Gets or sets the authenticated customer id when the quote is attached to an account.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Gets or sets the requested currency code.</summary>
    public string CurrencyCode { get; set; } = "THB";

    /// <summary>Gets or sets the requested culture.</summary>
    public string Culture { get; set; } = SupportedCultures.DefaultCulture;

    /// <summary>Gets or sets the lead-time code.</summary>
    public string LeadTimeCode { get; set; } = "STANDARD";

    /// <summary>Gets or sets the part drafts to estimate.</summary>
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
    public string FileName { get; set; } = string.Empty;

    /// <summary>Gets or sets the browser content type.</summary>
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
}
