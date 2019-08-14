using CognitiveSearch.Azure.Search;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CognitiveSearch.WebApi.Models
{
    public class FacetsFilteringRequest: FacetsBody
    {

    }

    public class FacetsFilteringResponse: FacetsBody
    {

        public class OutputRecordMessage
        {
            public string Message { get; set; }
        }
        public List<OutputRecordMessage> Errors { get; set; }
        public List<OutputRecordMessage> Warnings { get; set; }
    }

    public class FacetsBody
    {
        [JsonProperty("values")]
        public List<FacetRecord> Values { get; set; }
        [JsonProperty("searchFacets")]
        public SearchFacet[] SearchFacets { get; set; }
        [JsonProperty("currentPage")]
        public int CurrentPage { get; set; } = 1;
    }

    public class FacetRecord
    {
        [JsonProperty("recordId")]
        public string RecordId { get; set; }

        [JsonProperty("data")]
        public FacetData Data { get; set; }
    }

    public class FacetData
    {
        [JsonProperty("facetName")]
        public string FacetName { get; set; }
        [JsonProperty("facets")]
        public List<string> Facets { get; set; }
    }
}