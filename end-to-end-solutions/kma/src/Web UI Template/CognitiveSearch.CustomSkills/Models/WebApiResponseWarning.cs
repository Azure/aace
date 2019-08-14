using Newtonsoft.Json;

namespace CognitiveSkills.CustomSkills.Models
{
    public class WebApiResponseWarning
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}