using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.JsonModels
{
    public partial class RedditModel
    {
        [JsonProperty("data")]
        public RedditArray Data { get; set; }
    }

    public partial class RedditArray
    {
        [JsonProperty("children")]
        public IReadOnlyList<RedditChilds> Children { get; set; }
    }

    public partial class RedditChilds
    {
        [JsonProperty("data")]
        public ChildData ChildData { get; set; }
    }

    public partial class ChildData
    {
        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("selftext")]
        public string Selftext { get; set; }

        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }    
}