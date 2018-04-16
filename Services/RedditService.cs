using System.Linq;
using Valerie.Models;
using Valerie.Handlers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using Discord.WebSocket;
using Raven.Client.Documents;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Services
{
    public class RedditService
    {
        HttpClient HttpClient { get; }
        DiscordSocketClient Client { get; }
        ConfigHandler ConfigHandler { get; }
        DocumentStore DocumentStore { get; }
        Dictionary<ulong, (List<string>, Timer)> ChannelTracker { get; set; }
        public RedditService(HttpClient httpClient, ConfigHandler configHandler, DocumentStore documentStore, DiscordSocketClient client)
        {
            Client = client;
            HttpClient = httpClient;
            ConfigHandler = configHandler;
            DocumentStore = documentStore;
            ChannelTracker = new Dictionary<ulong, (List<string>, Timer)>();
        }

        public async Task InitializeAsync()
        {
            List<GuildModel> Configs;
            using (var Session = DocumentStore.OpenSession())
                Configs = Session.Query<GuildModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            foreach (var Server in Configs.Where(x => x.Reddit.IsEnabled && x.Reddit.TextChannel != 0 && x.Reddit.Subreddits.Any()))
            {
                var Channel = Client.GetChannel(Server.Reddit.TextChannel);
                foreach (var Subbredit in Server.Reddit.Subreddits)
                {
                    var CheckSub = await SubredditAsync(Subbredit).ConfigureAwait(false);
                    if (CheckSub == null) return;
                    var SubData = CheckSub.Data.Children[0].ChildData;
                    if (ChannelTracker.ContainsKey(Channel.Id)) return;
                    var ChannelData = ChannelTracker[Channel.Id];
                    if (ChannelData.Item1.Contains(SubData.Id)) return;


                    ChannelData.Item1.Add(SubData.Id);
                }
            }
        }

        async Task<RedditModel> SubredditAsync(string SubredditName)
        {
            var Get = await HttpClient.GetAsync($"https://reddit.com/r/{SubredditName}/new.json?limit=5").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) return null;
            return JsonConvert.DeserializeObject<RedditModel>(await Get.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}