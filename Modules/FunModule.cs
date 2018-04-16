using System;
using Discord;
using Valerie.Enums;
using Valerie.Addons;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Valerie.Modules
{
    public class FunModule : Base
    {
        [Command("Clap"), Summary("Replaces spaces in your message with a clap emoji.")]
        public Task ClapAsync([Remainder] string Message) => ReplyAsync(Message.Replace(" ", " 👏 "));

        [Command("AdorableAvatar"), Summary("Generates an avatar for a specified user.")]
        public Task AdorableAvatarAsync(IGuildUser User = null) => ReplyAsync($"https://api.adorable.io/avatars/500/{(User ?? Context.User).Username}.png");

        [Command("Lmgtfy"), Summary("Googles something for that special person who is crippled")]
        public Task LmgtfyAsync([Remainder] string Search = "How to use Lmgtfy") => ReplyAsync($"http://lmgtfy.com/?q={ Uri.EscapeUriString(Search) }");

        [Command("Robohash"), Summary("Generates a random robot images for a specified user.")]
        public Task RobohashAsync(IGuildUser User = null)
        {
            string[] Sets = { "?set=set1", "?set=set2", "?set=set3" };
            return ReplyAsync($"https://robohash.org/{(User ?? Context.User).Username}{Sets[Context.Random.Next(0, Sets.Length)]}");
        }

        [Command("Neko"), Summary("Eh, Get yourself some Neko?")]
        public async Task NekoAsync()
            => await ReplyAsync($"{JToken.Parse(await Context.HttpClient.GetStringAsync("http://nekos.life/api/neko").ConfigureAwait(false))["neko"]}");

        [Command("Trump"), Summary("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync() =>
            await ReplyAsync($"{JObject.Parse(await Context.HttpClient.GetStringAsync("https://api.tronalddump.io/random/quote").ConfigureAwait(false))["value"]}");

        [Command("Yomama"), Summary("Gets a random Yomma Joke")]
        public async Task YommaAsync() =>
            await ReplyAsync($"{JObject.Parse(await Context.HttpClient.GetStringAsync("http://api.yomomma.info/").ConfigureAwait(false))["joke"]}");

        [Command("Dog"), Summary("Get some woof fluff good boye.")]
        public async Task DogAsync() =>
            await ReplyAsync($"https://random.dog/{await Context.HttpClient.GetStringAsync("https://random.dog/woof").ConfigureAwait(false)}");

        [Command("Cat"), Summary("MEOW! Moew meow, meoooow. Meow Meow?")]
        public async Task CatAsync() =>
            await ReplyAsync($"{JToken.Parse(await Context.HttpClient.GetStringAsync("http://random.cat/meow").ConfigureAwait(false))["file"]}");

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate) => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");

        [Command("Daily"), Summary("Your daily reward. Claim it everyday for higher streak.")]
        public Task DailyAsync()
        {
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            var OldTime = Profile.DailyReward.Value;
            var Passed = Context.MethodHelper.EasternTime - OldTime;
            var WaitTime = OldTime - Passed;
            if (Passed.Hours < 24 || Passed.Days < 1) return ReplyAsync($"Woops, you'll have to wait {WaitTime.Hour} hours, {WaitTime.Minute} minutes {Emotes.PepeSad}");
            Profile.DailyStreak++;
            Profile.Crystals += Profile.DailyStreak * 100;
            Profile.DailyReward = Context.MethodHelper.EasternTime;
            return ReplyAsync($"You've received your daily reward. {Emotes.DHeart}", Document: DocumentType.Server);
        }

    }
}