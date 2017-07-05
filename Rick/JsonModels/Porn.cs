using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rick.JsonModels
{
    public class Porn
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("result")]
        public List<VideoModel> VideoModel { get; set; }
    }

    public class VideoModel
    {
        [JsonProperty("url")]
        public string VideoUrl { get; set; }
        [JsonProperty("thumb")]
        public string VideoThumb { get; set; }
        [JsonProperty("title")]
        public string VideoTitle { get; set; }
        [JsonProperty("duration")]
        public int duration { get; set; }
        [JsonProperty("views")]
        public int views { get; set; }
    }

}
