using System.Collections.Generic;

namespace Rick.Models
{
    public class ProfileModel
    {
        public string Username { get; set; }
        public string ProfileMessage { get; set; }
        public string BgColor { get; set; }
        public int Karma { get; set; }
        public int Level { get; set; }
        public List<string> Titles { get; set; } = new List<string>();
    }
}
