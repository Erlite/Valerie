using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Valerie.Attributes
{
    public class RequireNsfw : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var Chn = Context.Channel as Discord.ITextChannel;
            if (Chn.IsNsfw || Chn.Name.Contains("nsfw"))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** command can only be ran in NSFW channel, pervert."));
        }
    }
}
