using System.Collections.Generic;

namespace Rick.Classes
{
    public class SearchValue
    {
        public string name { get; set; }
        public string displayUrl { get; set; }
        public string snippet { get; set; }
    }

    public class SearchWebPages
    {
        public int totalEstimatedMatches { get; set; }
        public List<SearchValue> value { get; set; }
    }

    public class SearchRoot
    {
        public SearchWebPages webPages { get; set; }
    }

    public class DocsResult
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public string itemType { get; set; }
        public string itemKind { get; set; }
        public string description { get; set; }
    }

    public class DocsMain
    {
        public List<DocsResult> results { get; set; }
        public int count { get; set; }
        public string @nextLink { get; set; }
    }
}
