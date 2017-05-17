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

    public class VideoValue
    {
        public string name { get; set; }
        public string thumbnailUrl { get; set; }
        public string contentUrl { get; set; }
    }

    public class VideoRoot
    {
        public int totalEstimatedMatches { get; set; }
        public List<VideoValue> value { get; set; }
    }

}
