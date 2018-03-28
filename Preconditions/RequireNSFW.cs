using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Valerie.Preconditions
{
    public class RequireNSFW : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var Channel = Context.Channel as Discord.ITextChannel;
            if (Channel.IsNsfw || Channel.Name.Contains("nsfw")) return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** command can only be ran in NSFW channel, pervert."));
        }
    }
}