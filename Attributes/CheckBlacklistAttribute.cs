using Discord.Commands;
using System.Threading.Tasks;
using Rick.Handlers;
using System;

namespace Rick.Attributes
{
    public class CheckBlacklistAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var userId = context.User.Id;
            var Blacklist = BotHandler.BotConfig.Blacklist;
            var getUser = Blacklist.ContainsKey(userId);
            string reason;
            var getReason = Blacklist.TryGetValue(userId, out reason);
            return await Task.FromResult(getUser) ? PreconditionResult.FromError($"You are forbidden from using Bot's commands!\n**Reason:** {reason}") : PreconditionResult.FromSuccess();
        }
    }
}