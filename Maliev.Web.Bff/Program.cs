using Maliev.Aspire.ServiceDefaults;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Components;
using Maliev.Web.Bff.Services;
using Maliev.Web.Client.Services;
using Maliev.Web.Shared.Localization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddDefaultApiVersioning();
builder.AddIAMServiceClient("WebBff");

builder.Services.AddLocalization();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient("MalievAPI", (sp, client) =>
{
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
    if (httpContext is not null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}{request.PathBase}/");
    }
    else
    {
        client.BaseAddress = new Uri(builder.Configuration["PublicBaseUrl"] ?? "http://localhost/");
    }

    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MalievAPI"));
builder.Services.AddScoped<MalievApiClient>();
builder.Services.AddScoped<PreferenceService>();
builder.Services.AddScoped<CartState>();

builder.AddAuthenticatedServiceClient<IMaterialServiceClient, MaterialServiceClient>("MaterialService");
builder.AddAuthenticatedServiceClient<IPricingServiceClient, PricingServiceClient>("PricingService");
builder.AddAuthenticatedServiceClient<IUploadServiceClient, UploadServiceClient>("UploadService");
builder.AddAuthenticatedServiceClient<IOrderServiceClient, OrderServiceClient>("OrderService");
builder.AddAuthenticatedServiceClient<IPaymentServiceClient, PaymentServiceClient>("PaymentService");
builder.AddAuthenticatedServiceClient<IDeliveryServiceClient, DeliveryServiceClient>("DeliveryService");
builder.AddAuthenticatedServiceClient<ICustomerServiceClient, CustomerServiceClient>("CustomerService");
builder.AddAuthenticatedServiceClient<IContactServiceClient, ContactServiceClient>("ContactService");

builder.Services.AddHttpClient("UploadServiceStreaming", (sp, client) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var explicitUrl = configuration["Services:UploadService:BaseUrl"];
    client.BaseAddress = !string.IsNullOrWhiteSpace(explicitUrl)
        ? new Uri(explicitUrl)
        : new Uri("http://UploadService");
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { MaxConnectionsPerServer = 20 })
.AddServiceDiscovery()
.AddHttpMessageHandler<ServiceAccountAuthenticationHandler>();

builder.Services.AddSingleton<ICommerceCatalogService, MockCommerceCatalogService>();
builder.Services.AddScoped<IManufacturingCatalogService, ManufacturingCatalogService>();
builder.Services.AddScoped<IWebQuoteService, WebQuoteService>();
builder.Services.AddScoped<IQuoteUploadService, QuoteUploadService>();
builder.Services.AddScoped<ICheckoutDraftService, CheckoutDraftService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

var supportedCultures = SupportedCultures.Names.ToArray();
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(SupportedCultures.DefaultCulture)
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}
app.UseAntiforgery();

app.MapDefaultEndpoints("web");
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Maliev.Web.Client._Imports).Assembly);

app.Run();

/// <summary>
/// Marker program type for WebApplicationFactory-based tests.
/// </summary>
public partial class Program
{
}
