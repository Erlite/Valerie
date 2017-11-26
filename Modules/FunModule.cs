using Discord;
using System;
using System.Linq;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.JsonModels;
using Newtonsoft.Json.Linq;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Fun Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class FunModule : ValerieBase
    {
        string Normal = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&()*+,-./:;<=>?@[\\]^_`{|}~ ";
        string FullWidth = "０１２３４５６７８９ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯ" +
            "ＰＱＲＳＴＵＶＷＸＹＺ！＃＄％＆（）＊＋、ー。／：；〈＝〉？＠［\\］＾＿‘｛｜｝～ ";

        [Command("Slotmachine"), Summary("Want to earn quick bytes? That's how you earn some.")]
        public Task SlotMachineAsync(int Bet = 100)
        {
            string[] Slots = new string[] { ":heart:", ":eggplant:", ":poo:", ":eyes:", ":star2:", ":peach:", ":pizza:" };
            var UserByte = Context.Server.Memory.FirstOrDefault(x => x.Id == $"{Context.User.Id}");
            if (UserByte.Byte < Bet || UserByte == null) return ReplyAsync("You don't have enough bytes for slot machine!");
            if (Bet <= 0) return ReplyAsync("Your bet is too low. :-1:");

            var embed = new EmbedBuilder();
            int[] s = new int[]
            {
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length)
            };
            embed.AddField("Slot 1", Slots[s[0]], true);
            embed.AddField("Slot 2", Slots[s[1]], true);
            embed.AddField("Slot 3", Slots[s[2]], true);

            int win = 0;
            if (s[0] == s[1] & s[0] == s[2]) win = 10;
            else if (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) win = 2;

            if (win == 0)
            {
                UserByte.Byte -= Bet;
                embed.Description = $"You lost {Bet} bytes. Your have {UserByte.Byte} bytes. Better luck next time! :weary: ";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                UserByte.Byte += Bet;
                embed.Description = $"You won {Bet} bytes :tada: Your have {UserByte.Byte} bytes.";
                embed.Color = new Color(0x93ff89);
            }
            return SendEmbedAsync(embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!")]
        public Task FlipAsync(string Side, int Bet = 100)
        {
            if (int.TryParse(Side, out int res)) return ReplyAsync("Side can either be Heads Or Tails.");
            var UserBytes = Context.Server.Memory.FirstOrDefault(x => x.Id == $"{Context.User.Id}");
            if (UserBytes.Byte < Bet || UserBytes == null) return ReplyAsync("You don't have enough bytes.");
            if (Bet <= 0) return ReplyAsync("Bet can't be lower than 0! Default bet is set to 50!");
            string[] Sides = { "Heads", "Tails" };
            var GetSide = Sides[Context.Random.Next(0, Sides.Length)];
            if (Side.ToLower() == GetSide.ToLower())
            {
                UserBytes.Byte += Bet;
                return SaveAsync(ModuleEnums.Server, $"Congratulations! You won {Bet} bytes! You have {UserBytes.Byte} bytes. 👌");
            }
            else
            {
                UserBytes.Byte -= Bet;
                return SaveAsync(ModuleEnums.Server, $"You lost {Bet} bytes! Your have {UserBytes.Byte} byte. :frowning:");
            }
        }

        [Command("Trump"), Summary("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync()
        {
            var Get = await Context.HttpClient.GetAsync("https://api.tronalddump.io/random/quote").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Using TrumpDump API was the worse trade deal, maybe ever.");
                return;
            }
            await ReplyAsync((JObject.Parse(await Get.Content.ReadAsStringAsync().ConfigureAwait(false)))["value"].ToString());

        }

        [Command("Yomama"), Summary("Gets a random Yomma Joke")]
        public async Task YommaAsync()
        {
            var Get = await Context.HttpClient.GetAsync("http://api.yomomma.info/").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Yo mama so fat she crashed Yomomma's API.");
                return;
            }
            await ReplyAsync(JObject.Parse(await Get.Content.ReadAsStringAsync().ConfigureAwait(false))["joke"].ToString());

        }

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate)
            => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");

        [Command("Show XpLeaderboards"), Alias("Showxpl", "ShowXpTop"), Summary("Shows top 10 users with the highest XP for this server.")]
        public async Task ShowXpTopAsync()
        {
            if (!Context.Server.ChatXP.Rankings.Any())
            {
                await ReplyAsync($"{Context.Guild} leadboards is empty.");
                return;
            }
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Title: $"Leaderboards For {Context.Guild}");
            foreach (var Rank in Context.Server.ChatXP.Rankings.OrderByDescending(x => x.Value).Take(10))
                Embed.AddField(await StringExt.CheckUserAsync(Context, Rank.Key), Rank.Value, true);
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Rank"), Summary("Shows your or a specified user's rank.")]
        public Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            if (!Context.Server.ChatXP.Rankings.ContainsKey(User.Id))
                return ReplyAsync($"**{User}** isn't ranked yet. 🙄");
            var UserRank = Context.Server.ChatXP.Rankings[User.Id];
            return ReplyAsync($"**{User} Stats 🔰**\n*Level:* {IntExt.GetLevel(UserRank)} | *Total XP:* {UserRank} | " +
                $"*Next Level:* {IntExt.GetXpForNextLevel(IntExt.GetLevel(UserRank))}");
        }

        [Command("Robohash"), Summary("Generates a random robot images for a specified user.")]
        public async Task RobohashAsync(IGuildUser User = null)
        {
            string[] Sets = { "?set=set1", "?set=set2", "?set=set3" };
            var GetRandom = Sets[Context.Random.Next(0, Sets.Length)];
            await ReplyAsync($"https://robohash.org/{(User ?? Context.User).Username}{GetRandom}");
        }

        [Command("AdorableAvatar"), Summary("Generates an avatar for a specified user.")]
        public async Task AdorableAvatarAsync(IGuildUser User = null)
            => await ReplyAsync($"https://api.adorable.io/avatars/500/{(User ?? Context.User).Username}.png");

        [Command("Neko"), Summary("Eh, Get yourself some Neko?")]
        public async Task NekoAsync()
    => await ReplyAsync($"{JToken.Parse(await Context.HttpClient.GetStringAsync("http://nekos.life/api/neko").ConfigureAwait(false))["neko"]}");

        [Command("Lmgtfy"), Summary("Googles something for that special person who is crippled")]
        public Task LmgtfyAsync([Remainder] string Search = "How to use Lmgtfy")
            => ReplyAsync($"http://lmgtfy.com/?q={ Uri.EscapeUriString(Search) }");

        [Command("Daily"), Summary("Get your daily dose of bytes.")]
        public Task DailyAsync()
        {
            var User = Context.Server.Memory.FirstOrDefault(x => x.Id == $"{Context.User.Id}");
            if (User == null)
            {
                Context.Server.Memory.Add(new MemoryWrapper
                {
                    Id = $"{Context.User.Id}",
                    Memory = Memory.Kilobyte,
                    DailyReward = DateTime.Now,
                    Byte = Context.Random.Next(100)
                });
                return SaveAsync(ModuleEnums.Server, $"You recieved 100 bytes ☺.");
            }
            var PassedTime = DateTime.Now.Subtract(User.DailyReward);
            var TimeLeft = User.DailyReward.Subtract(PassedTime);
            if (Math.Abs(PassedTime.TotalHours) < 24)
                return ReplyAsync($"You need to wait {TimeLeft.Hour} Hours, {TimeLeft.Minute} Minutes, {TimeLeft.Second} Seconds for your next daily reward.");
            User.Byte += 100;
            return SaveAsync(ModuleEnums.Server, $"You recieved 100 bytes ☺.");
        }

        [Command("Bytes"), Summary("Shows how many bytes a user have.")]
        public Task BytesAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var GetUser = Context.Server.Memory.FirstOrDefault(x => x.Id == $"{User.Id}");
            if (GetUser is null) return ReplyAsync($"**{User}** has no bytes 😶.");
            var UserByte = IntExt.GetMemory(GetUser.Byte);
            return ReplyAsync($"**{User}** has {UserByte.Item2} {UserByte.Item1}s ⚜.");
        }

        [Command("Expand"), Summary("Converts text to full width.")]
        public Task ExpandAsync([Remainder] string Text)
        {
            var NormalCheck = Text.Select(x => Normal.Contains(x) ? x : ' ');
            return ReplyAsync(string.Join("", NormalCheck.Select(x => FullWidth[Normal.IndexOf(x)])));
        }
    }
}