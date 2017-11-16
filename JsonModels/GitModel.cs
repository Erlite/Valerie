using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public class GitModel
    {
        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }
    }

    public class Commit
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}