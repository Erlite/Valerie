using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rick.Handlers;
using System.Linq;
using System.Threading.Tasks;

namespace Rick.Attributes
{
    public class RequireNSFWChannelAttribute : PreconditionAttribute
    {
        private GuildHandler GuildHandler;

        ulong[] ChannelsList;
        public RequireNSFWChannelAttribute(params ulong[] RequiredChannel)
        {
            ChannelsList = RequiredChannel;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var ChannelName = context.Channel.Name.Contains("nsfw");
            var ChannelUlong = ChannelsList.Contains(context.Channel.Id);
            var gld = context.Guild as SocketGuild;
            //var ChannelUlong = GuildHandler.GuildConfig[gld.Id].RequiredChannelIDs;
            //var ChannelName = GuildHandler.GuildConfig[gld.Id].RequiredChannelNames;
            return await Task.FromResult(ChannelName || ChannelUlong) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}This command can only be used in a channel named \"nsfw\"!");
        }
    }
}
