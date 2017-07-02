using System;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class PermissionAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Command, IServiceProvider Provider)
        {
            var User = Context.User as SocketGuildUser;

            if (!(User.GuildPermissions.KickMembers || User.GuildPermissions.BanMembers || User.GuildPermissions.ManageMessages))
                return await Task.FromResult(PreconditionResult.FromError($"{Command.Name} requires one of the following permissions: KickMembers, BanMembers, ManageMessages"));
            else
                return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
