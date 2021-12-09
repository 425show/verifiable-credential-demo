using System.Text.Json.Serialization;

public class VcResponse
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("expiry")]
    public long Expiry { get; set; }

    public string Pin { get; set; }
}

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Headers
{
    [JsonPropertyName("api-key")]
    public string ApiKey { get; set; }
}

public class Callback
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("headers")]
    public Headers Headers { get; set; }
}

public class Registration
{
    [JsonPropertyName("clientName")]
    public string ClientName { get; set; }
}

public class Pin
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("length")]
    public int Length { get; set; }
}

public class Claims
{
    [JsonPropertyName("given_name")]
    public string GivenName { get; set; }

    [JsonPropertyName("family_name")]
    public string FamilyName { get; set; }
}

public class Issuance
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("manifest")]
    public string Manifest { get; set; }

    [JsonPropertyName("pin")]
    public Pin Pin { get; set; }

    [JsonPropertyName("claims")]
    public Claims Claims { get; set; }
}

public class VcRequest
{
    [JsonPropertyName("includeQRCode")]
    public bool IncludeQRCode { get; set; }

    [JsonPropertyName("callback")]
    public Callback Callback { get; set; }

    [JsonPropertyName("authority")]
    public string Authority { get; set; }

    [JsonPropertyName("registration")]
    public Registration Registration { get; set; }

    [JsonPropertyName("issuance")]
    public Issuance Issuance { get; set; }

    public string Id { get; set; }
}

public class PresentationRequest
{
    [JsonPropertyName("includeQRCode")]
    public bool IncludeQRCode { get; set; }

    [JsonPropertyName("callback")]
    public Callback Callback { get; set; }

    [JsonPropertyName("authority")]
    public string Authority { get; set; }

    [JsonPropertyName("registration")]
    public Registration Registration { get; set; }

    [JsonPropertyName("presentation")]
    public Presentation Presentation { get; set; }

}

 public class RequestedCredential
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; }

    [JsonPropertyName("acceptedIssuers")]
    public List<string> AcceptedIssuers { get; set; }
}

public class Presentation
{
    [JsonPropertyName("includeReceipt")]
    public bool IncludeReceipt { get; set; }
    
    [JsonPropertyName("requestedCredentials")]
    public List<RequestedCredential> RequestedCredentials { get; set; }
}

public class IssuanceCallback
{
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("error")]
    public Error Error { get; set; }
}

public class Error
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class CacheObject
{
    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("payload")]
    public string Payload { get; set; }

    [JsonPropertyName("expiry")]
    public string Expiry {get;set;}

    [JsonPropertyName("subjet")]
    public string Subject {get;set;}

    [JsonPropertyName("name")]
    public string Name {get; set;}
}

