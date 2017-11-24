using Models;
using Raven.Client.Documents;
using System.Threading.Tasks;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers
{
    public class ServerHandler
    {
        IDocumentSession Session { get; }
        IAsyncDocumentSession AsyncSession { get; }
        public ServerHandler(IDocumentStore DocumentStore)
        {
            Session = DocumentStore.OpenSession();
            AsyncSession = DocumentStore.OpenAsyncSession();
        }

        public ServerModel GetServer(ulong Id) => Session.Load<ServerModel>($"{Id}");

        public async Task AddServerAsync(ulong Id)
        {
            if (await AsyncSession.ExistsAsync($"{Id}")) return;
            await AsyncSession.StoreAsync(new ServerModel
            {
                Id = $"{Id}",
                Prefix = "!!"
            });
            await AsyncSession.SaveChangesAsync().ConfigureAwait(false);
        }

        public void Remove(ulong Id) => Session.Delete($"{Id}");

        public async Task SaveAsync(ServerModel Server, ulong Id)
        {
            await AsyncSession.StoreAsync(Server, Server.Id).ConfigureAwait(false);
            await AsyncSession.SaveChangesAsync().ConfigureAwait(false);
        }
        
    }
}