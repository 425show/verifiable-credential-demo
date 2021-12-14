using Humanizer;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace AspNetCoreVerifiableCredentials.Pages
{
    public class IssuerModel : PageModel
    {

        protected readonly AppSettingsModel _appSettings;
        protected readonly List<CredentialType> _credentialTypes;
        public IssuerModel( IOptions<AppSettingsModel> appSettings, CredentialTypeHelper credHelper)
        {
            _appSettings = appSettings.Value;
            _credentialTypes = credHelper.GetCredentialTypes();
        }
        
        public List<SelectListItem> CredentialTypes { get; set; }

        public void OnGet(string credType)
        {
            if(!string.IsNullOrEmpty(credType) && !_credentialTypes.Any( x => x.Name.Equals(credType, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new Exception("Invalid credential type specified");
            }
            
            credType = credType ?? string.Empty;
            CredentialTypes = new List<SelectListItem>();

            foreach(string cred in _credentialTypes.Select( x => x.Name))
            {
                CredentialTypes.Add(new SelectListItem
                {
                    Value = cred,
                    Text = cred.Humanize(LetterCasing.Title),
                    Selected = cred.ToLower() == credType.ToLower()
                });
            }
        }
    }
}
