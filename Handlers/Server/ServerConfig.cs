using Raven.Client.Documents.Session;
using System.Threading.Tasks;
using Valerie.Enums;
using Valerie.Handlers.Server.Models;

namespace Valerie.Handlers.Server
{
    public class ServerConfig
    {
        public ServerModel LoadConfig(ulong GuildId)
        {
            using (IDocumentSession Session = MainHandler.Store.OpenSession())
                return Session.Load<ServerModel>($"{GuildId}");
        }

        public async Task SaveAsync(ServerModel Model, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                await Session.StoreAsync(Model, id: $"{GuildId}").ConfigureAwait(false);
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }

        public async Task LoadOrDeleteAsync(Actions Action, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                switch (Action)
                {
                    case Actions.Add:
                        if (!await Session.ExistsAsync($"{GuildId}"))
                            await Session.StoreAsync(
                                new ServerModel
                                {
                                    Id = $"{GuildId}",
                                    Prefix = "?>"
                                }).ConfigureAwait(false);
                        break;
                    case Actions.Delete:
                        if (await Session.ExistsAsync($"{GuildId}")) Session.Delete($"{GuildId}"); break;
                }
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }
    }
}