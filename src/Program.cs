using AspNetCoreVerifiableCredentials;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddAzureKeyVault(
    new Uri("https://kv425show.vault.azure.net"),
    new ChainedTokenCredential(
        new AzureCliCredential(),
        new ManagedIdentityCredential()
    ),
    new KeyVaultSecretManager()
);

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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.Run();