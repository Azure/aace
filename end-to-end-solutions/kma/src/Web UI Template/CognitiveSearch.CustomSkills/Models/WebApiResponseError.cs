using Newtonsoft.Json;

namespace CognitiveSkills.CustomSkills.Models
{
    public class WebApiResponseError
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}