using System;
using Discord.Commands;
using System.Threading.Tasks;

namespace Valerie.Addons.Preconditions
{
    public class RequireNSFW : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var Channel = Context.Channel as Discord.ITextChannel;
            if (Channel.IsNsfw) return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** command can only be ran in NSFW channel, pervert."));
        }
    }
}
