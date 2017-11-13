using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Handlers.ModuleHandler;
using Models;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Valerie.JsonModels;

namespace Valerie.Modules
{
    [Name("NSFW Commands"), @RequireNsfw]
    public class NSFWModule : ValerieBase
    {
        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!")]
        public async Task BoobsAsync()
        {
            try
            {
                JToken Token = JArray.Parse(await Context.HttpClient.GetStringAsync($"http://api.oboobs.ru/boobs/{ Context.Random.Next(0, 10229) }").ConfigureAwait(false))[0];
                await ReplyAsync($"http://media.oboobs.ru/{ Token["preview"].ToString() }");
            }
            catch { }
        }

        [Command("Ass"), Summary("I can't believe you need help with this command.")]
        public async Task BumsAsync()
        {
            try
            {
                JToken Token = JArray.Parse(await Context.HttpClient.GetStringAsync($"http://api.obutts.ru/butts/{ Context.Random.Next(0, 4963) }").ConfigureAwait(false))[0];
                await ReplyAsync($"http://media.obutts.ru/{ Token["preview"].ToString() }");
            }
            catch { }
        }

        [Command("E621"), Summary("Never used this command. Don't ask me")]
        public async Task E621Async(string Search)
        {
            //string Results = await StringExtension.GetE621ImageLinkAsync(Search).ConfigureAwait(false);
            //if (Results == null) await ReplyAsync(Context.User.Mention + " No results found! Try another term?");
        }

        [Command("Porn"), Summary("Uses Porn.com API to fetch videos.")]
        public async Task PornAsync([Remainder] string Search)
        {
            var Response = await Context.HttpClient.GetAsync($"http://api.porn.com/videos/find.json?search={Uri.EscapeDataString(Search)}").ConfigureAwait(false);
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync("No results found.");
                return;
            }
            var Json = JsonConvert.DeserializeObject<PornModel>(await Response.Content.ReadAsStringAsync());
            var Getvid = Json.Result[Context.Random.Next(Json.Result.Count())];
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Description: $"[{Getvid.Title}]({Getvid.Url})\nTotal Views: {Getvid.Views}", ThumbUrl: Getvid.Thumb);
            await SendEmbedAsync(Embed.Build());
            Response.Dispose();
        }

        [Command("Lewd"), Summary("Weeb heaven.")]
        public async Task LewdAsync()
        {
            try
            {
                JToken Token = JToken.Parse(await Context.HttpClient.GetStringAsync("http://nekos.life/api/lewd/neko").ConfigureAwait(false));
                await ReplyAsync(Token["neko"].ToString());
            }
            catch { }
        }
    }
}