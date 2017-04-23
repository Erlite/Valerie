using Discord.Commands;
using System.Threading.Tasks;
using Rick.Models;


namespace Rick.Attributes
{
    public class RequiredBlacklistAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var userId = context.User.Id;
            var Blacklist = BotModel.BotConfig.Blacklist;
            var getUser = Blacklist.ContainsKey(userId);
            string reason;
            var getReason = Blacklist.TryGetValue(userId, out reason);
            return await Task.FromResult(getUser) ? PreconditionResult.FromError($"You are forbidden from using Bot's commands!\n**Reason:** {reason}") : PreconditionResult.FromSuccess();
        }
    }
}