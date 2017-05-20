using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rick.Classes
{
    public class Newsitem
    {
        public string title { get; set; }
        public string url { get; set; }
        public string contents { get; set; }
        public string feedlabel { get; set; }
    }

    public class Appnews
    {
        public List<Newsitem> newsitems { get; set; }
        public int count { get; set; }
    }

    public class SteamAppNews
    {
        public Appnews appnews { get; set; }
    }
}
