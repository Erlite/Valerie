using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Meeseeks.Services
{
    public class ExtraService
    {
        public static bool IsChannelListed(IChannel channel, List<string> list, bool allow_others = true)
        {
            if (!(channel is ITextChannel))
                return (allow_others ? true : false);
            if (list.Count == 0)
                return true;
            return list.Contains(channel.Name);
        }
    }
}
