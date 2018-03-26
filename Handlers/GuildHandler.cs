using Valerie.Models;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class GuildHandler
    {
        IDocumentStore Store { get; }
        public GuildHandler(IDocumentStore store) => Store = store;

        public GuildModel GetServer(ulong Id) { using (var Session = Store.OpenSession()) return Session.Load<GuildModel>($"{Id}"); }

        public void RemoveServer(ulong Id) { using (var Session = Store.OpenSession()) Session.Delete($"{Id}"); }

        public void AddServer(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists($"{Id}")) return;
                Session.Store(new GuildModel
                {
                    Id = $"{Id}",
                    Prefix = "!"
                });
                Session.SaveChanges();
            }
        }

        public void Save(GuildModel Server, ulong Id)
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