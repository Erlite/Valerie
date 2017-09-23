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

        public Task SaveAsync(ServerModel Model, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                Session.StoreAsync(Model, id: $"{GuildId}");
                Session.SaveChangesAsync();
                Session.Dispose();
            }
            return Task.CompletedTask;
        }

        public async Task LoadOrDeleteAsync(Actions Action, ulong GuildId)
        {
            using (IAsyncDocumentSession Session = MainHandler.Store.OpenAsyncSession())
            {
                var Load = await Session.LoadAsync<ServerModel>($"{GuildId}");
                switch (Action)
                {
                    case Actions.Add:
                        if (Load == null)
                            await Session.StoreAsync(new ServerModel
                            {
                                Id = $"{GuildId}",
                                Prefix = "?>"
                            }).ConfigureAwait(false);
                        break;
                    case Actions.Delete:
                        if (Load != null)
                            Session.Delete($"{GuildId}");
                        break;
                }
                await Session.SaveChangesAsync().ConfigureAwait(false);
                Session.Dispose();
            }
        }
    }
}
