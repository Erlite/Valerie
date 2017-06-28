using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Database.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Database.Interfaces;
using Database.Wrappers;
using Raven.Client;
using Raven.Client.Document;

namespace Database
{
    public class MainHandler
    {
        public void Load()
        {
            using (IDocumentStore store = new DocumentStore
            {
                Url = "http://localhost:8080/",
                DefaultDatabase = "GuildConfig",
            })
            {
                store.Initialize();

                using (IDocumentSession Session = store.OpenSession())
                {
                    GuildModel Model = new GuildModel();

                    Session.Store(Model);
                    Session.SaveChanges();
                }
            }
        }
    }
}
