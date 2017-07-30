using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Valerie.Handlers.ConfigHandler;
using Valerie.Attributes;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireAPIKeys, RequireBotPermission(GuildPermission.SendMessages)]
    public class GoogleModule : ModuleBase
    {
        [Command("Google"), Alias("G"), Summary("Searches google for your search terms.")]
        public async Task GoogleAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            string URL = "http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png";

            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = BotDB.Config.APIKeys.GoogleKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = BotDB.Config.APIKeys.SearchEngineID;

            var items = RequestList.Execute().Items.Take(3);
            foreach (var result in items)
            {
                Str.AppendLine($"• **{result.Title}**\n{result.Snippet}\n{StringExtension.ShortenUrl(result.Link)}\n");
            }

            var embed = Vmbed.Embed(VmbedColors.Pastel, Description: Str.ToString(), ThumbUrl: URL);

            if (string.IsNullOrWhiteSpace(Str.ToString()) || Str.ToString() == null)
                await ReplyAsync("No results found!");
            else
                await ReplyAsync("", embed: embed);
        }

        [Command("GImage"), Summary("Searches google for your image and returns a random image from 50 results.")]
        public async Task GImageAsync([Remainder] string search)
        {
            using (var http = new HttpClient())
            {
                var rng = new Random();
                var reqString = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(search)}&cx=018084019232060951019%3Ahs5piey28-e&num=1&searchType=image&start=" +
                    $"{ rng.Next(1, 10) }&fields=items%2Flink&key={BotDB.Config.APIKeys.GoogleKey}";
                var obj = JObject.Parse(await http.GetStringAsync(reqString));
                var items = obj["items"] as JArray;
                string image = items[0]["link"].ToString();

                if (!string.IsNullOrWhiteSpace(image))
                    await ReplyAsync(image);
                else
                    await ReplyAsync("No results found!");
                http.Dispose();
            }
        }

        [Command("Youtube"), Alias("Yt"), Summary("Searches the first search result from youtube.")]
        public async Task YoutubeAsync([Remainder] string search)
        {
            var Link = "http://www.youtube.com/watch?v=" + StringExtension.Youtube(search);
            await ReplyAsync(Link);
        }

        [Command("Shorten"), Summary("Shortens a URL using Google URL Shortner.")]
        public async Task ShortenAsync([Remainder] string URL)
        {
            await ReplyAsync($"This is your shortened URL: {StringExtension.ShortenUrl(URL)}");
        }

        [Command("Revav"), Summary("Performs a reverse image search for a user avatar.")]
        public async Task RevavAsync(SocketGuildUser User)
        {
            await ReplyAsync(
                $"Reverse Image Result: " +
                $"{StringExtension.ShortenUrl($"https://images.google.com/searchbyimage?image_url={User.GetAvatarUrl()}")}");
        }
    }
}
