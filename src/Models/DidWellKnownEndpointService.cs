using System.Text.Json;

public static class DidWellKnownEndpointService
{
    private static DidConfiguration _configuration = null;
    
    public static async Task<DidConfiguration> GetDidConfiguration()
    {
       
        if (_configuration == null)
        {
            var json = await File.ReadAllTextAsync("did-configuration.json");
            _configuration =  JsonSerializer.Deserialize<DidConfiguration>(json);
        }

        return _configuration;
    }
}