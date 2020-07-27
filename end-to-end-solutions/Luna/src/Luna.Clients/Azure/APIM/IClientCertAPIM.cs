using System;
using Luna.Clients.Azure.Auth;

namespace Luna.Clients.Azure.APIM
{
    public interface IClientCertAPIM
    {
        string GetAPIMRESTAPIPath(string owner);
        Task<ClientCertConfiguration> GetCert(string owner);
    }
}
