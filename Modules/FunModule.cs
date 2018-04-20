using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;
using System.Collections.Specialized;

namespace Valerie.Modules
{
    [Name("Fun & Games Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
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
            await ReplyAsync($"{JToken.Parse(await Context.HttpClient.GetStringAsync("http://aws.random.cat/meow").ConfigureAwait(false))["file"]}");

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate) => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");

        [Command("Expand"), Summary("Converts text to full width.")]
        public Task ExpandAsync([Remainder] string Text)
            => ReplyAsync(string.Join("", Text.Select(x => StringHelper.Normal.Contains(x) ? x : ' ').Select(x => StringHelper.FullWidth[StringHelper.Normal.IndexOf(x)])));

        [Command("Daily"), Summary("Your daily reward. Claim it everyday for higher streak.")]
        public Task DailyAsync()
        {
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (Profile.DailyReward.HasValue)
            {
                var OldTime = Profile.DailyReward.Value;
                var Passed = Context.MethodHelper.EasternTime - OldTime;
                var WaitTime = OldTime - Passed;
                if (Passed.Hours < 24 || Passed.Days < 1) return ReplyAsync($"Woops, you'll have to wait {WaitTime.Hour} hours, {WaitTime.Minute} minutes {Emotes.PepeSad}");
                Profile.DailyStreak++;
                Profile.Crystals += Profile.DailyStreak * 100;
                Profile.DailyReward = Context.MethodHelper.EasternTime;
            }
            else
            {
                Profile.DailyReward = Context.MethodHelper.EasternTime;
                Profile.Crystals += 100;
            }
            Context.GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
            return ReplyAsync($"You've received your daily reward. {Emotes.DHeart}");
        }

        [Command("Slotmachine"), Summary("Want to earn quick crystals? That's how you earn some.")]
        public Task SlotMachineAsync(int Bet = 100)
        {
            var Slots = new string[] { "☄", "🔥", "👾", "🔆", "👀", "👅", "🍑" };
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (Profile.Crystals < Bet) return ReplyAsync($"Awww shoot! You don't have enough crystals {Emotes.PepeSad}");
            var Embed = GetEmbed(Paint.Lime);
            var GetSlot = new int[]
            {
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length)
            };
            Embed.AddField("Slot 1", Slots[GetSlot[0]], true);
            Embed.AddField("Slot 2", Slots[GetSlot[1]], true);
            Embed.AddField("Slot 3", Slots[GetSlot[2]], true);

            int win = 0;
            if (GetSlot[0] == GetSlot[1] & GetSlot[0] == GetSlot[2]) win = 10;
            else if (GetSlot[0] == GetSlot[1] || GetSlot[0] == GetSlot[2] || GetSlot[1] == GetSlot[2]) win = 2;

            if (win == 0)
            {
                Profile.Crystals -= Bet;
                Embed.Description = $"*Aww..* it seems you lost **{Bet}** crystals {Emotes.PepeSad}\nYou currently have {Profile.Crystals} crystals.";
            }
            else
            {
                Profile.Crystals -= Bet;
                Embed.Description = $"**CONGRATS!** You won **{Bet}** crystals {Emotes.DWink}\nYou currently have {Profile.Crystals} crystals.";
            }
            Context.GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
            return ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET CRYSTALS!")]
        public Task FlipAsync(char Side, int Bet = 100)
        {
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (Profile.Crystals < Bet) return ReplyAsync($"Awww shoot! You don't have enough crystals {Emotes.PepeSad}");
            Side = Char.ToLower(Side);
            bool Heads = Context.Random.Next(0, 101) < 50 ? true : false;
            if ((Side == 'h' && Heads) || (Side == 't' && !Heads))
            {
                Profile.Crystals += Bet;
                return ReplyAsync($"{Emotes.DWink} You won {Bet} crystals! Currently have {Profile.Crystals} crystals.", Document: DocumentType.Server);
            }
            else if ((Side == 'h' && !Heads) || (Side == 't' && Heads))
            {
                Profile.Crystals -= Bet;
                return ReplyAsync($"{Emotes.PepeSad} You lost {Bet} crystals! Currently have {Profile.Crystals} crystals.", Document: DocumentType.Server);
            }
            else return ReplyAsync($"Side can either be `h` or `t`.");
        }

        [Command("Guess"), Summary("Guess the right number to win bytes.")]
        public async Task GuessAsync()
        {
            int MinNum = Context.Random.Next(0, 50);
            int MaxNum = Context.Random.Next(50, 101);
            int RandomNum = Context.Random.Next(MinNum, MaxNum);
            await ReplyAsync($"Guess a number between **{MinNum}** and **{MaxNum}**. You have **10** seconds. *GO!!!*");
            var UserGuess = await ResponseWaitAsync(Timeout: TimeSpan.FromSeconds(10));
            if (RandomNum != int.Parse(UserGuess.Content))
            {
                await ReplyAsync($"Aww, it seems you guessed the wrong number. The lucky number was: **{RandomNum}**.");
                return;
            }
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            Profile.Crystals += (int)Math.Pow(RandomNum, 2);
            Context.GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
            await ReplyAsync($"Ayyy, you guessed it right {Emotes.DWink}. Got {(int)Math.Pow(RandomNum, 2)} crystals.");
        }

        [Command("Leaderboards"), Summary("Shows top 10 users with the highest XP for this server.")]
        public Task LeaderboardAsync()
        {
            if (!Context.Server.Profiles.Any() || !Context.Server.ChatXP.IsEnabled) return ReplyAsync($"{Context.Guild} leadboards is empty.");
            var Embed = GetEmbed(Paint.Yellow)
                .WithTitle($"XP Leaderboards For {Context.Guild}");

            var Ordered = Context.Server.Profiles.OrderByDescending(x => x.Value.ChatXP).Where(y => y.Value.ChatXP != 0).Take(10).ToList();
            if (Ordered.Count > 3)
            {
                Embed.AddField($"🥇: {StringHelper.CheckUser(Context.Client, Ordered[0].Key)}", $"**Total XP:** {Ordered[0].Value.ChatXP}", true);
                Embed.AddField($"🥈: {StringHelper.CheckUser(Context.Client, Ordered[1].Key)}", $"**Total XP:** {Ordered[1].Value.ChatXP}", true);
                Embed.AddField($"🥉: {StringHelper.CheckUser(Context.Client, Ordered[2].Key)}", $"**Total XP:** {Ordered[2].Value.ChatXP}", true);
                foreach (var Rank in Ordered.Skip(3)) Embed.AddField($"{StringHelper.CheckUser(Context.Client, Rank.Key)}", $"**Total XP:** {Rank.Value.ChatXP}", true);
            }
            else foreach (var Rank in Ordered)
                    Embed.AddField($"{StringHelper.CheckUser(Context.Client, Rank.Key)}", $"**Total XP:** {Rank.Value.ChatXP}", true);
            return ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Profile"), Summary("Shows a users profile statistics.")]
        public Task ProfileAsync(SocketGuildUser User = null)
        {
            User = User ?? Context.User as SocketGuildUser;
            var Servers = Context.Session.Query<GuildModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            var Profiles = Servers.Select(x => x.Profiles.Where(y => y.Key == User.Id));
            var Starboard = Servers.Select(x => x.Starboard.StarboardMessages.Where(y => y.AuthorId == User.Id));
            var GuildProfile = Context.GuildHelper.GetProfile(Context.Guild.Id, User.Id);
            var Commands = GuildProfile.Commands.OrderByDescending(x => x.Value);
            string FavCommand = !GuildProfile.Commands.Any() ? $"None {Emotes.PepeSad}" : $"{Commands.FirstOrDefault().Key} ({Commands.FirstOrDefault().Value} times)";
            var Blacklisted = GuildProfile.IsBlacklisted ? Emotes.TickYes : Emotes.TickNo;
            int TotalXp = Profiles.Sum(x => x.Sum(y => y.Value.ChatXP));
            int Level = IntHelper.NextLevelXp(IntHelper.GetLevel(TotalXp));

            var Embed = GetEmbed(Paint.Magenta)
                .WithAuthor($"👾 {User.Username} Profile", User.GetAvatarUrl())
                .WithThumbnailUrl(User.GetAvatarUrl())
                .AddField("Server Stats",
                $"**Level:** {IntHelper.GetLevel(GuildProfile.ChatXP)}  ({GuildProfile.ChatXP} / {IntHelper.NextLevelXp(IntHelper.GetLevel(GuildProfile.ChatXP))})\n" +
                $"**Stars:** {Context.Server.Starboard.StarboardMessages.Where(x => x.AuthorId == User.Id).Sum(x => x.Stars)}\n" +
                $"**Crystals:** {GuildProfile.Crystals}", true)
                .AddField("Global Stats",
                $"**XP:** {TotalXp} / {Level}\n" +
                $"**Stars:** {Starboard.Sum(x => x.Sum(y => y.Stars))}\n" +
                $"**Crystals:** {Profiles.Sum(x => x.Sum(y => y.Value.Crystals))}", true)
                .AddField("Blacklisted?", Blacklisted, true)
                .AddField("Favorite Command", FavCommand, true);
            return ReplyAsync(string.Empty, Embed.Build());
        }

        [Command("Rank"), Summary("Shows user's server's rank.")]
        public Task RankAsync(SocketGuildUser User = null)
        {
            User = User ?? Context.User as SocketGuildUser;
            var Profile = Context.GuildHelper.GetProfile(Context.Guild.Id, User.Id);
            return ReplyAsync(string.Empty, GetEmbed(Paint.Yellow)
                .WithAuthor($"{User.Username}'s Server Rank", User.GetAvatarUrl())
                .WithThumbnailUrl(User.GetAvatarUrl())
                .AddField("Rank", $" {IntHelper.GetGuildRank(Context, User.Id)} / {Context.Server.Profiles.Count}", true)
                .AddField("Level", IntHelper.GetLevel(Profile.ChatXP), true)
                .AddField("Current XP", $"{Profile.ChatXP} XP", true)
                .AddField("Next Level XP", IntHelper.NextLevelXp(IntHelper.GetLevel(Profile.ChatXP)), true).Build());
        }
    }
}