using Discord.Commands;
using Discord.WebSocket;

namespace Rick.Models
{
    public class ScriptGlobals
    {
        public CommandContext Context { get; internal set; }
        public DiscordSocketClient Client { get; internal set; }
        public SocketGuildChannel Channel => Context.Channel as SocketGuildChannel;
        public SocketGuildUser User => Context.User as SocketGuildUser;
        public SocketReaction Reaction => Context.Message.Reactions as SocketReaction;
        public SocketGuild Guild => Context.Guild as SocketGuild;
        public SocketMessage Message => Context.Message as SocketMessage;
        public SocketUser SocketUser => Context.User as SocketUser;
        public SocketRole Role => Context.Guild.Roles as SocketRole;
    }
}
