using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Valerie.Handlers.Config.Models
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public int CommandUsed { get; set; }
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string ServerMessage { get; set; }
        public string ReportChannel { get; set; }
        public string CensoredWords { get; set; } =
            "\\\b(f+u+c+k+|b+i+t+c+h+|w+h+o+r+e+|c+u+n+t+|a+ss+h+o+l+e+|a+s+s+|n+i+g+g+e+r+|d+i+c+k+|p+u+s+s+y+|f+a+g+g+o+t+)(w+i+t+|e+r+|i+n+g+)?\\\b";
        public KeysModel APIKeys { get; set; } = new KeysModel();
        public IList<string> EvalImports { get; set; } = new List<string>() { "System", "System.Collections.Generic", "System.Linq" };
        public IList<string> BotGames { get; set; } = new List<string>() { "With You", "Tumblr" };
        public ConcurrentDictionary<ulong, string> UsersBlacklist { get; set; } = new ConcurrentDictionary<ulong, string>();
    }
}