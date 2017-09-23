using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.Handlers.Server;

namespace Valerie.Attributes
{
    public class RequireSchmeckles : PreconditionAttribute
    {
        int Schmeckles { get; }
        public RequireSchmeckles(int RequiredSchmeckles = 25)
        {
            Schmeckles = RequiredSchmeckles;
        }

        public override Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var GetService = Provider.GetService<ServerConfig>();
            var Config = GetService.LoadConfig(Context.Guild.Id);
            var GetUserEridium = Config.EridiumHandler.UsersList[Context.User.Id];
            var UserSchmeckles = IntExtension.ConvertToSchmeckles(GetUserEridium);
            int ConvertedEridium = IntExtension.ConvertToEridium(Schmeckles);

            if (Schmeckles > UserSchmeckles)
                return Task.FromResult(PreconditionResult.FromError($"{Discord.Format.Bold(Info.Name)} requires **{Schmeckles}** Schmeckles."));
            else
            {
                Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, GetUserEridium - ConvertedEridium, GetUserEridium);
                GetService.SaveAsync(Config, Context.Guild.Id);
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
