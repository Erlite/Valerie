using Newtonsoft.Json;
using System.Collections.Generic;
using Valerie.Models.Enums;

namespace Valerie.Models
{
    public class SearchValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayurl")]
        public string URL { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }
    }

    public class SearchWebPages
    {
        [JsonProperty("totalEstimatedMatches")]
        public int TotalMatches { get; set; }

        [JsonProperty("value")]
        public List<SearchValue> Value { get; set; }
    }

    public class SearchRoot
    {
        [JsonProperty("webPages")]
        public SearchWebPages Pages { get; set; }
    }

    public class DocsMembers
    {
        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("itemType")]
        public DocsType Type { get; set; }

        [JsonProperty("itemKind")]
        public DocsKind Kind { get; set; }

        [JsonProperty("description")]
        public string Snippet { get; set; }
    }

    public class DocsRoot
    {
        [JsonProperty("results")]
        public List<DocsMembers> Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
