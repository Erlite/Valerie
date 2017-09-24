using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Valerie.Attributes
{
    public class CustomUserPermission : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var AppInfo = (Context.Client as DiscordSocketClient).GetApplicationInfoAsync().GetAwaiter().GetResult();
            var User = Context.User as IGuildUser;
            if (User.GuildPermissions.Administrator || User.GuildPermissions.ManageGuild || User.Id == AppInfo.Owner.Id || Context.User.Id == Context.Guild.OwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** command required Admin or ManageGuild permission."));
        }
    }
}