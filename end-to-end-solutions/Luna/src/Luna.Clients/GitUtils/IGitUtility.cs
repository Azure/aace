using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Clients.GitUtils
{
    public interface IGitUtility
    {
        Task<string> DownloadProjectAsZipToAzureStorageAsync(string gitUrl, string version, string paToken, string containerName, string fileName);
    }
}
