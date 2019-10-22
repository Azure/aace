using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.Azure.Search
{
    public class SearchClient
    {
        private readonly SearchConfig _searchConfig;
        private readonly SearchServiceClient _searchClient;

        // Client logs all searches in Application Insights
        private readonly TelemetryClient _telemetryClient;
        public static string _searchId;

        public SearchClient(SearchConfig searchConfig, TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
            _searchConfig = searchConfig;
            _searchClient = new SearchServiceClient(searchConfig.ServiceName, new SearchCredentials(searchConfig.Key));

            Schema = new SearchSchema().AddFields(_searchClient.Indexes.Get(_searchConfig.IndexName).Fields);
            Model = new SearchModel(Schema);
        }

        public SearchSchema Schema { get; set; }
        public SearchModel Model { get; set; }
        
        /// <summary>
        /// Initiates a run of the search indexer.
        /// </summary>
        public async Task RunIndexer()
        {
            var indexer = await GetDefaultIndexer();
            var indexStatus = await _searchClient.Indexers.GetStatusAsync(indexer.Name);
            if (indexStatus.LastResult.Status != IndexerExecutionStatus.InProgress)
            {
                _searchClient.Indexers.Run(indexer.Name);
            }
        }

        private async Task<Indexer> GetDefaultIndexer()
        {
            var indexers = await _searchClient.Indexers.ListAsync();
            if (indexers == null || !indexers.Indexers.Any())
            {
                Console.WriteLine($"No indexers found in the Azure Search Service named {_searchConfig.ServiceName}");
                return null;
            }

            // For simplicity in the starter, assume only one Indexer is defined, so grab the first one.
            // For production environments, this should be updated to receive the name of the target indexer to run.
            return indexers.Indexers.First();
        }

        public async Task<IndexerExecutionInfo> GetIndexerStatus()
        {
            var indexer = await GetDefaultIndexer();
            var indexStatus = await _searchClient.Indexers.GetStatusAsync(indexer.Name);
            return indexStatus;
        }


        public async Task ResetIndexer()
        {
            var indexer = await GetDefaultIndexer();
            await _searchClient.Indexers.ResetAsync(indexer.Name);
            return;
        }

        public async Task<DocumentSearchResult<Document>> Search(string searchText, SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
            try
            {
                using (var indexClient = new SearchIndexClient(_searchConfig.ServiceName, _searchConfig.IndexName, new SearchCredentials(_searchConfig.Key)))
                {
                    var sp = await GenerateSearchParameters(searchFacets, selectFilter, currentPage);
                    sp.HighlightFields = new List<string> { "content" };
                    if (!string.IsNullOrEmpty(_telemetryClient.InstrumentationKey))
                    {
                        var s = GenerateSearchId(indexClient, searchText, sp);
                        _searchId = s.Result;
                    }
                    return indexClient.Documents.Search(searchText, sp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }

        private async Task<string> GenerateSearchId(SearchIndexClient indexClient, string searchText, SearchParameters sp)
        {

            var headers = new Dictionary<string, List<string>>() { { "x-ms-azs-return-searchid", new List<string>() { "true" } } };
            var response = await indexClient.Documents.SearchWithHttpMessagesAsync(searchText: searchText, searchParameters: sp, customHeaders: headers);
            string searchId = string.Empty;
            if (response.Response.Headers.TryGetValues("x-ms-azs-searchid", out IEnumerable<string> headerValues))
            {
                searchId = headerValues.FirstOrDefault();
            }
            return searchId;
        }

        public async Task<string> GetSearchId() => string.IsNullOrWhiteSpace(_searchId)
                ? string.Empty
                : _searchId;

        public async Task<Document> Lookup(string id)
        {
            using (var indexClient = await GetSearchIndexClient(_searchConfig.IndexName))
            {
                try
                {
                    return await indexClient.Documents.GetAsync(id);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error querying index: {ex.Message}");
                }
                return null;
            }
        }

        public async Task<string> ExecuteSearchQuery(string q)
        {
            var searchId = _searchClient.Indexes.Client;
            return "";
        }

        public async Task<SearchParameters> GenerateSearchParameters(SearchFacet[] searchFacets = null, string[] selectFilter = null, int currentPage = 1)
        {
            // For more information on search parameters visit: 
            // https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.search.models.searchparameters?view=azure-dotnet
            SearchParameters sp = new SearchParameters()
            {
                SearchMode = SearchMode.All,
                Top = 10,
                Skip = (currentPage - 1) * 10,
                IncludeTotalResultCount = true,
                QueryType = QueryType.Full,
                Select = selectFilter,
                Facets = Model.Facets.Select(f => f.Name).ToList()
            };

            string filter = null;
            var filterStr = string.Empty;

            if (searchFacets != null)
            {
                foreach (var item in searchFacets)
                {
                    var facet = Model.Facets.Where(f => f.Name == item.Key).FirstOrDefault();

                    filterStr = string.Join(",", item.Value);

                    // Construct Collection(string) facet query
                    if (facet.Type == typeof(string[]))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                        else
                            filter += $" and {item.Key}/any(t: search.in(t, '{filterStr}', ','))";
                    }
                    // Construct string facet query
                    else if (facet.Type == typeof(string))
                    {
                        if (string.IsNullOrEmpty(filter))
                            filter = $"{item.Key} eq '{filterStr}'";
                        else
                            filter += $" and {item.Key} eq '{filterStr}'";
                    }
                    // Construct DateTime facet query
                    else if (facet.Type == typeof(DateTime))
                    {
                        // TODO: Date filters
                    }
                }
            }

            sp.Filter = filter;
            return sp;
        }


        private async Task<SearchIndexClient> GetSearchIndexClient(string indexName)
        {
            return await Task.FromResult(new SearchIndexClient(_searchConfig.ServiceName, indexName, new SearchCredentials(_searchConfig.Key)));
        }

        /// <summary>
        /// Retrieves suggested documents containing the search text.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="fuzzy"></param>
        /// <returns></returns>
        public async Task<DocumentSuggestResult<Document>> Suggest(string searchText, bool fuzzy)
        {
            // Execute search based on query string
            try
            {
                using (var indexClient = await GetSearchIndexClient(_searchConfig.IndexName))
                {
                    var sp = new SuggestParameters
                    {
                        UseFuzzyMatching = fuzzy,
                        Top = 8
                    };

                    return await indexClient.Documents.SuggestAsync(searchText, "sg", sp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error querying index: {0}\r\n", ex.Message.ToString());
            }
            return null;
        }
    }
}