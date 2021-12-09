using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace AspNetCoreVerifiableCredentials.Pages
{
    public class IssuerModel : PageModel
    {

        protected readonly AppSettingsModel _appSettings;
        public IssuerModel( IOptions<AppSettingsModel> appSettings)
        {
            _appSettings = appSettings.Value;
        }
        
        public List<SelectListItem> CredentialTypes { get; set; }

        public void OnGet(string credType)
        {
            if(!string.IsNullOrEmpty(credType) && !_appSettings.CredentialTypesList.Contains(credType, StringComparer.InvariantCultureIgnoreCase))
            {
                throw new Exception("Invalid credential type specified");
            }
            
            credType = credType ?? string.Empty;
            CredentialTypes = new List<SelectListItem>();

            foreach(string cred in _appSettings.CredentialTypesList)
            {
                CredentialTypes.Add(new SelectListItem
                {
                    Value = cred,
                    Text = cred,
                    Selected = cred.ToLower() == credType.ToLower()
                });
            }
        }
    }
}
