using Valerie.JsonModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers
{
    public class ServerHandler
    {
        IDocumentStore Store { get; }

        public ServerHandler(IDocumentStore GetStore)
        {
            Store = GetStore;
        }

        public ServerModel GetServer(ulong Id)
        {
            using (var Session = Store.OpenSession())
                return Session.Load<ServerModel>($"{Id}");
        }

        public void Remove(ulong Id)
        {
            using (var Session = Store.OpenSession())
                Session.Delete($"{Id}");
        }

        public void AddServer(ulong Id)
        {using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists($"{Id}")) return;
                Session.Store(new ServerModel
                {
                    Id = $"{Id}",
                    Prefix = "!!"
                });
                Session.SaveChanges();
            }
        }

        public void Save(ServerModel Server, ulong Id)
        {
            if (Server == null) return;
            using (var Session = Store.OpenSession())
            {
                Session.Store(Server, Server.Id);
                Session.SaveChanges();
            }
        }
    }
}