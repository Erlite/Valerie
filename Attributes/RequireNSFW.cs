using System;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Extensions;

namespace Valerie.Attributes
{
    public class RequireNSFW : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            if (BoolExtension.IsNSFW(Context.Channel as Discord.ITextChannel))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** command can only be ran in NSFW channel, pervert."));
        }
    }
}