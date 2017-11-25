using System;
using System.Linq;
using Newtonsoft.Json;
using Valerie.Attributes;
using Discord.Commands;
using Valerie.JsonModels;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Valerie.Services;

namespace Valerie.Modules
{
    [Name("NSFW Commands"), @RequireNsfw]
    public class NSFWModule : ValerieBase
    {
        NsfwService NsfwService { get; }
        public NSFWModule(NsfwService GetService)
        {
            NsfwService = GetService;
        }

        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!")]
        public async Task BoobsAsync()
            => await ReplyAsync(await NsfwService.RuNsfwAsync("http://api.oboobs.ru/boobs/", 11272));

        [Command("Ass"), Summary("I can't believe you need help with this command.")]
        public async Task BumsAsync()
            => await ReplyAsync(await NsfwService.RuNsfwAsync("http://api.obutts.ru/butts/", 5265));

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
            var Getvid = Json.Result[Context.Random.Next(Json.Count)];
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Description: $"[{Getvid.Title}]({Getvid.Url})\nTotal Views: {Getvid.Views}", ThumbUrl: Getvid.Thumb);
            await ReplyAsync(string.Empty, embed: Embed.Build());
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