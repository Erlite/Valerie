using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Services;
using Rick.Classes;
using System.Linq;
using Rick.Attributes;
using Rick.Handlers;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using Google.Apis.YouTube.v3;

namespace Rick.Modules
{
    [CheckBlacklist]
    public class GoogleModule : ModuleBase
    {
        [Command("Google"), Summary("Google Very wow"), Remarks("Seaches google for your terms"), Alias("G")]
        public async Task GoogleAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = BotHandler.BotConfig.GoogleAPIKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = BotHandler.BotConfig.SearchEngineID;

            var items = RequestList.Execute().Items.Take(5);
            foreach (var result in items)
            {
                Str.AppendLine($"**=> {result.Title}**\n{result.Link}");
            }
            string URL = "http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png";
            var embed = EmbedService.Embed(EmbedColors.Pastle, $"Search for: {search}", Context.Client.CurrentUser.GetAvatarUrl(), null, Str.ToString(), null, null, null, URL);
            await ReplyAsync("", embed: embed);
        }

        [Command("GImage"), Summary("GImage doges"), Remarks("Searches google for your image")]
        public async Task GImageAsync([Remainder] string search)
        {
            using (var http = new HttpClient())
            {
                var rng = new Random();
                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(search)}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start={ rng.Next(1, 50) }&fields=items%2Flink&key={BotHandler.BotConfig.GoogleAPIKey}";
                var obj = JObject.Parse(await http.GetStringAsync(reqString));
                var items = obj["items"] as JArray;
                var image = items[0]["link"].ToString();
                var embed = EmbedService.Embed(EmbedColors.Yellow, $"Searched for: {search}", Context.Client.CurrentUser.GetAvatarUrl(), null, null, null, null, image);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Youtube"), Summary("Youtube SomeVideo Name"), Remarks("Searches youtube for your video")]
        public async Task YoutubeAsync([Remainder] string search)
        {
            var Service = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = BotHandler.BotConfig.GoogleAPIKey
            });
            var SearchRequest = Service.Search.List("snippet");
            SearchRequest.Q = search;
            SearchRequest.MaxResults = 1;
            SearchRequest.Type = "video";
            var SearchResponse = (await SearchRequest.ExecuteAsync()).Items.Select(x => "http://www.youtube.com/watch?v=" + x.Id.VideoId).FirstOrDefault();
            await ReplyAsync(SearchResponse);

        }
    }
}
