using System;

namespace Valerie.Modules.Addons
{
    public class Archive
    {
        public string Author { get; set; }
        public string Message { get; set; }
        public string Attachments { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}