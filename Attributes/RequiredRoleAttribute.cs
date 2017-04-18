using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class RequiredRoleAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var user = context.User as IGuildUser;
            return await Task.FromResult(user) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}Role is missing! Please get the appropriate role for this command!");
        }
    }
}