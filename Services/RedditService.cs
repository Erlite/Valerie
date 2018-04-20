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
using System.Collections.Concurrent;

namespace Valerie.Services
{
    public class RedditService
    {
        HttpClient HttpClient { get; }
        GuildHandler GuildHandler { get; }
        DiscordSocketClient Client { get; }

        ConcurrentDictionary<ulong, Timer> ChannelTimers { get; set; } = new ConcurrentDictionary<ulong, Timer>();
        ConcurrentDictionary<ulong, List<string>> PostTrack { get; set; } = new ConcurrentDictionary<ulong, List<string>>();

        public RedditService(HttpClient httpClient, GuildHandler guildHandler, DiscordSocketClient client)
        {
            Client = client;
            HttpClient = httpClient;
            GuildHandler = guildHandler;
        }

        public Task Start(ulong GuildId)
        {
            var Server = GuildHandler.GetGuild(GuildId);
            if (!Server.Reddit.IsEnabled || Server.Reddit.TextChannel == 0 || !Server.Reddit.Subreddits.Any()) return Task.CompletedTask;
            var Channel = Client.GetChannel(Server.Reddit.TextChannel) as IMessageChannel;
            if (ChannelTimers.ContainsKey(Channel.Id)) return Task.CompletedTask;
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
                    PostTrack.TryRemove(Channel.Id, out List<string> Useless);
                    PostTrack.TryAdd(Channel.Id, PostIds);
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
            ChannelTimers.TryAdd(Channel.Id, ChannelTimer);
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