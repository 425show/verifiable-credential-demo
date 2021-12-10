using System.Text.Json.Serialization;

public class DidConfiguration
{
    [JsonPropertyName("@context")]
    public string Context { get; set; }

    [JsonPropertyName("linked_dids")]
    public List<string> LinkedDids { get; set; }
}