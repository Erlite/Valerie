using System;

namespace Rick.Classes
{
    public class Tag
    {
        public string tag;
        public string text;
        public ulong creator;
        public DateTime when;

        public Tag(string tag, string text, ulong creator_id, DateTime when)
        {
            this.tag = tag;
            this.text = text;
            this.creator = creator_id;
            this.when = when;
        }
    }
}