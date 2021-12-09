using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Microsoft.Identity.Client;

using System.Text.Json.Serialization;
using System.Text.Json;

namespace AspNetCoreVerifiableCredentials
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class IssuerController : ControllerBase
    {
        protected readonly AppSettingsModel _appSettings;
        protected IMemoryCache _cache;
        protected readonly ILogger<IssuerController> _log;
        private HttpClient _httpClient;
        private readonly IConfidentialClientApplication _msal;

        public IssuerController(IOptions<AppSettingsModel> appSettings, MsalTokenProvider msal, IMemoryCache memoryCache, ILogger<IssuerController> log, IHttpClientFactory httpClientFactory)
        {
            _appSettings = appSettings.Value;
            _cache = memoryCache;
            _log = log;
            _httpClient = httpClientFactory.CreateClient();
            _msal = msal._client;
        }

        private string GetRandomPIN(int length)
        {
            var pinMaxValue = (int)Math.Pow(10, length) - 1;
            var randomNumber = RandomNumberGenerator.GetInt32(1, pinMaxValue);
            return string.Format("{0:D" + length.ToString() + "}", randomNumber);
        }
        private VcRequest GenerateVcRequest(string reqId, string credType, string firstName, string lastName)
        {
            var request = new VcRequest()
            {
                Id = reqId,
                Callback = new Callback()
                {
                    Url = _appSettings.IssuerCallbackUrl,
                    State = reqId,
                },
                Authority = _appSettings.IssuerAuthority,
                IncludeQRCode = false,
                Registration = new Registration()
                {
                    ClientName = _appSettings.ClientName
                },
                Issuance = new Issuance()
                {
                    Type = credType,
                    Manifest = $"{_appSettings.CredentialManifest}{credType}",
                    Pin = IsMobile()? null : new Pin()
                    {
                        Value = GetRandomPIN(4),
                        Length = 4
                    },
                    Claims = new Claims()
                    {
                        FamilyName = lastName,
                        GivenName = firstName
                    }
                }
            };
            return request;
        }
        
        [HttpGet("/api/issuer/issuance-request")]
        public async Task<ActionResult> IssuanceRequest(string credType, string firstName, string lastName)
        {
            try
            {
                string jsonResponse = string.Empty;
                var requestId = Guid.NewGuid().ToString();

                _log.LogInformation($"Issuer Request for credtype {credType}");

                //"VC:Endpoint": "https://beta.did.msidentity.com/v1.0/{0}/verifiablecredentials/request",
                _log.LogInformation("Starting new issue request");
                var t = await _msal.AcquireTokenForClient(new[] { _appSettings.VCServiceScope }).ExecuteAsync();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t.AccessToken);

                _log.LogInformation($"Generating request ID {requestId}");
                var request = GenerateVcRequest(requestId, credType, firstName, lastName);

                var reqPayload = System.Text.Json.JsonSerializer.Serialize(request, new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                //CALL REST API WITH PAYLOAD
                HttpStatusCode statusCode = HttpStatusCode.OK;

                try
                {
                     _log.LogInformation($"Sending {reqPayload} to VC service {_appSettings.Endpoint}"); 

                    //var serviceRequest = await _httpClient.PostAsJsonAsync<VcRequest>("verifiablecredentials/request", request);
                    var a = new StringContent(reqPayload, System.Text.Encoding.UTF8, "application/json");

                    var serviceRequest = await _httpClient.PostAsync(_appSettings.ApiEndpoint, a);

                    //var response = await serviceRequest.Content.ReadAsStringAsync();
                    var response = await serviceRequest.Content.ReadAsStringAsync();
                    var res = System.Text.Json.JsonSerializer.Deserialize<VcResponse>(response);
                    
                    statusCode = serviceRequest.StatusCode;

                    if (statusCode == HttpStatusCode.Created)
                    {
                        _log.LogTrace("succesfully called Request API");

                        //We use in memory cache to keep state about the request. The UI will check the state when calling the presentationResponse method

                        var cacheData = new CacheObject
                        {
                            Status = "notscanned",
                            Message = "Request ready, please scan with Authenticator",
                            Expiry = res.Expiry.ToString()
                        };
                        _cache.Set(requestId, JsonSerializer.Serialize(cacheData));

                        res.Pin = IsMobile()? null : request.Issuance.Pin.Value;
                        return new OkObjectResult(res);
                    }
                    else
                    {
                        _log.LogError("Unsuccesfully called Request API");
                        return BadRequest(new { error = "400", error_description = "Something went wrong calling the API: " + response });
                    }

                }
                catch (Exception ex)
                {
                    return BadRequest(new { error = "400", error_description = "Something went wrong calling the API: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> IssuanceCallback()
        {
            try
            {
                string content = await new StreamReader(this.Request.Body).ReadToEndAsync();

                var issuanceResponse = string.IsNullOrEmpty(content)
                    ? null
                    : JsonSerializer.Deserialize<IssuanceCallback>(content);

                if(issuanceResponse == null)
                {
                    _log.LogError("No issuance response received in body");
                }

                _log.LogTrace($"callback!: {issuanceResponse.RequestId}");
                var requestId = issuanceResponse.RequestId;

                if (issuanceResponse.Code.Equals("request_retrieved", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cacheData = new CacheObject
                    {
                        Status = "request_retrieved",
                        Message = "QR Code is scanned. Waiting for issuance...",
                    };
                    _cache.Set(requestId, JsonSerializer.Serialize(cacheData));
                }

                if (issuanceResponse.Code.Equals("issuance_successful", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cacheData = new CacheObject
                    {
                        Status = "issuance_successful",
                        Message = "Credential issued successfully",
                    };
                    
                    _cache.Set(requestId, JsonSerializer.Serialize(cacheData));
                }
                //
                //We capture if something goes wrong during issuance. See documentation with the different error codes
                //
                if (issuanceResponse.Code.Equals("issuance_error", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cacheData = new CacheObject
                    {
                        Status = "issuance_error",
                        Payload = issuanceResponse.Error.Code.ToString(),
                        Message = issuanceResponse.Error.Message

                    };
                    _cache.Set(requestId, JsonSerializer.Serialize(cacheData));
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        [HttpGet("/api/issuer/issuance-response")]
        public ActionResult IssuanceResponse(string id)
        {
            try
            {
                var requestId = id;
                if (string.IsNullOrEmpty(requestId))
                {
                    return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
                }
                if (_cache.TryGetValue(requestId, out string buf))
                {
                    var cacheObject = JsonSerializer.Deserialize<CacheObject>(buf);

                    Debug.WriteLine("check if there was a response yet: " + cacheObject.Status);
                    return new ContentResult { ContentType = "application/json", Content = JsonSerializer.Serialize(cacheObject) };
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "400", error_description = ex.Message });
            }
        }

        //some helper functions
        /*
        protected string GetRequestHostName()
        {
            string scheme = "https";// : this.Request.Scheme;
            string originalHost = this.Request.Headers["x-original-host"];
            string hostname = "";
            if (!string.IsNullOrEmpty(originalHost))
                hostname = string.Format("{0}://{1}", scheme, originalHost);
            else hostname = string.Format("{0}://{1}", scheme, this.Request.Host);
            return hostname;
        }
        */

        protected bool IsMobile()
        {
            string userAgent = this.Request.Headers["User-Agent"];

            if (userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return true;
            else
                return false;
        }
    }
}
