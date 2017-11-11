using Newtonsoft.Json;
using System;
namespace Valerie.JsonModels
{
    public class PornModel
    {
        [JsonProperty("result")]
        public Result[] Result { get; set; }
    }

    public class Result
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