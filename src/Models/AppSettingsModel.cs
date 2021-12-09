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
                return String.Format(CultureInfo.InvariantCulture, Instance, TenantId);
            }
        }
        public string ApiEndpoint
        {
            get
            {
                return String.Format(CultureInfo.InvariantCulture, Endpoint, TenantId);
            }
        }

        public string ClientSecret { get; set; }

        public string CertificateName { get; set; }

        public bool AppUsesClientSecret(AppSettingsModel config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if (!String.IsNullOrWhiteSpace(config.CertificateName) && config.CertificateName != certificatePlaceholderValue)
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }
        public X509Certificate2 ReadCertificate(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
            {
                throw new ArgumentException("certificateName should not be empty. Please set the CertificateName setting in the appsettings.json", "certificateName");
            }
            CertificateDescription certificateDescription = CertificateDescription.FromStoreWithDistinguishedName(certificateName);
            DefaultCertificateLoader defaultCertificateLoader = new DefaultCertificateLoader();
            defaultCertificateLoader.LoadIfNeeded(certificateDescription);
            return certificateDescription.Certificate;
        }
    }

}



