using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class RequiredChannelAttribute : PreconditionAttribute
    {
        ulong[] ChannelsList;
        public RequiredChannelAttribute(params ulong[] RequiredChannel)
        {
            ChannelsList = RequiredChannel;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var ChannelName = context.Channel.Name.Contains("spam");
            var ChannelUlong = ChannelsList.Contains(context.Channel.Id);
            return await Task.FromResult(ChannelName || ChannelUlong) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}This command cannot be used here! Please make sure you are in correct channel!");
        }
    }
}
