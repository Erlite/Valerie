using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.Models
{
    public partial class UrbanModel
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }
        [JsonProperty("word")]
        public string Word { get; set; }
        [JsonProperty("example")]
        public string Example { get; set; }
    }

    public partial class UrbanModel
    {
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        [JsonProperty("list")]
        public List<UrbanModel> List { get; set; }
    }
}