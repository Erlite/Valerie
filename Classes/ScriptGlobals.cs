using Discord.Commands;
using Discord.WebSocket;

namespace Rick.Classes
{
    public class ScriptGlobals
    {
        public SocketGuildChannel channel => context.Channel as SocketGuildChannel;
        public DiscordSocketClient client { get; internal set; }
        public CommandContext context { get; internal set; }
        public SocketGuild guild => context.Guild as SocketGuild;
        public SocketMessage msg => context.Message as SocketMessage;
        public SocketUser user => context.User as SocketUser;
        public SocketGuildUser usr => context.User as SocketGuildUser;
    }
}
