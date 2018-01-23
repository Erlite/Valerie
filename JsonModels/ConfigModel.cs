using System.Collections.Generic;

namespace Valerie.JsonModels
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string Version { get; set; }
        public string ReportChannel { get; set; }
        public string ServerMessage { get; set; }
        public List<string> Games { get; set; } = new List<string>();
        public List<string> Imports { get; set; } = new List<string>();
        public ApplicationKeys ApplicationKeys { get; set; } = new ApplicationKeys();
        public Dictionary<ulong, string> UsersBlacklist { get; set; } = new Dictionary<ulong, string>();
    }
}