using Discord.WebSocket;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules.Addons
{
    public class Globals
    {
        public IContext Context { get; internal set; }
        public SocketGuild Guild { get; internal set; }
        public SocketGuildUser User { get; internal set; }
        public DiscordSocketClient Client { get; internal set; }       
        public SocketGuildChannel Channel { get; internal set; }
    }
}