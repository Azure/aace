using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Luna.Clients.Azure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Luna.Clients.GitUtils
{
    public class GitUtility: IGitUtility
    {
        private static string GITHUB_PROJECT_ZIP_URL_FORMAT = @"https://api.github.com/repos/{0}{1}/zipball/{2}";
        private static string AZUREDEPOPS_PROJECT_ZIP_URL_FORMAT = @"https://dev.azure.com/{0}{1}_apis/git/repositories/{2}/items?api-version=5.0&download=true&versionDescriptor.version={3}&versionDescriptor.versionType=commit";

        private readonly ILogger<GitUtility> _logger;
        private readonly HttpClient _httpClient;
        private readonly IStorageUtility _storageUtillity;

        [ActivatorUtilitiesConstructor]
        public GitUtility(ILogger<GitUtility> logger,
                   IStorageUtility storageUtility,
                   HttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageUtillity = storageUtility ?? throw new ArgumentNullException(nameof(storageUtility));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string> DownloadProjectAsZipToAzureStorageAsync(string gitUrl, string version, string paToken, string containerName, string fileName)
        {
            Uri gitUri = new Uri(gitUrl);
            byte[] content = null;
            if (gitUri.Host.Equals("dev.azure.com", StringComparison.InvariantCultureIgnoreCase))
            {
                string projectZipUrl = string.Format(AZUREDEPOPS_PROJECT_ZIP_URL_FORMAT, gitUri.Segments[1], gitUri.Segments[2], gitUri.Segments[4], version);

                var request = new HttpRequestMessage { RequestUri = new Uri(projectZipUrl), Method = HttpMethod.Get };
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", paToken))));
                request.Headers.Add("Accept", "application/zip");

                var response = await _httpClient.SendAsync(request);

                content = await response.Content.ReadAsByteArrayAsync();
            }
            else if (gitUri.Host.Equals("github.com", StringComparison.InvariantCultureIgnoreCase))
            {
                string projectZipUrl = string.Format(GITHUB_PROJECT_ZIP_URL_FORMAT, gitUri.Segments[1], gitUri.Segments[2].Substring(0, gitUri.Segments[2].Length - 4), version);

                var request = new HttpRequestMessage { RequestUri = new Uri(projectZipUrl), Method = HttpMethod.Get };
                request.Headers.Authorization = new AuthenticationHeaderValue("token", paToken);
                request.Headers.Add("User-Agent", "Luna Service");

                var response = await _httpClient.SendAsync(request);

                content = await response.Content.ReadAsByteArrayAsync();
            }

            return await _storageUtillity.UploadBinaryFileAsync(containerName, fileName, content, true);
        }
    }
}
