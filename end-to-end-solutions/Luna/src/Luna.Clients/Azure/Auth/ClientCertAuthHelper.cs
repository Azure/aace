using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Luna.Clients.Azure.Auth
{
    public class ClientCertAuthHelper
    {
        public static bool IsValidClientCertificate(X509Certificate2 certificate)
        {
            //1. Check time validity of certificate
            if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 || DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
            {
                return false;
            }

            //2. Check subject name of certificate
            bool foundSubject = false;
            string[] certSubjectData = certificate.Subject.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in certSubjectData)
            {
                if (String.Compare(s.Trim(), "CN=Test2") == 0)
                {
                    foundSubject = true;
                    break;
                }
            }
            if (!foundSubject) return false;

            //3. Check issuer name of certificate
            bool foundIssuerCN = false;
            string[] certIssuerData = certificate.Issuer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in certIssuerData)
            {
                if (String.Compare(s.Trim(), "CN=Root Agency") == 0)
                {
                    foundIssuerCN = true;
                    break;
                }
            }
            if (!foundIssuerCN) ;// return false;

            // 4. Check thumbprint of certificate
            if (String.Compare(certificate.Thumbprint.Trim().ToUpper(), "D7E5A7AD603CFD6AEBEA8B546AA8AF9C11A6A7A5") != 0) return false;

            return true;
        }
    }
}
