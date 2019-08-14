using Microsoft.Azure.Search.Models;
using System.Collections.Generic;

namespace CognitiveSearch.Azure.Search
{
    public class DocumentResult
    {
        public List<object> Facets { get; set; }
        public Document Result { get; set; }
        public IList<SearchResult<Document>> Results { get; set; }
        public int? Count { get; set; }
        public string Token { get; set; }
        public List<object> Tags { get; set; }
        public string SearchId { get; set; }
    }
}