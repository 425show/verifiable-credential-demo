using AspNetCoreVerifiableCredentials;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddOptions<AppSettingsModel>()
    .Configure<IConfiguration>((options, configuration) =>
        configuration.GetSection("AppSettings").Bind(options));

builder.Services.AddSingleton<MsalTokenProvider>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);//You can set Time   
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => false;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddHttpClient();

var app = builder.Build();


app.MapGet("/.well-known/did-configuration.json", async () =>
{
    return await DidWellKnownEndpointService.GetDidConfiguration();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.Run();