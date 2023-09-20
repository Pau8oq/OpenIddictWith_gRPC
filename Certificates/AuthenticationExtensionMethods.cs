using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Certificates
{
    public static class AuthenticationExtensionMethods
    {
        public static X509Certificate2 TokenEncryptionCertificateForDocker()
        {
            return X509Certificate2.CreateFromPemFile("/https/encripting.crt", "/https/encripting.key");
        }

        public static X509Certificate2 TokenSigningCertificateForDocker()
        {
            return X509Certificate2.CreateFromPemFile("/https/signing.crt", "/https/signing.key");
        }

        public static X509Certificate2 TokenEncryptionCertificate()
        {
            return X509Certificate2.CreateFromPemFile(GetFullPath("certs/encripting.crt"), GetFullPath("certs/encripting.key"));
        }

        public static X509Certificate2 TokenSigningCertificate()
        {
            return X509Certificate2.CreateFromPemFile(GetFullPath("certs/signing.crt"), GetFullPath("certs/signing.key"));
        }

        private static string GetFullPath(string path)
        {
            string workingLocation = (typeof(AuthenticationExtensionMethods)).Assembly.Location;
            string workingDir = Path.GetDirectoryName(workingLocation);

            var fullPath = Path.Combine(workingDir, path);
            return fullPath;
        }
    }
}
