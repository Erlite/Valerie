using Newtonsoft.Json;

namespace Valerie.Models
{
    public partial class DotnetModel
    {
        [JsonProperty("results")]
        public DotnetModel[] Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public partial class DotnetModel
    {
        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("itemType")]
        public string Type { get; set; }

        [JsonProperty("itemKind")]
        public string Kind { get; set; }

        [JsonProperty("description")]
        public string Snippet { get; set; }
    }
}