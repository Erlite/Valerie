using Discord.Commands;
using System.Threading.Tasks;
using Rick.Handlers;
using System;

namespace Rick.Attributes
{
    public class RequireChannelAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider map)
        {
            var ChannelName = GuildHandler.GuildConfigs[context.Guild.Id].RequiredChannelNames.Contains(context.Channel.Name);
            return await Task.FromResult(ChannelName) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"This command cannot be used in this channel! Please make sure you are in the correct channel first!");
        }
    }
}
