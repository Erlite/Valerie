using Newtonsoft.Json;

namespace Valerie.JsonModels
{
    public partial class DocsModel
    {
        [JsonProperty("results")]
        public DocsModel[] Results { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public partial class DocsModel
    {
        [JsonProperty("displayName")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string URL { get; set; }

        [JsonProperty("itemType")]
        public DType Type { get; set; }

        [JsonProperty("itemKind")]
        public DKind Kind { get; set; }

        [JsonProperty("description")]
        public string Snippet { get; set; }
    }

    public enum DType
    {
        Type,
        Namespace,
        Member,
    }

    public enum DKind
    {
        Namespace,
        Class,
        Enumeration,
        Method,
        Structure,
        Property,
        Constructor,
        Field,
        Event,
        Interface,
        Delegate
    }
}