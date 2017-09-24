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
        public RequireSchmeckles(int RequiredSchmeckles) => Schmeckles = RequiredSchmeckles;

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var GetService = Provider.GetService<ServerConfig>();
            var Config = GetService.LoadConfig(Context.Guild.Id);
            var GetUserEridium = Config.EridiumHandler.UsersList[Context.User.Id];
            var UserSchmeckles = IntExtension.ConvertToSchmeckles(GetUserEridium);
            int ConvertedEridium = IntExtension.ConvertToEridium(Schmeckles);

            var GetOwner = await Context.Client.GetApplicationInfoAsync();
            if (Context.User == GetOwner.Owner)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (Schmeckles > UserSchmeckles)
                return await Task.FromResult(PreconditionResult.FromError($"{Discord.Format.Bold(Info.Name)} requires **{Schmeckles}** Schmeckles."));

            Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, GetUserEridium - ConvertedEridium, GetUserEridium);
            await GetService.SaveAsync(Config, Context.Guild.Id);
            return await Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}