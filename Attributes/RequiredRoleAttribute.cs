using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;
using Rick.Handlers;
using System;

namespace Rick.Attributes
{
    public class RequiredRoleAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var user = context.User as IGuildUser;
            var role = user.RoleIds.Intersect(GuildHandler.GuildConfigs[context.Guild.Id].RequiredRoleIDs).Any();
            return await Task.FromResult(role) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}Role is missing! Please get the appropriate role for this command!");
        }
    }
}