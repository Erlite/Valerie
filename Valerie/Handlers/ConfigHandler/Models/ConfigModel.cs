using System.Collections.Generic;

namespace Valerie.Handlers.ConfigHandler.Models
{
    public class ConfigModel
    {
        public string Id { get; set; }
        public string Token { get; set; }
        public string Prefix { get; set; }
        public string GuildJoinMessage { get; set; }
        public int CommandsUsed { get; set; }
        public int MessagesReceived { get; set; }
        public string ReportChannel { get; set; }
        public string CensoredWords { get; set; } =
            "\\\b(f+u+c+k+|b+i+t+c+h+|w+h+o+r+e+|c+u+n+t+|a+ss+h+o+l+e+|a+s+s+|n+i+g+g+e+r+|d+i+c+k+|p+u+s+s+y+|f+a+g+g+o+t+)(w+i+t+|e+r+|i+n+g+)?\\\b";
        public KeysModel APIKeys { get; set; } = new KeysModel();
        public List<string> EvalImports { get; set; } = new List<string>();
        public List<string> Games { get; set; } = new List<string>();
        public Dictionary<ulong, string> Blacklist { get; set; } = new Dictionary<ulong, string>();
    }
}
