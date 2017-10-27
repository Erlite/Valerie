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
                JToken Token = JArray.Parse(await Client.GetStringAsync($"http://api.oboobs.ru/boobs/{ Context.Random.Next(0, 10229) }").ConfigureAwait(false))[0];
                await ReplyAsync($"http://media.oboobs.ru/{ Token["preview"].ToString() }");
            }
            catch { }
        }

        [Command("Ass"), Summary("I can't believe you need help with this command."), Alias("Butt")]
        public async Task BumsAsync()
        {
            try
            {
                JToken Token = JArray.Parse(await Client.GetStringAsync($"http://api.obutts.ru/butts/{ Context.Random.Next(0, 4963) }").ConfigureAwait(false))[0];
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
            var Getvid = ConvertJson.VideoModel[Context.Random.Next(0, ConvertJson.Count)];
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
            Timer _Timer;
            if (TimePeriod == 0)
            {
                AutoNSFW.TryRemove(Context.Channel.Id, out _Timer);
                if (_Timer != null)
                    _Timer.Change(Timeout.Infinite, Timeout.Infinite);
                await ReplyAsync("AutoNSFW has been stopped.");
                return;
            }
            if (TimePeriod < 30) return;

            _Timer = new Timer(async _ =>
            {
                var Rand = Context.Random.Next(0, 2);
                switch (Rand)
                {
                    case 0: await BumsAsync(); break;
                    case 1: await BoobsAsync(); break;
                }
            }, null, TimePeriod * 1000, TimePeriod * 1000);

            AutoNSFW.AddOrUpdate(Context.Channel.Id, _Timer, (Key, Old) =>
            {
                Old.Change(Timeout.Infinite, Timeout.Infinite);
                return _Timer;
            });
            await ReplyAsync($"Auto NSFW has been enabled for {(Context.Channel as ITextChannel).Mention}.");
        }
    }
}