using System.Collections.Generic;

namespace Rick.Models
{
    public class RelatedTopic
    {
        public string FirstURL { get; set; }
        public string Text { get; set; }
    }

    public class DuckDuckGo
    {
        public string Heading { get; set; }
        public List<RelatedTopic> RelatedTopics { get; set; }
        public string AbstractURL { get; set; }
        public string Image { get; set; }
        public string Abstract { get; set; }
    }
}
