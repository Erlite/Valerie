using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.Models
{
    public class Article
    {
        [JsonProperty("author")]
        public string Author { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("urlToImage")]
        public string ImageUrl { get; set; }
        [JsonProperty("publishedAt")]
        public string PublishDate { get; set; }
    }

    public class BBC
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("sortBy")]
        public string SortBy { get; set; }
        [JsonProperty("articles")]
        public List<Article> Articles { get; set; }
    }
}
