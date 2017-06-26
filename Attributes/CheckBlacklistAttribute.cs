using Discord.Commands;
using System.Threading.Tasks;
using Rick.Handlers;
using System;

namespace Rick.Attributes
{
    public class CheckBlacklistAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Command, IServiceProvider Provider)
        {
            var Blacklist = ConfigHandler.IConfig.Blacklist;
            var getUser = Blacklist.ContainsKey(Context.User.Id);
            Blacklist.TryGetValue(Context.User.Id, out string reason);
            return await Task.FromResult(getUser) ? PreconditionResult.FromError($"You are forbidden from using Bot's commands!\n**Reason:** {reason}") : PreconditionResult.FromSuccess();
        }
    }
}