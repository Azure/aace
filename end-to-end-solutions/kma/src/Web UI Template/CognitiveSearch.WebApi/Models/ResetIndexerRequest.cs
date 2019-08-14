using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.WebApi.Models
{
    public class ResetIndexerRequest
    {
        [JsonProperty("delete")]
        public bool Delete { get; set; }

        [JsonProperty("run")]
        public bool Run { get; set; }

        [JsonProperty("reset")]
        public bool Reset { get; set; }
    }
}
