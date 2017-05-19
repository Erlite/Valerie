using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System.Linq;

namespace Rick.Attributes
{
    public class PermissionCheck : PreconditionAttribute
    {
        public GuildPermission[] Perms { get; }

        public PermissionCheck(params GuildPermission[] permission)
        {
            Perms = permission;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var guildUser = await context.Guild.GetCurrentUserAsync();
            if (!guildUser.GuildPermissions.Has(Perms.FirstOrDefault()))
                return PreconditionResult.FromError($"**{Perms.FirstOrDefault()}** is required for {command.Name}!");
            return PreconditionResult.FromSuccess();
        }
    }
}
