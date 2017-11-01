using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Valerie.Attributes
{
    public class ServerLock : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            if (Context.Guild.Id == 226838224952098820)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(string.Empty));
        }
    }
}
