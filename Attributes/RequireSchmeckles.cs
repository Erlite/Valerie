using System;
using System.Threading.Tasks;
using Discord.Commands;
using Valerie.Handlers.Server;
using Valerie.Extensions;

namespace Valerie.Attributes
{
    public class RequireSchmeckles : PreconditionAttribute
    {
        int Schmeckles { get; }

        public RequireSchmeckles(int RequiredSchmeckles = 25)
        {
            Schmeckles = RequiredSchmeckles;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext Context, CommandInfo Info, IServiceProvider Provider)
        {
            var GuildConfig = await ServerConfig.ConfigAsync(Context.Guild.Id).ConfigureAwait(false);

            var GetUserEridium = GuildConfig.EridiumHandler.UsersList[Context.User.Id];
            var UserSchmeckles = IntExtension.ConvertToSchmeckles(GetUserEridium);
            int ConvertedEridium = IntExtension.ConvertToEridium(Schmeckles);

           if (Schmeckles > UserSchmeckles)
                return await Task.FromResult(PreconditionResult.FromError($"{Discord.Format.Bold(Info.Name)} requires **{Schmeckles}** Schmeckles."));
            else
            {
                //await ServerDB.EridiumHandlerAsync(Context.Guild.Id, ModelEnum.EridiumSubtract, Context.User.Id, ConvertedEridium);
                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}
