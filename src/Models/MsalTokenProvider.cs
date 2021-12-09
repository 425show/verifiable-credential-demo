using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Client;
using AspNetCoreVerifiableCredentials;

public class MsalTokenProvider
{
    protected readonly IConfiguration _config;
    protected readonly AppSettingsModel _appSettings;
    public readonly IConfidentialClientApplication _client;

    public MsalTokenProvider(IOptions<AppSettingsModel> appSettings, IConfiguration configuration)
    {
        _appSettings = appSettings.Value;
        _config = configuration;
            
        _client = ConfidentialClientApplicationBuilder.Create(_appSettings.ClientId)
            .WithClientSecret(_config["ClientSecret"])
            .WithAuthority(new Uri(_appSettings.Authority))
            .Build();

        _client.AddDistributedTokenCache(services =>
            {
                services.AddDistributedMemoryCache();
                services.AddLogging(configure => configure.AddConsole())
                .Configure<LoggerFilterOptions>(options => options.MinLevel = Microsoft.Extensions.Logging.LogLevel.Debug);
            });
    }
}