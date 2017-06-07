using System.Collections.Generic;
using Rick.Enums;

namespace Rick.JsonResponse
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

    public class DocsMembers
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public DocsType itemType { get; set; }
        public DocsKind itemKind { get; set; }
        public string description { get; set; }
    }

    public class DocsRoot
    {
        public List<DocsMembers> results { get; set; }
        public int count { get; set; }
    }
}
