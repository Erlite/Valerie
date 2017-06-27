using System;
using System.Collections.Generic;
using System.Text;

namespace Rick.Models
{
    public class ArchiveModel
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
