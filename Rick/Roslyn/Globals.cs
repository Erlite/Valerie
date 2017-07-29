using Discord.Commands;
using Discord.WebSocket;

namespace Rick.Roslyn
{
    public class Globals
    {
        public ICommandContext Context { get; internal set; }
        public DiscordSocketClient Client { get; internal set; }
        public SocketGuildUser SocketGuildUser { get; internal set; }
        public SocketGuild SocketGuild { get; internal set; }
        public SocketGuildChannel SocketGuildChannel { get; internal set; }
    }
}
