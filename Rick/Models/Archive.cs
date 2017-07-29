using System;

namespace Rick.Models
{
    public class Archive
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
