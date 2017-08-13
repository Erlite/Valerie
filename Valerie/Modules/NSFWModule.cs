using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Valerie.Models;
using Valerie.Extensions;
using Valerie.Attributes;

namespace Valerie.Modules
{
    [RequireNSFW, RequireBotPermission(ChannelPermission.SendMessages)]
    public class NSFWModule : CommandBase
    {
        [Command("Boobs", RunMode = RunMode.Async), Summary("Oh my, you naughty lilttle boiii!"), Alias("Tits")]
        public async Task BoobsAsync()
        {
            try
            {
                JToken obj;
                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.oboobs.ru/boobs/{ new Random().Next(0, 10229) }"))[0];
                }
                await Context.Channel.SendMessageAsync($"http://media.oboobs.ru/{ obj["preview"].ToString() }");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("Ass", RunMode = RunMode.Async), Summary("I can't believe you need help with this command."), Alias("Butt")]
        public async Task BumsAsync()
        {
            try
            {
                JToken obj;
                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"http://api.obutts.ru/butts/{ new Random().Next(0, 4222) }"))[0];
                }
                await Context.Channel.SendMessageAsync($"http://media.obutts.ru/{ obj["preview"].ToString() }");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("E621", RunMode = RunMode.Async), Summary("Never used this command. Don't ask me"), Remarks("E621 Kawaii")]
        public async Task E621Async(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                await ReplyAsync("Please provide me a search term!");
                return;
            }
            search = search?.Trim() ?? "";
            string url = await StringExtension.GetE621ImageLinkAsync(search);
            if (url == null)
                await ReplyAsync(Context.User.Mention + " No results found! Try another term?");
            else
            {
                var embed = Vmbed.Embed(VmbedColors.Pastel, url, $"{Context.User.Username} searched for {search}", ImageUrl: url);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Porn", RunMode = RunMode.Async), Summary("Uses Porn.com API to fetch videos.")]
        public async Task PornAsync([Remainder] string Search)
        {
            var Response = await new HttpClient().GetAsync($"http://api.porn.com/videos/find.json?search={Uri.EscapeDataString(Search)}");
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertJson = JsonConvert.DeserializeObject<Porn>(await Response.Content.ReadAsStringAsync());
            if (!ConvertJson.IsSuccess)
            {
                await ReplyAsync("No results found!");
                return;
            }
            var Getvid = ConvertJson.VideoModel[new Random().Next(0, 20)];
            var embed = Vmbed.Embed(VmbedColors.Snow, Getvid.VideoThumb, Getvid.VideoTitle, Description: Getvid.VideoUrl, ThumbUrl: Getvid.VideoThumb,
                FooterText: $"Total Results: {ConvertJson.Count}");
            embed.AddInlineField("Video Length", Getvid.duration);
            embed.AddInlineField("Total Views", Getvid.views);
            await ReplyAsync("", embed: embed);
        }
    }
}
