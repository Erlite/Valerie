using System;
using Valerie.JsonModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers
{
    public class ServerHandler
    {
        IDocumentStore Store { get; }
        IDocumentSession Session { get; }

        public ServerHandler(IDocumentStore GetStore, IDocumentSession GetSession)
        {
            Store = GetStore;
            Session = GetSession;
        }

        public ServerModel GetServer(ulong Id) => Session.Load<ServerModel>($"{Id}");

        public void Remove(ulong Id) => Session.Delete($"{Id}");

        public void AddServer(ulong Id)
        {
            if (Session.Advanced.Exists($"{Id}")) return;
            Session.Store(new ServerModel
            {
                Id = $"{Id}",
                Prefix = "!!"
            });
            Session.SaveChanges();
        }

        public void Save(ServerModel Server, ulong Id)
        {
            if (Server == null) return;
            Session.Store(Server, Server.Id);
            Session.SaveChanges();
        }
    }
}