using Discord;
using Discord.Commands;
using Rick.Handlers;
using System.Linq;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class RequiredRoleAttribute : PreconditionAttribute
    {
        private GuildHandler GuildHandler;

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as IGuildUser;
            var role = user.RoleIds.Intersect(GuildHandler.RequiredRoleID).Any();
            return await Task.FromResult(role) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}Role is missing! Please get the appropriate role for this command!");
        }
    }
}