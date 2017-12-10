using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public partial class ImgurModel
    {
        [JsonProperty("data")]
        public ImgurData Data { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public partial class ImgurData
    {
        [JsonProperty("images")]
        public ImgurImages[] Images { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }

    public partial class ImgurImages
    {
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}