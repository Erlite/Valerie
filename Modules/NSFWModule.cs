using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Valerie.Handlers;
using Valerie.Models;
using Valerie.Extensions;
using Valerie.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Valerie.Modules
{
    [RequireNSFW, RequireBotPermission(ChannelPermission.SendMessages)]
    public class NSFWModule : ValerieBase<ValerieContext>
    {
        readonly HttpClient Client = new HttpClient();
        readonly ConcurrentDictionary<ulong, Timer> AutoNSFW = new ConcurrentDictionary<ulong, Timer>();

        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!"), Alias("Tits")]
        public async Task BoobsAsync()
        {
            try
            {
                JToken Token = JArray.Parse(await Client.GetStringAsync($"http://api.oboobs.ru/boobs/{ new Random().Next(0, 10229) }").ConfigureAwait(false))[0];
                await ReplyAsync($"http://media.oboobs.ru/{ Token["preview"].ToString() }");
            }
            catch { }
        }

        [Command("Ass"), Summary("I can't believe you need help with this command."), Alias("Butt")]
        public async Task BumsAsync()
        {
            try
            {
                JToken Token = JArray.Parse(await Client.GetStringAsync($"http://api.obutts.ru/butts/{ new Random().Next(0, 4963) }").ConfigureAwait(false))[0];
                await ReplyAsync($"http://media.obutts.ru/{ Token["preview"].ToString() }");
            }
            catch { }
        }

        [Command("E621"), Summary("Never used this command. Don't ask me"), Remarks("E621 Kawaii")]
        public async Task E621Async(string search)
        {
            search = search?.Trim() ?? "";
            string url = await StringExtension.GetE621ImageLinkAsync(search).ConfigureAwait(false);
            if (url == null)
                await ReplyAsync(Context.User.Mention + " No results found! Try another term?");
            else
            {
                var embed = ValerieEmbed.Embed(EmbedColor.Pastel, url, ImageUrl: url);
                await ReplyAsync("", embed: embed.Build());
            }
        }

        [Command("Porn"), Summary("Uses Porn.com API to fetch videos.")]
        public async Task PornAsync([Remainder] string Search)
        {
            var Response = await Client.GetAsync($"http://api.porn.com/videos/find.json?search={Uri.EscapeDataString(Search)}").ConfigureAwait(false);
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
            var embed = ValerieEmbed.Embed(EmbedColor.Snow, Getvid.VideoThumb, Getvid.VideoTitle, Description: Getvid.VideoUrl, ThumbUrl: Getvid.VideoThumb,
                FooterText: $"Total Results: {ConvertJson.Count}");
            embed.AddField("Video Length", Getvid.duration, true);
            embed.AddField("Total Views", Getvid.views, true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Lewd"), Summary("Eh, Get yourself some lewd?")]
        public async Task LewdAsync()
        {
            JToken Token = JToken.Parse(await Client.GetStringAsync("http://nekos.life/api/lewd/neko").ConfigureAwait(false));
            await ReplyAsync(Token["neko"].ToString());
        }

        [Command("AutoNSFW"), Summary("Toggles Auto NSFW for a specific channel."), CustomUserPermission]
        public async Task AutoNSFWAsync(int TimePeriod)
        {
        }
    }
}