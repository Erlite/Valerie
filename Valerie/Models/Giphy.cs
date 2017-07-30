using Newtonsoft.Json;
using System.Collections.Generic;

namespace Valerie.Models
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
