using System.Text.Json;
using System.Text.Json.Serialization;

public class CredentialTypeHelper
{
    private List<CredentialType> _credentialTypes;

    public List<CredentialType> GetCredentialTypes()
    {
        if(_credentialTypes == null || _credentialTypes.Count == 0)
        {
            _credentialTypes = JsonSerializer.Deserialize<List<CredentialType>>(File.ReadAllText("credentialTypes.json"));
        }
        return _credentialTypes;
    }

}
public class CredentialType
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}