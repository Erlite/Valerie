using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rick.JsonModels
{
    public class Datum
    {
        [JsonProperty("embed_url")]
        public string EmbedUrl { get; set; }
    }

    public class Giphy
    {
        [JsonProperty("data")]
        public List<Datum> Root { get; set; }
    }
}
