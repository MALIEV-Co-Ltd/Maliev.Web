using Maliev.Web.Client.Services;
using Maliev.Web.Shared.Localization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddLocalization();
builder.Services.AddMudServices();
builder.Services.AddHttpClient("MalievAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
});
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("MalievAPI"));
builder.Services.AddScoped<MalievApiClient>();
builder.Services.AddScoped<PreferenceService>();
builder.Services.AddScoped<CartState>();

var host = builder.Build();
var js = host.Services.GetRequiredService<IJSRuntime>();
var culture = await js.InvokeAsync<string>("malievCulture.resolveCulture", SupportedCultures.DefaultCulture);
SupportedCultures.Apply(culture);

await host.RunAsync();
