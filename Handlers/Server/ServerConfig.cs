using System.Threading.Tasks;
using Raven.Client.Documents.Session;
using Valerie.Modules.Enums;
using Valerie.Handlers.Server.Models;

namespace Valerie.Handlers.Server
{
    public class ServerConfig
    {
        public static ServerModel Config = null;

        public static async Task<ServerModel> ConfigAsync(ulong GuildId)
        {
            using (IAsyncDocumentSession Session = Database.Store.OpenAsyncSession())
            {
                var Load = await Session.LoadAsync<ServerModel>($"{GuildId}").ConfigureAwait(false);
                Config = Load;
                return Load;
            }
        }

        public static async Task LoadOrDeleteAsync(Actions Action, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = Database.Store.OpenAsyncSession())
            {
                switch (Action)
                {
                    case Actions.Add:
                        if (!await Session.ExistsAsync($"{GuildId}").ConfigureAwait(false))
                            await Session.StoreAsync(new ServerModel
                            {
                                Id = $"{GuildId}",
                                Prefix = "?>"
                            }).ConfigureAwait(false);
                        break;
                    case Actions.Remove:
                        if (await Session.ExistsAsync($"{GuildId}").ConfigureAwait(false))
                            Session.Delete($"{GuildId}");
                        break;
                }
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }

        public static Task SaveAsync()
        {
            using (IAsyncDocumentSession SaveSession = Database.Store.OpenAsyncSession())
            {
                SaveSession.StoreAsync(Config);
                SaveSession.SaveChangesAsync();
                SaveSession.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
