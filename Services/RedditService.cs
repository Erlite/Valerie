using System;
using System.Linq;
using Valerie.Handlers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading;
using Valerie.Extensions;
using Valerie.JsonModels;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Services
{
    public class RedditService
    {
        HttpClient HttpClient { get; }
        ServerHandler ServerHandler { get; }
        ConfigHandler ConfigHandler { get; }
        Dictionary<ulong, Timer> ChannelsTimers { get; set; } = new Dictionary<ulong, Timer>();
        Dictionary<ulong, List<string>> PostTrack { get; set; } = new Dictionary<ulong, List<string>>();

        public RedditService(ServerHandler ServerParam, ConfigHandler ConfigParam, HttpClient HttpParam)
        {
            HttpClient = HttpParam;
            ConfigHandler = ConfigParam;
            ServerHandler = ServerParam;
        }

        public void Start(SocketGuild Guild)
        {
            var Server = ServerHandler.GetServer(Guild.Id);
            var SubChannel = Guild.GetTextChannel(Convert.ToUInt64(Server.Reddit.TextChannel));
            if (SubChannel == null || !Server.Reddit.Subreddits.Any() || ChannelsTimers.ContainsKey(SubChannel.Id)) return;
            ChannelsTimers.Add(SubChannel.Id, (new Timer(async _ =>
            {
                foreach (var Subreddit in Server.Reddit.Subreddits)
                {
                    var SubIds = new List<string>();
                    var Sub = (await GetSubredditAsync(Subreddit).ConfigureAwait(false)).Data.Children[0].ChildData;

                    if (PostTrack.ContainsKey(SubChannel.Id)) PostTrack.TryGetValue(SubChannel.Id, out SubIds);
                    if (SubIds.Contains(Sub.Id)) return;
                    string Description = Sub.Selftext.Length > 1990 ? $"{Sub.Selftext.Substring(0, 1500)}...." : Sub.Selftext;
                    string Title = Sub.Title.Length > 40 ? $"{Sub.Title.Substring(0, 35)}...." : Sub.Title;
                    string Image = await GetImgurLinkAsync(Sub.Url).ConfigureAwait(false);
                    var Embed = ValerieEmbed.Embed(EmbedColor.Random, AuthorName: $"[r/{Sub.Subreddit}]  Poster: {Sub.Author}",
                        AuthorUrl: Image, AuthorIcon: "https://i.imgur.com/IgrRtnE.png", Title: Title, Description: Description ?? null,
                    ImageUrl: Image, FooterText: $"Posted At {DateExt.UnixDT(Sub.Created)}");
                    await SubChannel.SendMessageAsync(string.Empty, embed: Embed.Build());
                    SubIds.Add(Sub.Id);
                    PostTrack.Remove(SubChannel.Id);
                    PostTrack.Add(SubChannel.Id, SubIds);
                }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))));
        }

        public void Stop(string ChannelId)
        {
            var Id = Convert.ToUInt64(ChannelId);
            if (!ChannelsTimers.ContainsKey(Id)) return;
            var GetTimer = ChannelsTimers[Id];
            GetTimer.Change(Timeout.Infinite, Timeout.Infinite);
            GetTimer.Dispose();
            ChannelsTimers.Remove(Id);
        }

        public async Task<RedditModel> GetSubredditAsync(string Subreddit)
            => JsonConvert.DeserializeObject<RedditModel>(await HttpClient.GetStringAsync($"https://reddit.com/r/{Subreddit}/new.json?limit=5").ConfigureAwait(false));

        public async Task<bool> VerifySubredditAsync(string Subreddit)
        {
            var Get = await HttpClient.GetAsync($"https://reddit.com/r/{Subreddit}").ConfigureAwait(false);
            return Get.IsSuccessStatusCode;
        }

        async Task<string> GetImgurLinkAsync(string Link)
        {
            if (!Link.Contains("https://imgur.com/")) return Link;
            var GetLink = Link.Split('/', StringSplitOptions.None);
            bool IsAlbum = false;
            string Url = null;
            if (Link.Contains("/a/"))
            {
                Url = $"https://api.imgur.com/3/album/{GetLink[4]}?client_id={ConfigHandler.Config.ApplicationKeys.ImgurKey}";
                IsAlbum = true;
            }
            else
            {
                Url = $"https://api.imgur.com/3/image/{GetLink[3]}?client_id={ConfigHandler.Config.ApplicationKeys.ImgurKey}";
                IsAlbum = false;
            }
            try
            {
                var Get = await HttpClient.GetStringAsync(Url).ConfigureAwait(false);
                var Convert = JsonConvert.DeserializeObject<ImgurModel>(Get);
                if (!Convert.Success || Convert.Status == 401) return Link;
                return IsAlbum ? Convert.Data.Images[0].Link : Convert.Data.Link;
            }
            catch
            {
                return Link;
            }
        }
    }
}