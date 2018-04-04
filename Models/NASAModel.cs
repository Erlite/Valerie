using Newtonsoft.Json;

namespace Valerie.Models
{
    public class NASAModel
    {
        [JsonProperty("explanation")]
        public string Explanation { get; set; }
        [JsonProperty("date")]
        public string Date { get; set; }
        [JsonProperty("hdurl")]
        public string Hdurl { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}