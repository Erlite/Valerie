using System.Threading.Tasks;
using Raven.Client.Documents.Session;
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

        public Task Save(ServerModel Model, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                Session.StoreAsync(Model, id: $"{GuildId}");
                Session.SaveChangesAsync();
                Session.Dispose();
            }
            return Task.CompletedTask;
        }

        public static async Task LoadOrDeleteAsync(Actions Action, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
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
                    case Actions.Delete:
                        if (await Session.ExistsAsync($"{GuildId}").ConfigureAwait(false))
                            Session.Delete($"{GuildId}");
                        break;
                }
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }
    }
}
