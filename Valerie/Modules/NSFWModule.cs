﻿using System;
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
        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!"), Alias("Tits")]
        public async Task BoobsAsync()
        {
            JToken Token = JArray.Parse(await new HttpClient().GetStringAsync($"http://api.oboobs.ru/boobs/{ new Random().Next(0, 10229) }").ConfigureAwait(false))[0];
            await ReplyAsync($"http://media.oboobs.ru/{ Token["preview"].ToString() }");
        }

        [Command("Ass"), Summary("I can't believe you need help with this command."), Alias("Butt")]
        public async Task BumsAsync()
        {
            JToken Token = JArray.Parse(await new HttpClient().GetStringAsync($"http://api.obutts.ru/butts/{ new Random().Next(0, 4963) }").ConfigureAwait(false))[0];
            await ReplyAsync($"http://media.obutts.ru/{ Token["preview"].ToString() }");
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
                var embed = Vmbed.Embed(VmbedColors.Pastel, url, ImageUrl: url);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Porn"), Summary("Uses Porn.com API to fetch videos.")]
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
