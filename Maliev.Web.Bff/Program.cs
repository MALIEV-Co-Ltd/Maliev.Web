using System.Security.Claims;
using Maliev.Aspire.ServiceDefaults;
using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.Web.Bff.Clients;
using Maliev.Web.Bff.Components;
using Maliev.Web.Bff.Security;
using Maliev.Web.Bff.Services;
using Maliev.Web.Client.Services;
using Maliev.Web.Shared.Localization;
using Maliev.Web.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    AddDevelopmentSharedSecretsFallback(builder);
}

builder.WebHost.UseStaticWebAssets();
builder.AddServiceDefaults();
builder.AddDefaultApiVersioning();
builder.AddIAMServiceClient("WebBff");

builder.Services.AddLocalization();
builder.Services.AddMudServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
var authentication = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = "MalievExternal";
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "__Host-Maliev.Web";
        options.LoginPath = "/auth/sign-in";
        options.LogoutPath = "/auth/sign-out";
        options.AccessDeniedPath = "/auth/sign-in";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
    })
    .AddCookie("MalievExternal", options =>
    {
        options.Cookie.Name = "__Host-Maliev.Web.External";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authentication.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.SignInScheme = "MalievExternal";
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/auth/google/signin";
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.ClaimActions.MapJsonKey("picture", "picture");
        options.SaveTokens = true;
        options.Events.OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        };
    });
}
builder.Services.AddPermissionAuthorization();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(WebAuthorizationPolicies.CustomerAccount, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("user_type", "customer");
        policy.RequireAssertion(context => HasValidCustomerId(context.User));
    });
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddTransient<InternalBrowserCookieForwardingHandler>();

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
})
.AddHttpMessageHandler<InternalBrowserCookieForwardingHandler>();
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
builder.AddAuthenticatedServiceClient<ICustomerServiceClient, CustomerServiceClient>("CustomerService")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.AddAuthenticatedServiceClient<IAuthServiceClient, AuthServiceClient>("AuthService");
builder.AddAuthenticatedServiceClient<ICountryServiceClient, CountryServiceClient>("CountryService");
builder.AddAuthenticatedServiceClient<IRegistryServiceClient, RegistryServiceClient>("RegistryService");
builder.AddAuthenticatedServiceClient<IContactServiceClient, ContactServiceClient>("ContactService");
builder.AddAuthenticatedServiceClient<ICommerceServiceClient, CommerceServiceClient>("CommerceService")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(10));
builder.AddAuthenticatedServiceClient<IPdfServiceClient, PdfServiceClient>("PdfService")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(30));
builder.AddServiceClient<IChatbotServiceClient, ChatbotServiceClient>("ChatbotService")
    .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(45));

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

builder.Services.AddScoped<ICommerceCatalogService, CommerceCatalogService>();
builder.Services.AddScoped<IManufacturingCatalogService, ManufacturingCatalogService>();
builder.Services.AddScoped<IWebQuoteService, WebQuoteService>();
builder.Services.AddScoped<IQuoteUploadService, QuoteUploadService>();
builder.Services.AddScoped<ICheckoutDraftService, CheckoutDraftService>();
builder.Services.AddScoped<IContactMessageService, ContactMessageService>();
builder.Services.AddScoped<ICustomerChatbotService, CustomerChatbotService>();
builder.Services.AddScoped<CustomerAssistantHandoffCookie>();
builder.Services.AddScoped<BlogEbookPdfService>();

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
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapDefaultEndpoints("web");
app.MapControllers();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Maliev.Web.Client._Imports).Assembly);

app.Run();

static void AddDevelopmentSharedSecretsFallback(WebApplicationBuilder builder)
{
    var sharedSecretsPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "..",
        "Maliev.Aspire",
        "Maliev.Aspire.AppHost",
        "sharedsecrets.json");
    if (!File.Exists(sharedSecretsPath))
    {
        return;
    }

    var sharedSecrets = new ConfigurationBuilder()
        .AddJsonFile(sharedSecretsPath, optional: true, reloadOnChange: true)
        .Build();

    ApplyMissingSharedSecretValues(builder.Configuration, sharedSecrets);
}

static void ApplyMissingSharedSecretValues(IConfiguration target, IConfiguration fallback)
{
    foreach (var section in fallback.GetChildren())
    {
        ApplyMissingSharedSecretValue(target, section, section.Key);
    }
}

static void ApplyMissingSharedSecretValue(IConfiguration target, IConfigurationSection fallbackSection, string path)
{
    var children = fallbackSection.GetChildren().ToArray();
    if (children.Length > 0)
    {
        foreach (var child in children)
        {
            ApplyMissingSharedSecretValue(target, child, $"{path}:{child.Key}");
        }

        return;
    }

    if (string.IsNullOrWhiteSpace(target[path]) && !string.IsNullOrWhiteSpace(fallbackSection.Value))
    {
        target[path] = fallbackSection.Value;
    }
}

static bool HasValidCustomerId(ClaimsPrincipal user)
{
    return Guid.TryParse(user.FindFirst("customer_id")?.Value, out _);
}

/// <summary>
/// Marker program type for WebApplicationFactory-based tests.
/// </summary>
public partial class Program
{
}
