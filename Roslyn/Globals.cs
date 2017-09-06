using Discord.Commands;
using Discord.WebSocket;

namespace Valerie.Roslyn
{
    public class Globals
    {
        public ICommandContext Context { get; internal set; }
        public DiscordSocketClient Client { get; internal set; }
        public SocketGuildUser User { get; internal set; }
        public SocketGuild Guild { get; internal set; }
        public SocketGuildChannel Channel { get; internal set; }
    }
}
