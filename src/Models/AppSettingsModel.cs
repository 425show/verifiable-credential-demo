// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Identity.Web;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace AspNetCoreVerifiableCredentials
{
    public class AppSettingsModel
    {
        public string Instance { get; set; }

        public string Endpoint { get; set; }

        public string VCServiceScope { get; set; }

        public string CredentialManifest { get; set; }

        public string IssuerAuthority { get; set; }

        public string VerifierAuthority { get; set; }

        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string IssuerCallbackUrl { get; set; }

        public string PresentationCallbackUrl { get; set; }

        public string CredentialTypes { get; set; }

        public bool UseKeyVaultForSecrets {get ;set; }

        public string KeyVaultName {get;set;}

        public string[] CredentialTypesList
        {
            get
            {
                return CredentialTypes.Split(',');
            }
        }

        public string Authority
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, Instance, TenantId);
            }
        }
        public string ApiEndpoint
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, Endpoint, TenantId);
            }
        }

        public string ClientSecret { get; set; }

        public string CertificateName { get; set; }

        public bool AppUsesClientSecret()
        {
            if (!string.IsNullOrWhiteSpace(this.ClientSecret) || this.UseKeyVaultForSecrets)
            {
                return true;
            }
            else if (!string.IsNullOrWhiteSpace(this.CertificateName))
            {
                return false;
            }
            else
            {
                throw new Exception("You must choose between using client secret or certificate. Please update the 'appsettings.json' file.");
            }
        }
        public X509Certificate2 ReadCertificate(string certificateName)
        {
            ArgumentNullException.ThrowIfNull(certificateName, nameof(certificateName));

            var certificateDescription = CertificateDescription.FromStoreWithDistinguishedName(certificateName);
            var defaultCertificateLoader = new DefaultCertificateLoader();
            defaultCertificateLoader.LoadIfNeeded(certificateDescription);
            return certificateDescription.Certificate;
        }
    }

}



