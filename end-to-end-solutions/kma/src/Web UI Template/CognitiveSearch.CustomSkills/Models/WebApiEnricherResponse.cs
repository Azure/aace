using Newtonsoft.Json;
using System.Collections.Generic;

namespace CognitiveSkills.CustomSkills.Models
{
    public class WebApiEnricherResponse
    {
        [JsonProperty("values")]
        public List<WebApiResponseRecord> Values { get; set; }
    }
}