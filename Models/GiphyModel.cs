using System.Collections.Generic;

namespace Rick.Models
{
    public class Datum
    {
        public string embed_url { get; set; }
    }

    public class GiphyRoot
    {
        public List<Datum> data { get; set; }
    }
}
