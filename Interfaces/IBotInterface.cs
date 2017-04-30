using System.Collections.Generic;
using Discord.WebSocket;

namespace Rick.Interfaces
{
    public interface IBotInterface
    {
        string BotToken { get; set; }
        string BotName { get; set; }
        string BotGame { get; set; }
        string DefaultPrefix { get; set; }
        string BingAPIKey { get; set; }
        string MashapeKey { get; set; }
        bool DebugMode { get; set; }
        bool ClientLatency { get; set; }
        bool AutoUpdate { get; set; }
        bool MentionDefaultPrefix { get; set; }
        Dictionary<ulong, string> Blacklist { get; set; }
        Dictionary<ulong, string> OwnerAfk { get; set; }
        List<string> EvalImports { get; set; }
        bool MentionDefaultPrefixEnabled(SocketUserMessage message, DiscordSocketClient client, ref int argPos);
    }
}