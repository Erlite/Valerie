using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Valerie.Preconditions;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("General Commands"), RequireNSFW, RequireBotPermission(ChannelPermission.SendMessages)]
    public class NSFWModule : Base
    {
        [Command("Lewd"), Summary("Replies back with some newd stuff.")]
        public async Task LewdAsync()
        {
            try
            {
                var Parse = JToken.Parse(await Context.HttpClient.GetStringAsync("http://nekos.life/api/lewd/neko").ConfigureAwait(false));
                await ReplyAsync(string.Empty, GetEmbed(Paint.Crimson).WithImageUrl($"{Parse["neko"]}").Build());
            }
            catch { }
        }

        [Command("Boobs"), Summary("Displays a random titty picture from the Russian site.")]
        public async Task BoobsAsync()
            => await ReplyAsync($"http://media.oboobs.ru/{ await StringHelper.NsfwAsync(Context.HttpClient, Context.Random, "http://api.oboobs.ru/boobs/", 12560)}");

        [Command("Ass"), Summary("Displays a random ass picture from the Russian site.")]
        public async Task AssAsync()
            => await ReplyAsync($"http://media.obutts.ru/{ await StringHelper.NsfwAsync(Context.HttpClient, Context.Random, "http://api.obutts.ru/butts/", 5265)}");

        [Command("Rule34"), Summary("Searches rule34 for your tags.")]
        public async Task Rule34Async(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Rule34, Tags.ToList()));

        [Command("Yandere"), Summary("Searches yandere for your tags.")]
        public async Task Yanderesync(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Yandere, Tags.ToList()));

        [Command("Gelbooru"), Summary("Searches gelbooru for your tags.")]
        public async Task GelbooruAsync(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Gelbooru, Tags.ToList()));

        [Command("Danbooru"), Summary("Searches danbooru for your tags.")]
        public async Task DanbooruAsync(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Danbooru, Tags.ToList()));

        [Command("Cureninja"), Summary("Searches cureninja for your tags.")]
        public async Task CureninjaAsync(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Cureninja, Tags.ToList()));

        [Command("Konachan"), Summary("Searches konachan for your tags.")]
        public async Task KonachanAsync(params string[] Tags) => await ReplyAsync(await StringHelper.HentaiAsync(Context.HttpClient, Context.Random, NsfwType.Konachan, Tags.ToList()));

    }
}