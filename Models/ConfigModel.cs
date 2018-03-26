using System.Collections.Generic;

namespace Valerie.Models
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string ReportChannel { get; set; }
        public string ServerMessage { get; set; }
        public List<string> Games { get; set; } = new List<string>()
        {
            "with Yucked.", "Guess the prefix.", "Borderlands 2"
        };
        public List<string> Imports { get; set; } = new List<string>()
        {
            "System", "System.IO", "System.Linq", "System.Net.Http", "Newtonsoft.Json",
            "System.Threading", "System.Reflection", "System.Collections.Generic", 
        };
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();
        public Dictionary<string, string> APIKeys { get; set; } = new Dictionary<string, string>()
        { {"Giphy", "dc6zaTOxFJmzC" }, {"Google", "" }, {"Steam", "" }, {"Imgur", "" }, {"Cleverbot", "" } };
    }
}