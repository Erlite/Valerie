using Valerie.Enums;
using Valerie.Models;
using System.Drawing;
using Valerie.Services;
using Raven.Client.Documents;

namespace Valerie.Handlers
{
    public class GuildHandler
    {
        IDocumentStore Store { get; }
        public GuildHandler(IDocumentStore store) => Store = store;

        public GuildModel GetGuild(ulong Id) { using (var Session = Store.OpenSession()) return Session.Load<GuildModel>($"{Id}"); }

        public void RemoveGuild(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession()) Session.Delete($"{Id}");
            LogService.Write(LogSource.EVT, string.IsNullOrWhiteSpace(Name) ? $"Removed Server With Id: {Id}" : $"Removed Config For {Name}", Color.Crimson);
        }

        public void AddGuild(ulong Id, string Name = null)
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
            LogService.Write(LogSource.EVT, string.IsNullOrWhiteSpace(Name) ? $"Added Server With Id: {Id}" : $"Created Config For {Name}", Color.Orange);
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