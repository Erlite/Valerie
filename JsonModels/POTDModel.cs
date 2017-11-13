using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public class POTDModel
    {
        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("explanation")]
        public string Explanation { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("hdurl")]
        public string Hdurl { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("service_version")]
        public string ServiceVersion { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
