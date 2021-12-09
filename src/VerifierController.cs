using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Diagnostics;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using System.Text.Json;
using AspNetCoreVerifiableCredentials.PresentationModel;

namespace AspNetCoreVerifiableCredentials
{
    [Route("api/[controller]/[action]")]
    public class VerifierController : Controller
    {
        protected readonly AppSettingsModel _appSettings;
        protected IMemoryCache _cache;
        protected readonly ILogger<VerifierController> _log;
        private HttpClient _httpClient;
        private IConfidentialClientApplication _msal;

        public VerifierController(
            IOptions<AppSettingsModel> appSettings, 
            IMemoryCache memoryCache, 
            ILogger<VerifierController> log, 
            IHttpClientFactory httpClientFactory,
            MsalTokenProvider msal)
        {
            _msal = msal._client;
            _appSettings = appSettings.Value;
            _cache = memoryCache;
            _log = log;
            _httpClient = httpClientFactory.CreateClient();
        }

        private PresentationRequest CreatePresentationRequest(string credType, string requestId)
        {
            return new PresentationRequest
            {
                IncludeQRCode = false,
                Callback = new Callback
                {
                    State = requestId,
                    Url = _appSettings.PresentationCallbackUrl
                },
                Authority = _appSettings.VerifierAuthority,
                Registration = new Registration()
                {
                    ClientName = _appSettings.ClientName
                },
                Presentation = new Presentation
                {
                    IncludeReceipt = true,
                    RequestedCredentials = new List<RequestedCredential>()
                    {
                        new RequestedCredential
                        {
                            Type = credType,
                            Purpose = "Verifying issued credential",
                            AcceptedIssuers = new List<string>() { _appSettings.IssuerAuthority },
                        }
                    }
                }
            };
        }

        [HttpGet("/api/verifier/presentation-request")]
        public async Task<ActionResult> PresentationRequest(string credType)
        {
            try
            {
                 _log.LogInformation($"Starting new verification request for credential type {credType}");
                var t = await _msal.AcquireTokenForClient(new[] { _appSettings.VCServiceScope }).ExecuteAsync();
                if (t.AccessToken == String.Empty)
                {
                    _log.LogError("Failed to acquire accesstoken");
                    return BadRequest(new { error = "no access token", error_description = "Authentication Failed" });
                }
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", t.AccessToken);
                
                string requestId = Guid.NewGuid().ToString();
                var jsonString = JsonSerializer.Serialize(CreatePresentationRequest(credType, requestId));
                try
                {
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var serviceRequest = await _httpClient.PostAsync(_appSettings.ApiEndpoint, content);
                    var response = await serviceRequest.Content.ReadAsStringAsync();
                    var res = System.Text.Json.JsonSerializer.Deserialize<VcResponse>(response);

                    _log.LogTrace("succesfully called Request API");
                    var statusCode = serviceRequest.StatusCode;

                    if (statusCode == HttpStatusCode.Created)
                    {
                        var cacheData = new CacheObject
                        {
                            Status = "notscanned",
                            Message = "Request ready, please scan with Authenticator",
                            Expiry = res.Expiry.ToString()
                        };
                        _cache.Set(requestId, JsonSerializer.Serialize(cacheData));

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
        public async Task<ActionResult> PresentationCallback()
        {
            try
            {
                string content = await new StreamReader(this.Request.Body).ReadToEndAsync();

                var presentation = string.IsNullOrEmpty(content)
                    ? null
                    : JsonSerializer.Deserialize<PresentationCallback>(content);

                if(presentation == null)
                {
                    _log.LogError("No presentation response received in body");
                }

                Debug.WriteLine("callback!: " + presentation.RequestId);
                var requestId = presentation.RequestId;

                if (presentation.Code.Equals("request_retrieved", StringComparison.InvariantCultureIgnoreCase))
                {
                    var cacheData = new CacheObject
                    {
                        Status = "request_retrieved",
                        Message = "QR Code is scanned. Waiting for validation...",
                    };
                    _cache.Set(requestId, JsonSerializer.Serialize(cacheData));
                }

                if (presentation.Code.Equals("presentation_verified", StringComparison.CurrentCultureIgnoreCase))
                {
                    var cacheData = new CacheObject
                    {
                        Status = "presentation_verified",
                        Message = "Presentation verified",
                        Payload = string.Join(",", presentation.Issuers.First().Type),
                        Subject = presentation.Subject,
                        Name =  $"{presentation.Issuers.First().Claims.FirstName} {presentation.Issuers.First().Claims.LastName}"
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

        [HttpGet("/api/verifier/presentation-response")]
        public ActionResult PresentationResponse(string id)
        {
            try
            {
                //string state = this.Request.Query["id"];
                var state = id;
                if (string.IsNullOrEmpty(state))
                {
                    return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
                }
                if (_cache.TryGetValue(state, out string buf))
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
    }
}
