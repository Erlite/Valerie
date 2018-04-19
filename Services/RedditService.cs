using System;
using Discord;
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
        Timer FeedTimer { get; set; }
        HttpClient HttpClient { get; }
        DiscordSocketClient Client { get; }
        ConfigHandler ConfigHandler { get; }
        IDocumentStore DocumentStore { get; }

        Dictionary<ulong, Timer> ChannelTimers { get; set; } = new Dictionary<ulong, Timer>();
        Dictionary<ulong, List<string>> PostTrack { get; set; } = new Dictionary<ulong, List<string>>();

        public RedditService(HttpClient httpClient, ConfigHandler configHandler, IDocumentStore documentStore, DiscordSocketClient client)
        {
            Client = client;
            HttpClient = httpClient;
            ConfigHandler = configHandler;
            DocumentStore = documentStore;
        }

        public void Initialize() => FeedTimer = new Timer(_ =>
         {
             Start();
         }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));

        public void Start()
        {
            List<GuildModel> Configs;
            using (var Session = DocumentStore.OpenSession())
                Configs = Session.Query<GuildModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            foreach (var Server in Configs.Where(x => x.Reddit.IsEnabled && x.Reddit.TextChannel != 0 && x.Reddit.Subreddits.Any()))
            {
                var Channel = Client.GetChannel(Server.Reddit.TextChannel) as IMessageChannel;
                if (ChannelTimers.ContainsKey(Channel.Id)) return;
                var ChannelTimer = new Timer(async _ =>
                {
                    foreach (var Subbredit in Server.Reddit.Subreddits)
                    {
                        var PostIds = new List<string>();
                        var CheckSub = await SubredditAsync(Subbredit).ConfigureAwait(false);
                        if (CheckSub == null) return;
                        var SubData = CheckSub.Data.Children[0].ChildData;
                        if (PostTrack.ContainsKey(Channel.Id)) PostTrack.TryGetValue(Channel.Id, out PostIds);
                        if (PostIds.Contains(SubData.Id)) return;
                        string Description = SubData.Selftext.Length > 500 ? $"{SubData.Selftext.Substring(0, 400)} ..." : SubData.Selftext;
                        await Channel.SendMessageAsync($"New Post In **r/{SubData.Subreddit}** By **{SubData.Author}**\n" +
                            $"**{SubData.Title}**\n{Description}\nPost Link: {SubData.Url}").ConfigureAwait(false);
                        PostIds.Add(SubData.Id);
                        PostTrack.Remove(Channel.Id);
                        PostTrack.Add(Channel.Id, PostIds);
                    }
                }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
                ChannelTimers.Add(Channel.Id, ChannelTimer);
            }
        }

        public async Task<RedditModel> SubredditAsync(string SubredditName)
        {
            var Get = await HttpClient.GetAsync($"https://reddit.com/r/{SubredditName}/new.json?limit=5").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode) return null;
            return JsonConvert.DeserializeObject<RedditModel>(await Get.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public void Stop(ulong ChannelId)
        {
            if (!ChannelTimers.ContainsKey(ChannelId)) return;
            var GetTimer = ChannelTimers[ChannelId];
            GetTimer.Change(Timeout.Infinite, Timeout.Infinite);
            GetTimer.Dispose();
            GetTimer = null;
            ChannelTimers.Remove(ChannelId);
        }
    }
}