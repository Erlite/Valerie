using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Rick.Models;

namespace Rick.Attributes
{
    public class RequireChannelAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IDependencyMap map)
        {
            var ChannelUlong = GuildModel.GuildConfigs[context.Guild.Id].RequiredChannelIDs.Contains(context.Channel.Id);
            var ChannelName = GuildModel.GuildConfigs[context.Guild.Id].RequiredChannelNames.Contains(context.Channel.Name);
            var gld = context.Guild as SocketGuild;
            return await Task.FromResult(ChannelName || ChannelUlong) ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"{Format.Bold("ERROR: ")}This command cannot be used in this channel!");
        }
    }
}
