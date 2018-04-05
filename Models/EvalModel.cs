using Valerie.Addons;
using Discord.WebSocket;

namespace Valerie.Models
{
    public class EvalModel
    {
        public IContext Context { get; set; }
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public DiscordSocketClient Client { get; set; }
        public SocketGuildChannel Channel { get; set; }
    }
}