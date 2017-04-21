using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rick.Interfaces
{
    public interface IBotConfig
    {
        string BotToken { get; set; }
        string DefaultPrefix { get; set; }
        string BingAPIKey { get; set; }
        bool DebugMode { get; set; }
        bool ClientLatency { get; set; }
    }
}
