using System;
using Valerie.Models;
using Valerie.Services;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class GuildHandler
    {
        IDocumentStore Store { get; }
        public GuildHandler(IDocumentStore store) => Store = store;

        public GuildModel GetGuild(ulong Id) { using (var Session = Store.OpenSession()) return Session.Load<GuildModel>($"{Id}"); }

        public void RemoveGuild(ulong Id, string Event, string Name = null)
        {
            using (var Session = Store.OpenSession()) Session.Delete($"{Id}");
            LogService.Write(Event, string.IsNullOrWhiteSpace(Name) ? $"Removed Server With Id: {Id}" : $"Removed Config For {Name}", ConsoleColor.DarkRed);
        }

        public void AddGuild(ulong Id, string Event, string Name = null)
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
            LogService.Write(Event, string.IsNullOrWhiteSpace(Name) ? $"Added Server With Id: {Id}" : $"Created Config For {Name}", ConsoleColor.DarkCyan);
        }

        public void Save(GuildModel Server)
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