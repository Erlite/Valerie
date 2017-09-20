using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System;
using Newtonsoft.Json.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Valerie.Handlers;
using Valerie.Handlers.Config;
using Valerie.Attributes;
using Valerie.Extensions;
using System.Net.Http;

namespace Valerie.Modules
{
    [RequireAPIKeys, RequireBotPermission(ChannelPermission.SendMessages)]
    public class GoogleModule : ValerieBase<ValerieContext>
    {
        [Command("Google"), Alias("G"), Summary("Searches google for your search terms.")]
        public async Task GoogleAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            string URL = "http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png";

            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = BotConfig.Config.APIKeys.GoogleKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = BotConfig.Config.APIKeys.SearchEngineID;

            var items = RequestList.Execute().Items.Take(3);
            foreach (var result in items)
            {
                Str.AppendLine($"• **{result.Title}**\n{result.Snippet}\n{StringExtension.ShortenUrl(result.Link)}\n");
            }

            var embed = ValerieEmbed.Embed(VmbedColors.Pastel, Description: Str.ToString(), ThumbUrl: URL);

            if (string.IsNullOrWhiteSpace(Str.ToString()) || Str.ToString() == null)
                await ReplyAsync("No results found!");
            else
                await ReplyAsync("", embed: embed.Build());
        }

        [Command("GImage"), Summary("Searches google for your image and returns a random image from 50 results.")]
        public async Task GImageAsync([Remainder] string search)
        {
            var rng = new Random();
            var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(search)}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start=" +
                $"{ rng.Next(1, 10) }&fields=items%2Flink&key={BotConfig.Config.APIKeys.GoogleKey}";
            var obj = JObject.Parse(await new HttpClient().GetStringAsync(reqString));
            var items = obj["items"] as JArray;
            string image = items[0]["link"].ToString();

            if (!string.IsNullOrWhiteSpace(image))
                await ReplyAsync(image);
            else
                await ReplyAsync("No results found!");
        }

        [Command("Youtube"), Alias("Yt"), Summary("Searches the first search result from youtube.")]
        public Task YoutubeAsync([Remainder] string search) => ReplyAsync("https://www.youtube.com/watch?v=" + StringExtension.Youtube(search));

        [Command("Shorten"), Summary("Shortens a URL using Google URL Shortner.")]
        public Task ShortenAsync([Remainder] string URL) => ReplyAsync($"This is your shortened URL: {StringExtension.ShortenUrl(URL)}");

        [Command("Revav"), Summary("Performs a reverse image search for a user avatar.")]
        public Task RevavAsync(SocketGuildUser User)
            => ReplyAsync($"Reverse Image Result: " +
                $"{StringExtension.ShortenUrl($"https://images.google.com/searchbyimage?image_url={User.GetAvatarUrl()}")}");

    }
}
