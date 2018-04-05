using System;
using Valerie.Enums;
using Valerie.Addons;
using Discord.Commands;
using System.Threading.Tasks;

namespace Valerie.Preconditions
{
    public class RequirePermission : PreconditionAttribute
    {
        AccessLevel AccessLevel { get; }
        public RequirePermission(AccessLevel accessLevel)
        {
            AccessLevel = accessLevel;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo Info, IServiceProvider Provider)
        {
            var Context = context as IContext;
            //var Admins = Context.Server
        }
    }
}