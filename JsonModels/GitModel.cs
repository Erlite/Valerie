using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public partial class GitModel
    {
        [JsonProperty("commit")]
        public GitModel Commit { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }
    }

    public partial class GitModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}