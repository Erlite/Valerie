using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Valerie.Attributes
{
    public class RequireNSFW : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            if (IsNSFW(Context.Channel))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("Command can only be ran in NSFW channel, pervert."));
        }

        static bool IsNSFW(IChannel Channel) =>
            Channel.Name.Contains("nsfw");
    }
}