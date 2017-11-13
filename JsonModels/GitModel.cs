using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public class GitModel
    {
        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }
    }
}