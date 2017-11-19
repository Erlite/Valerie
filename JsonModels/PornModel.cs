using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public partial class PornModel
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("result")]
        public PornModel[] Result { get; set; }
    }

    public partial class PornModel
    {
        [JsonProperty("thumb")]
        public string Thumb { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }
    }
}