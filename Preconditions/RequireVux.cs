using System;
using Valerie.Addons;
using Discord.Commands;
using System.Threading.Tasks;

namespace Valerie.Preconditions
{
    public class RequireVux : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo Info, IServiceProvider Provider)
        {
            var Context = context as IContext;
            return Context.Config.VuxUsers.Contains(Context.User.Id) ? Task.FromResult(PreconditionResult.FromSuccess()) : 
                Task.FromResult(PreconditionResult.FromError($"**{Info.Name}** is only for Vux users. Use `{Context.Config.Prefix}Vux` for more information."));
        }
    }
}