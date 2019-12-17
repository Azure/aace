using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using CognitiveSearch.Azure.Storage.Blobs;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FacetsController : ControllerBase
    {
        private readonly AppInsightsConfig _appInsightsConfig;
        private readonly SearchConfig _searchConfig;
        private readonly SearchClient _searchClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly BlobStorageConfig _storageConfig;

        public FacetsController(AppInsightsConfig appInsightsConfig, SearchConfig searchConfig, TelemetryClient telemetryClient, BlobStorageConfig storageConfig)
        {
            _appInsightsConfig = appInsightsConfig;
            _searchConfig = searchConfig;
            _telemetryClient = telemetryClient;
            _telemetryClient.InstrumentationKey = _appInsightsConfig.InstrumentationKey;
            _storageConfig = storageConfig;
            _searchClient = new SearchClient(_searchConfig, _telemetryClient);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            BlobStorageConfig config = _storageConfig.Copy();
            config.ContainerName = config.FacetsFilteringContainerName;
            List<object> result = new List<object>();
            foreach (string name in _searchClient.Model.Facets.Select(f => f.Name))
            {
                var fileName = string.Format("{0}.txt", name.Replace(" ", "").ToLower());
                if (await BlobStorageClient.BlobExistsAsync(config, fileName))
                {
                    string text = await BlobStorageClient.ReadBlobAsync(config, fileName);
                    result.Add(new { name = name, restrictionList = text });
                }
                else
                {
                    await BlobStorageClient.UploadBlobAsync(config, fileName, "");

                    result.Add(new { name = name, restrictionList = "" });
                }
            }
            return await Task.FromResult(new JsonResult(result));
        }

        [HttpPost]
        public async Task<IActionResult> Update(FacetFilterUpdateRequest request)
        {
            BlobStorageConfig config = _storageConfig.Copy();
            config.ContainerName = config.FacetsFilteringContainerName;
            await BlobStorageClient.WriteBlobAsync(config, string.Format("{0}.txt", request.FacetName.Replace(" ", "").ToLower()), request.Text);
            return await Task.FromResult(new JsonResult("success"));
        }

        public class FacetFilterUpdateRequest
        {
            [JsonProperty("facetName")]
            public string FacetName { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}