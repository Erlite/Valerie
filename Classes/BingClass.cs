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
}
