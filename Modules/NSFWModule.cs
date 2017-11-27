using System;
using Discord;
using System.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.JsonModels;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;
using System.Text.RegularExpressions;

namespace Valerie.Modules
{
    [Name("NSFW Commands"), RequireBotPermission(ChannelPermission.SendMessages), @RequireNsfw]
    public class NsfwModule : ValerieBase
    {
        [Command("Boobs"), Summary("Oh my, you naughty lilttle boiii!")]
        public async Task BoobsAsync()
            => await ReplyAsync($"http://media.oboobs.ru/{ await RuNsfwAsync("http://api.oboobs.ru/boobs/", 11272)}");

        [Command("Ass"), Summary("I can't believe you need help with this command.")]
        public async Task BumsAsync()
            => await ReplyAsync($"http://media.obutts.ru/{ await RuNsfwAsync("http://api.obutts.ru/butts/", 5265)}");

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

        [Command("Lewd"), Summary("Replies back with some newd stuff.")]
        public async Task LewdAsync()
        {
            try
            {
                JToken Token = JToken.Parse(await Context.HttpClient.GetStringAsync("http://nekos.life/api/lewd/neko").ConfigureAwait(false));
                await ReplyAsync(Token["neko"].ToString());
            }
            catch { }
        }

        [Command("Rule34"), Summary("Searches rule34 for your tags.")]
        public async Task Rule34Async(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Rule34, Tags.ToList()));

        [Command("Yandere"), Summary("Searches yandere for your tags.")]
        public async Task Yanderesync(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Yandere, Tags.ToList()));

        [Command("Gelbooru"), Summary("Searches gelbooru for your tags.")]
        public async Task GelbooruAsync(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Gelbooru, Tags.ToList()));

        [Command("Danbooru"), Summary("Searches danbooru for your tags.")]
        public async Task DanbooruAsync(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Danbooru, Tags.ToList()));

        [Command("Cureninja"), Summary("Searches cureninja for your tags.")]
        public async Task CureninjaAsync(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Cureninja, Tags.ToList()));

        [Command("Konachan"), Summary("Searches konachan for your tags.")]
        public async Task KonachanAsync(params string[] Tags) => await ReplyAsync(await WeebAsync(Weeb.Konachan, Tags.ToList()));

        async Task<string> RuNsfwAsync(string Url, int Max)
        {
            try
            {
                var Parse = JArray.Parse(await Context.HttpClient.GetStringAsync($"{Url}{Context.Random.Next(Max)}").ConfigureAwait(false))[0];
                return ($"{Parse["preview"]}");
            }
            catch { return null; }
        }

        async Task<string> WeebAsync(Weeb Weeb, List<string> Tags)
        {
            string Url = null;
            string Result = null;
            MatchCollection Matches = null;
            Tags = !Tags.Any() ? new[] { "boobs", "tits", "ass", "sexy", "neko" }.ToList() : Tags;
            switch (Weeb)
            {
                case Weeb.Danbooru: Url = $"http://danbooru.donmai.us/posts?page={Context.Random.Next(0, 15)}{string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case Weeb.Gelbooru: Url = $"http://gelbooru.com/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case Weeb.Rule34: Url = $"http://rule34.xxx/index.php?page=dapi&s=post&q=index&limit=100&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case Weeb.Cureninja: Url = $"https://cure.ninja/booru/api/json?f=a&o=r&s=1&q={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case Weeb.Konachan: Url = $"http://konachan.com/post?page={Context.Random.Next(0, 5)}&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
                case Weeb.Yandere: Url = $"https://yande.re/post.xml?limit=25&page={Context.Random.Next(0, 15)}&tags={string.Join("+", Tags.Select(x => x.Replace(" ", "_")))}"; break;
            }
            var Get = await Context.HttpClient.GetStringAsync(Url).ConfigureAwait(false);
            switch (Weeb)
            {
                case Weeb.Danbooru: Matches = Regex.Matches(Get, "data-large-file-url=\"(.*)\""); break;
                case Weeb.Yandere:
                case Weeb.Gelbooru:
                case Weeb.Rule34: Matches = Regex.Matches(Get, "file_url=\"(.*?)\" "); break;
                case Weeb.Cureninja: Matches = Regex.Matches(Get, "\"url\":\"(.*?)\""); break;
                case Weeb.Konachan: Matches = Regex.Matches(Get, "<a class=\"directlink smallimg\" href=\"(.*?)\""); break;
            }
            if (!Matches.Any()) return "No results found.";
            switch (Weeb)
            {
                case Weeb.Danbooru: Result = $"http://danbooru.donmai.us/{Matches[Context.Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case Weeb.Konachan:
                case Weeb.Gelbooru: Result = $"http:{Matches[Context.Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case Weeb.Yandere:
                case Weeb.Rule34: Result = $"http:{Matches[Context.Random.Next(Matches.Count)].Groups[1].Value}"; break;
                case Weeb.Cureninja: Result = Matches[Context.Random.Next(Matches.Count)].Groups[1].Value.Replace("\\/", "/"); break;
            }
            return Result;
        }

        public enum Weeb
        {
            Rule34,
            Yandere,
            Gelbooru,
            Danbooru,
            Cureninja,
            Konachan
        }
    }
}