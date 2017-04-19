using Discord;
using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class RequireNSFWChannelAttribute : PreconditionAttribute
    {
        ulong[] ChannelsList;
        public RequireNSFWChannelAttribute(params ulong[] RequiredChannel)
        {
            ChannelsList = RequiredChannel;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var ChannelName = context.Channel.Name.Contains("nsfw");
            var ChannelUlong = ChannelsList.Contains(context.Channel.Id);
            return await Task.FromResult(ChannelName || ChannelUlong) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}This command can only be used in a channel named \"nsfw\"!");
        }
    }
}
