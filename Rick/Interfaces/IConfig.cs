using Rick.Wrappers;
using System.Collections.Generic;

namespace Rick.Interfaces
{
    public interface IConfig
    {
        string Prefix { get; set; }
        string Token { get; set; }
        int CommandsUsed { get; set; }
        int MessagesReceived { get; set; }
        string CensoredWords { get; set; }
        bool IsDebugEnabled { get; set; }
        bool IsUpdateEnabled { get; set; }
        bool IsMentionEnabled { get; set; }
        APIsWrapper APIKeys { get; set; }
        Dictionary<ulong, string> Blacklist { get; set; }
        List<string> EvalImports { get; set; }
        List<string> Games { get; set; }
        List<ulong> UpdateList { get; set; }
    }
}
