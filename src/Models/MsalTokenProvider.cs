using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;
using AspNetCoreVerifiableCredentials;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public class MsalTokenProvider
{
    protected readonly IConfiguration _config;
    protected readonly AppSettingsModel _appSettings;
    public readonly IConfidentialClientApplication _client;

    public MsalTokenProvider(IOptions<AppSettingsModel> appSettings, IConfiguration configuration)
    {
        _appSettings = appSettings.Value;
        var clientSecret = string.Empty;
        if(_appSettings.UseKeyVaultForSecrets == true)
        {
            var keyvaultClient = new SecretClient(
                new Uri($"https://{_appSettings.KeyVaultName}.vault.azure.net"), 
                new ChainedTokenCredential(
                    new AzureCliCredential(),
                    new ManagedIdentityCredential()
            ));
            var secret = keyvaultClient.GetSecret("ClientSecret").Value;
            clientSecret = secret.Value;
        }
        else
        {
            clientSecret = _appSettings.ClientSecret;
        }

        if(_appSettings.AppUsesClientSecret())
        {
            _client = ConfidentialClientApplicationBuilder.Create(_appSettings.ClientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(_appSettings.Authority))
                .Build();
        }
        else
        {
            var certificate = _appSettings.ReadCertificate(_appSettings.CertificateName);
            _client = ConfidentialClientApplicationBuilder.Create(_appSettings.ClientId)
                .WithCertificate(certificate)
                .WithAuthority(new Uri(_appSettings.Authority))
                .Build();
        }

        _client.AddDistributedTokenCache(services =>
        {
            services.AddDistributedMemoryCache();
            services.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
        });
    }
}