using Newtonsoft.Json;
using System.IO;

namespace Rick.Classes
{
    public class UsersProfile
    {
        public ulong UserID { get; set; }
        public string ProfileMsg { get; set; } = "Who am I? What am I???";
        public double Karma { get; set; }
        public double Level { get; set; }
    
    }
}