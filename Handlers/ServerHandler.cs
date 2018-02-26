using Valerie.Services;
using Valerie.JsonModels;
using Raven.Client.Documents;
using System.Linq;
using System;

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

        public void RemoveServer(ulong Id)
        {
            using (var Session = Store.OpenSession())
                Session.Delete($"{Id}");
            LogClient.Write(Source.SERVER, $"Removed Server With Id: {Id}");
        }

        public void AddServer(ulong Id)
        {
            using (var Session = Store.OpenSession())
            {
                if (Session.Advanced.Exists($"{Id}")) return;
                Session.Store(new ServerModel
                {
                    Id = $"{Id}",
                    Prefix = "!!"
                });
                Session.SaveChanges();
            }
            LogClient.Write(Source.SERVER, $"Added Server With Id: {Id}");
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

        public void MemoryUpdate(ulong GuildId, ulong UserId, float Bytes)
        {
            var Server = GetServer(GuildId);
            var MemUser = Server.Memory.FirstOrDefault(x => x.Id == $"{UserId}");
            if (MemUser == null)
                Server.Memory.Add(new MemoryWrapper
                {
                    Byte = Bytes,
                    Id = $"{UserId}",
                    DailyReward = DateTime.Now
                });
            MemUser.Byte += Bytes;
            Save(Server, GuildId);
        }
    }
}