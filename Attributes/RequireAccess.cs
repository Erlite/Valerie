using System;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Attributes
{
    public class RequireAccess : PreconditionAttribute
    {
        AccessLevel GetAccessLevel;
        public RequireAccess(AccessLevel Level) => GetAccessLevel = Level;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo Command, IServiceProvider Services)
        {
            var Context = (context as IContext);
            var User = Context.User as IGuildUser;
            var AdminPerms = (User.Id == Context.Guild.OwnerId || User.GuildPermissions.Administrator || Context.Server.Admins.Contains(User.Id) ||
                User.Id == Context.Client.GetApplicationInfoAsync().GetAwaiter().GetResult().Owner.Id);
            var ModPerms = (User.GuildPermissions.KickMembers || User.GuildPermissions.BanMembers || User.GuildPermissions.ManageRoles || User.GuildPermissions.ManageMessages);
            if (GetAccessLevel == AccessLevel.Admins && AdminPerms) return Task.FromResult(PreconditionResult.FromSuccess());
            else if (GetAccessLevel == AccessLevel.Mods && ModPerms) return Task.FromResult(PreconditionResult.FromSuccess());
            else if (GetAccessLevel == AccessLevel.AdminsNMods && (AdminPerms || ModPerms)) return Task.FromResult(PreconditionResult.FromSuccess());
            else return Task.FromResult(PreconditionResult.FromError($"{Command.Name} is limited to {GetAccessLevel}."));
        }
    }

    public enum AccessLevel
    {
        Admins,
        Mods,
        AdminsNMods
    }
}