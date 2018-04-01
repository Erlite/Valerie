using Newtonsoft.Json;

namespace Valerie.Models
{
    public partial class GithubModel
    {
        [JsonProperty("commit")]
        public GithubModel Commit { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }
    }

    public partial class GithubModel
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}