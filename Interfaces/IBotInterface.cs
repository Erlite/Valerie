using System.Collections.Generic;

namespace Rick.Interfaces
{
    public interface IBotInterface
    {
        string BotToken { get; set; }
        string BotName { get; set; }
        string DefaultPrefix { get; set; }
        string BingAPIKey { get; set; }
        bool MentionDefaultPrefix { get; set; }
        bool DebugMode { get; set; }
        bool ClientLatency { get; set; }
        Dictionary<ulong, string> Blacklist { get; set; }
    }
}