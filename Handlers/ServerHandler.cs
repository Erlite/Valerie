﻿using Valerie.Services;
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

        public void MemoryUpdate(ulong GuildId, ulong UserId, int Bytes)
        {
            var Server = GetServer(GuildId);
            var MemUser = Server.Profiles[UserId];
            if (MemUser == null)
                Server.Profiles.Add(UserId, new UserProfile
                {
                    Bytes = Bytes,
                    DailyStreak = 0,
                    DailyReward = DateTime.Now
                });
            MemUser.Bytes += Bytes;
            Save(Server, GuildId);
        }
    }
}