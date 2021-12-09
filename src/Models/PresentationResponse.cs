using System.Text.Json.Serialization;

namespace AspNetCoreVerifiableCredentials.PresentationModel
{
    public class Claims
    {
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }

    public class Issuer
    {
        [JsonPropertyName("type")]
        public List<string> Type { get; set; }
        [JsonPropertyName("claims")]
        public Claims Claims { get; set; }
        
        [JsonPropertyName("domain")]
        public string Domain { get; set; }
        
        [JsonPropertyName("verified")]
        public string Verified { get; set; }
        
        [JsonPropertyName("issuer")]
        public string VCIssuer { get; set; }
    }

    public class Receipt
    {
        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }
    }

    public class PresentationCallback
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }
        
        [JsonPropertyName("code")]
        public string Code { get; set; }
        
        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("subject")]
        public string Subject { get; set; }
        
        [JsonPropertyName("issuers")]
        public List<Issuer> Issuers { get; set; }
        
        [JsonPropertyName("receipt")]
        public Receipt Receipt { get; set; }
    }
}