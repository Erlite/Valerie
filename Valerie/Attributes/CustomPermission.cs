using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Valerie.Attributes
{
    public class CustomPermission : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var AppInfo = await (Context.Client as DiscordSocketClient).GetApplicationInfoAsync();
            var User = Context.User as IGuildUser;
            if (User.GuildPermissions.Has(GuildPermission.KickMembers | GuildPermission.BanMembers | GuildPermission.Administrator | GuildPermission.ManageMessages |
                GuildPermission.ManageRoles | GuildPermission.ManageGuild) || User.Id == AppInfo.Owner.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());
            else
                return await Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** requires one of the following permission: Kick, Ban, Admin, Manage Messages/Guild/Roles"));
            
        }
    }
}
