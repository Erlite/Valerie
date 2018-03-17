using System;
using Valerie.Services;
using Valerie.JsonModels;
using Raven.Client.Documents;

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

        public void RemoveServer(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession()) Session.Delete($"{Id}");
            string Msg = string.IsNullOrWhiteSpace(Name) ? $"Removed Server With Id: {Id}" : $"Removed Config For {Name}";
            LogClient.Write(Source.SERVER, Msg);
        }

        public void AddServer(ulong Id, string Name = null)
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
            string Msg = string.IsNullOrWhiteSpace(Name) ? $"Added Server With Id: {Id}" : $"Created Config For {Name}";
            LogClient.Write(Source.SERVER, Msg);
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

        public void MemoryUpdate(ulong GuildId, ulong UserId, int Bytes)
        {
            var Server = GetServer(GuildId);
            if (!Server.Profiles.ContainsKey(UserId)) Server.Profiles.Add(UserId, new UserProfile
            {
                Bytes = Bytes,
                DailyStreak = 0,
                DailyReward = DateTime.Now
            });
            else
            {
                var MemUser = Server.Profiles[UserId];
                MemUser.Bytes += Bytes;
            }
            Save(Server, GuildId);
        }
    }
}