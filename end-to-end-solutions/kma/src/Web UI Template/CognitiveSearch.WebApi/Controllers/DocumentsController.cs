using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveSearch.Azure.AppInsights;
using CognitiveSearch.Azure.Search;
using CognitiveSearch.Azure.Storage.Blobs;
using CognitiveSearch.WebApi.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CognitiveSearch.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly AppInsightsConfig _appInsightsConfig;
        private readonly SearchConfig _searchConfig;
        private readonly BlobStorageConfig _storageConfig;
        private readonly SearchClient _searchClient;
        private readonly TelemetryClient _telemetryClient;

        public DocumentsController(AppInsightsConfig appInsightsConfig, SearchConfig searchConfig, BlobStorageConfig storageConfig, TelemetryClient telemetryClient)
        {
            _appInsightsConfig = appInsightsConfig;
            _searchConfig = searchConfig;
            _storageConfig = storageConfig;
            _telemetryClient = telemetryClient;
            _telemetryClient.InstrumentationKey = _appInsightsConfig.InstrumentationKey;
            _searchClient = new SearchClient(_searchConfig, _telemetryClient);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                _telemetryClient.TrackEvent($"A document request with an empty Id was sent. Document Id is required.");
                return await Task.FromResult(new NotFoundObjectResult($"A Document Id is required."));
            }

            var token = await BlobStorageClient.GetContainerSasUriAsync(_storageConfig);
            var response = await _searchClient.Lookup(id);

            var telemetryDict = new Dictionary<string, string>
            {
                { "SearchServiceName", _searchConfig.ServiceName },
                { "ClickedDocId", id }
            };

            _telemetryClient.TrackEvent("Click", telemetryDict);

            return await Task.FromResult(new JsonResult(new DocumentResult { Result = response, Token = token }));
        }

        [HttpPost]
        public async Task<IActionResult> Search(SearchRequest searchRequest)
        {
            var token = await BlobStorageClient.GetContainerSasUriAsync(_storageConfig);
            var selectFilter = _searchClient.Model.SelectFilter;
            var search = searchRequest.Query;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Replace("-", "").Replace("?", "");
            }

            var response = await _searchClient.Search(search, searchRequest.SearchFacets, selectFilter, searchRequest.CurrentPage);
            var searchId = await _searchClient.GetSearchId();
            var facetResults = new List<object>();
            var tagsResults = new List<object>();

            if (response.Facets != null)
            {
                // Return only the selected facets from the Search Model
                foreach (var facetResult in response.Facets.Where(f => _searchClient.Model.Facets.Where(x => x.Name == f.Key).Any()))
                {
                    facetResults.Add(new
                    {
                        key = facetResult.Key,
                        value = facetResult.Value
                    });
                }

                foreach (var tagResult in response.Facets.Where(t => _searchClient.Model.Tags.Where(x => x.Name == t.Key).Any()))
                {
                    tagsResults.Add(new
                    {
                        key = tagResult.Key,
                        value = tagResult.Value
                    });
                }
            }
            var telemetryDict = new Dictionary<string, string>
            {
                { "SearchServiceName", _searchConfig.ServiceName },
                { "SearchId", searchId },
                { "IndexName", _searchConfig.IndexName },
                { "QueryTerms", search },
                { "ResultCount", response.Count.ToString() }
            };

            _telemetryClient.TrackEvent("Search", telemetryDict);

            return new JsonResult(new DocumentResult { Results = response.Results, Facets = facetResults, Tags = tagsResults, Count = Convert.ToInt32(response.Count), Token = token, SearchId = searchId });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            foreach (var formFile in Request.Form.Files)
            {
                if (formFile.Length > 0)
                {
                    await BlobStorageClient.UploadBlobAsync(_storageConfig, formFile.FileName, formFile.OpenReadStream());

                    var telemetryDict = new Dictionary<string, string>
                    {
                        { "FileName", formFile.FileName },
                        { "StorageAccountName", _searchConfig.ServiceName },
                        { "ContainerName", _storageConfig.ContainerName }
                    };

                    _telemetryClient.TrackEvent("FileUpload", telemetryDict);
                }
            }
            await _searchClient.RunIndexer();
            return new JsonResult("ok");
        }

        [HttpGet("indexerStatus")]
        public async Task<IActionResult> GetIndexerStatus()
        {
            var result = new JsonResult(await _searchClient.GetIndexerStatus());
            return result;
        }
    }
}