using Newtonsoft.Json;

namespace CognitiveSearch.Azure.Search
{
    public class FDGraphEdge
    {
        [JsonProperty("source")]
        public int Source { get; set; }
        [JsonProperty("target")]
        public int Target { get; set; }
    }
}