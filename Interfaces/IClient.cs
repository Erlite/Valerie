using System.Collections.Generic;
using Discord.WebSocket;
using Rick.Models;

namespace Rick.Interfaces
{
    public interface IClient
    {
        string BotToken { get; set; }
        string BotName { get; set; }
        string DefaultPrefix { get; set; }
        int CommandsUsed { get; set; }
        int MessagesReceived { get; set; }
        bool DebugMode { get; set; }
        bool ClientLatency { get; set; }
        bool AutoUpdate { get; set; }
        bool MentionDefaultPrefix { get; set; }
        APIKeysWrapper APIKeys { get; set; }
        TwitterWrapper Twitter { get; set; }
        Dictionary<ulong, string> Blacklist { get; set; }
        List<string> EvalImports { get; set; }
        List<string> Games { get; set; }
        bool MentionPrefix(SocketUserMessage message, DiscordSocketClient client, ref int argPos);
    }
}