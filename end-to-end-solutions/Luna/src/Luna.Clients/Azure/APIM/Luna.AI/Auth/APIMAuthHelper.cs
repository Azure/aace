using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Luna.Clients.Azure.APIM
{
    public class APIMAuthHelper
    {
        public static string CreateSharedAccessToken(string primaryKey, string secondaryKey)
        {
            var id = "integration";
            var key = string.Format("{0}/{1}", primaryKey, secondaryKey);
            var expiry = DateTime.UtcNow.AddDays(10);
            using (var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                var dataToSign = id + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var signature = Convert.ToBase64String(hash);
                var encodedToken = string.Format("SharedAccessSignature {0}&{1:o}&{2}", id, expiry, signature);
                return encodedToken;
            }
        }
    }
}
