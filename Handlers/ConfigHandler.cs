using Models;
using System;
using Valerie.Services;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers
{
    public class ConfigHandler
    {
        IDocumentSession Session { get; }
        public ConfigHandler(IDocumentStore DocumentStore)
        {
            Session = DocumentStore.OpenSession();
        }

        public ConfigModel Config
        {
            get
            {
                return Session.Load<ConfigModel>("Config");
            }
        }

        public void LoadConfig()
        {
            if (Session.Advanced.Exists("Config")) return;

            LogClient.Write(Source.CONFIG, "Enter Token: ");
            string Token = Console.ReadLine();
            LogClient.Write(Source.CONFIG, "Enter Prefix: ");
            string Prefix = Console.ReadLine();
            Session.Store(new ConfigModel
            {
                Id = "Config",
                Token = Token,
                Prefix = Prefix
            });
            Session.SaveChanges();
        }

        public void Save(ConfigModel Config)
        {
            Session.Store(Config, "Config");
            Session.SaveChanges();
        }
    }
}