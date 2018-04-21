using System;
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
using System.Collections.Concurrent;

namespace Valerie.Services
{
    public class RedditService
    {
        HttpClient HttpClient { get; }
        IDocumentStore Store { get; }
        GuildHandler GuildHandler { get; }
        DiscordSocketClient Client { get; }
        Timer AutoFeedTimer { get; set; }
        WebhookService WebhookService { get; }
        ConcurrentDictionary<ulong, Timer> ChannelTimers { get; set; } = new ConcurrentDictionary<ulong, Timer>();
        ConcurrentDictionary<ulong, List<string>> PostTrack { get; set; } = new ConcurrentDictionary<ulong, List<string>>();

        public RedditService(HttpClient httpClient, GuildHandler guildHandler, DiscordSocketClient client, IDocumentStore store, WebhookService webhook)
        {
            Client = client;
            Store = store;
            HttpClient = httpClient;
            GuildHandler = guildHandler;
            WebhookService = webhook;
        }

        public Task Start(ulong GuildId)
        {
            var Server = GuildHandler.GetGuild(GuildId);
            if (!Server.Reddit.Subreddits.Any()) return Task.CompletedTask;
            if (ChannelTimers.ContainsKey(Server.Reddit.Webhook.Key)) return Task.CompletedTask;
            ChannelTimers.TryAdd(Server.Reddit.Webhook.Key, new Timer(async _ =>
            {
                foreach (var Subbredit in Server.Reddit.Subreddits)
                {
                    var PostIds = new List<string>();
                    var CheckSub = await SubredditAsync(Subbredit).ConfigureAwait(false);
                    if (CheckSub == null) return;
                    var SubData = CheckSub.Data.Children[0].ChildData;
                    if (PostTrack.ContainsKey(Server.Reddit.Webhook.Key)) PostTrack.TryGetValue(Server.Reddit.Webhook.Key, out PostIds);
                    if (PostIds.Contains(SubData.Id)) return;
                    string Description = SubData.Selftext.Length > 500 ? $"{SubData.Selftext.Substring(0, 400)} ..." : SubData.Selftext;
                    await WebhookService.SendMessageAsync(new WebhookOptions
                    {
                        Message = $"New Post In **r/{SubData.Subreddit}** By **{SubData.Author}**\n**{SubData.Title}**\n{Description}\nPost Link: {SubData.Url}",
                        Name = "Reddit Feed",
                        Setting = Enums.SettingType.RedditChannel,
                        WebhookInfo = Server.Reddit.Webhook
                    });
                    PostIds.Add(SubData.Id);
                    PostTrack.TryRemove(Server.Reddit.Webhook.Key, out List<string> Useless);
                    PostTrack.TryAdd(Server.Reddit.Webhook.Key, PostIds);
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15)));
            return Task.CompletedTask;
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
            ChannelTimers.TryRemove(ChannelId, out Timer Useless);
            Useless.Change(Timeout.Infinite, Timeout.Infinite);
            Useless.Dispose();
        }
    }
}