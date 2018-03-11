using Discord;
using System;
using System.IO;
using System.Linq;
using SixLabors.Fonts;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.JsonModels;
using SixLabors.Primitives;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("Fun Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class FunModule : ValerieBase
    {
        string SavePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        string Normal = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&()*+,-./:;<=>?@[\\]^_`{|}~ ";
        string FullWidth = "０１２３４５６７８９ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯ" +
            "ＰＱＲＳＴＵＶＷＸＹＺ！＃＄％＆（）＊＋、ー。／：；〈＝〉？＠［\\］＾＿‘｛｜｝～ ";

        [Command("Slotmachine"), Summary("Want to earn quick bytes? That's how you earn some.")]
        public Task SlotMachineAsync(int Bet = 100)
        {
            var Slots = new string[] { "☄", "🔥", "👾", "🔆", "👀", "👅", "🍑" };
            var UserByte = Context.Server.Profiles[Context.User.Id];
            if (UserByte.Bytes < Bet || UserByte == null) return ReplyAsync("You do not have enough bytes.");
            var embed = new EmbedBuilder();
            int[] GetSlot = new int[]
            {
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length)
            };
            embed.AddField("Slot 1", Slots[GetSlot[0]], true);
            embed.AddField("Slot 2", Slots[GetSlot[1]], true);
            embed.AddField("Slot 3", Slots[GetSlot[2]], true);

            int win = 0;
            if (GetSlot[0] == GetSlot[1] & GetSlot[0] == GetSlot[2]) win = 10;
            else if (GetSlot[0] == GetSlot[1] || GetSlot[0] == GetSlot[2] || GetSlot[1] == GetSlot[2]) win = 2;

            if (win == 0)
            {
                UserByte.Bytes -= Bet;
                embed.Description = $"*Aww..* it seems you lost **{Bet}** bytes. 😞";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                UserByte.Bytes += Bet;
                embed.Description = $"**CONGRATS!** You won **{Bet}** bytes. 🎉";
                embed.Color = new Color(0x93ff89);
            }
            return SendEmbedAsync(embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET BYTES!")]
        public Task FlipAsync(char Side, int Bet = 100)
        {
            var User = Context.Server.Profiles[Context.User.Id];
            if (User == null || User.Bytes < Bet) return ReplyAsync($"You do not have enough bytes.");
            Side = Char.ToLower(Side);
            bool Heads = Context.Random.Next(0, 101) < 50 ? true : false;
            if ((Side == 'h' && Heads) || (Side == 't' && !Heads))
            {
                User.Bytes += Bet;
                return SaveAsync(ModuleEnums.Server, $"**CONGRATS!** You won **{Bet}** bytes. 🎉");
            }
            else if ((Side == 'h' && !Heads) || (Side == 't' && Heads))
            {
                User.Bytes -= Bet;
                return SaveAsync(ModuleEnums.Server, $"*Aww..* it seems you lost **{Bet}** bytes. 😞");
            }
            else return ReplyAsync($"Side can either be `h` or `t`.");
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
        public async Task RateAsync([Remainder] string ThingToRate)
            => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");

        [Command("XpLeaderboards"), Alias("XPL", "XpTop"), Summary("Shows top 10 users with the highest XP for this server.")]
        public async Task XpLeaderboardAsync()
        {
            if (!Context.Server.Profiles.Any())
            {
                await ReplyAsync($"{Context.Guild} leadboards is empty.");
                return;
            }
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Title: $"XP Leaderboards For {Context.Guild}");

            var Ordered = Context.Server.Profiles.OrderByDescending(x => x.Value.ChatXP).Where(y => y.Value.ChatXP != 0).Take(10).ToList();
            if (Ordered.Count > 3)
            {
                Embed.AddField($"🥇: {await StringExt.CheckUserAsync(Context, Ordered[0].Key)}", $"**Total XP:** {Ordered[0].Value.ChatXP}", true);
                Embed.AddField($"🥈: {await StringExt.CheckUserAsync(Context, Ordered[1].Key)}", $"**Total XP:** {Ordered[1].Value.ChatXP}", true);
                Embed.AddField($"🥉: {await StringExt.CheckUserAsync(Context, Ordered[2].Key)}", $"**Total XP:** {Ordered[2].Value.ChatXP}", true);
                foreach (var Rank in Ordered.Skip(3)) Embed.AddField($"{await StringExt.CheckUserAsync(Context, Rank.Key)}", $"**Total XP:** {Rank.Value.ChatXP}", true);
            }
            else foreach (var Rank in Ordered) Embed.AddField($"{await StringExt.CheckUserAsync(Context, Rank.Key)}", $"**Total XP:** {Rank.Value.ChatXP}", true);
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Rank"), Summary("Shows your or a specified user's rank.")]
        public Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            if (!Context.Server.Profiles.ContainsKey(User.Id)) return ReplyAsync($"**{User}** isn't ranked yet. 🙄");
            var UserRank = Context.Server.Profiles[User.Id].ChatXP;
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

        [Command("Lmgtfy"), Summary("Googles something for that special person who is crippled")]
        public Task LmgtfyAsync([Remainder] string Search = "How to use Lmgtfy")
            => ReplyAsync($"http://lmgtfy.com/?q={ Uri.EscapeUriString(Search) }");

        [Command("Daily"), Summary("Get your daily dose of bytes.")]
        public Task DailyAsync()
        {
            if (!Context.Server.Profiles.ContainsKey(Context.User.Id))
            {
                Context.Server.Profiles.Add(Context.User.Id, new UserProfile
                {
                    Bytes = 100,
                    DailyStreak = 0,
                    DailyReward = Time
                });
                return SaveAsync(ModuleEnums.Server, $"You recieved 100 bytes ☺");
            }
            var User = Context.Server.Profiles[Context.User.Id];
            if (!User.DailyReward.HasValue)
            {
                User.Bytes = 100;
                User.DailyReward = Time;
                return SaveAsync(ModuleEnums.Server, $"You recieved 100 bytes ☺");
            }
            var Get = User.DailyReward;
            var Passed = Time - User.DailyReward;
            var Wait = Get - Passed;
            if (Passed.Value.Days < 1) return ReplyAsync($"You need to wait **{Wait.Value.Hour}** hour(s), **{Wait.Value.Minute}** minute(s) for your next reward.");
            User.Bytes += User.DailyStreak * 100;
            User.DailyStreak++;
            User.DailyReward = Time;
            return SaveAsync(ModuleEnums.Server, $"You recieved 100 bytes ☺");
        }

        [Command("Bytes"), Summary("Shows how many bytes a user have.")]
        public Task BytesAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var GetUser = Context.Server.Profiles[User.Id];
            if (GetUser is null) return ReplyAsync($"**{User}** has no bytes 😶");
            var UserByte = IntExt.GetMemory(GetUser.Bytes);
            return ReplyAsync($"**{User}** has {UserByte.Item2} {UserByte.Item1}s ⚜");
        }

        [Command("Expand"), Summary("Converts text to full width.")]
        public Task ExpandAsync([Remainder] string Text)
            => ReplyAsync(string.Join("", Text.Select(x => Normal.Contains(x) ? x : ' ').Select(x => FullWidth[Normal.IndexOf(x)])));

        [Command("Guess"), Summary("Guess the right number to win bytes.")]
        public async Task GuessAsync()
        {
            int MinNum = Context.Random.Next(0, 50);
            int MaxNum = Context.Random.Next(50, 101);
            int RandomNum = Context.Random.Next(MinNum, MaxNum);
            await ReplyAsync($"Guess a number between {MinNum} and {MaxNum}. You have 10 seconds. GO!!!");
            var UserGuess = await ResponseWaitAsync(Timeout: TimeSpan.FromSeconds(10));
            if (RandomNum != int.Parse(UserGuess.Content))
            {
                await ReplyAsync($"Aww, it seems you guessed the wrong number. The lucky number was: {RandomNum}.");
                return;
            }
            Context.ServerHandler.MemoryUpdate(Context.Guild.Id, Context.User.Id, (int)Math.Pow(RandomNum, 3));
            await ReplyAsync($"BRAVOO! You guessed it right!! ");
        }

        [Command("Typefast"), Summary("Check how fast you can type given snippet within 7 seconds to win bytes!")]
        public async Task TypefastAsync(int Limit = 10)
        {
            var Word = new Char[Limit];
            for (int i = 0; i < Limit; i++) Word[i] = Normal[Context.Random.Next(Normal.Length - 1)];
            var Snippet = string.Join("", Word).Replace("\\", "");
            await Context.Channel.SendFileAsync(TextBitmap(Snippet));
            var Check = await ResponseWaitAsync(false, Timeout: TimeSpan.FromSeconds(7));
            if (Check == null || Check.Content != Snippet)
            {
                await ReplyAsync($"💢 How hard is it to type **{Snippet}** out?");
                return;
            }
            if (Check.Content == Snippet)
            {
                Context.ServerHandler.MemoryUpdate(Context.Guild.Id, Context.User.Id, (int)Math.Pow(Word.Length, 2));
                await ReplyAsync($"**{Context.User}, THE MAD MAN DID IT!!**");
            }
        }

        [Command("Rip"), Summary("Rip? Ripping rip ripped a user? RIP.")]
        public async Task RipAsync(IGuildUser User) => await Context.Channel.SendFileAsync(await GraveAsync(User));

        [Command("Give"), Summary("Wanna be kind and share your bytes?")]
        public Task GiveAsync(IGuildUser User, int Amount)
        {
            var Giver = Context.Server.Profiles[User.Id];
            if (Giver == null || Giver.Bytes < Amount) return ReplyAsync($"Woops, you have insufficent bytes.");
            Context.ServerHandler.MemoryUpdate(Context.Guild.Id, User.Id, Amount);
            Giver.Bytes -= Amount;
            return SaveAsync(ModuleEnums.Server, $"Thank you for being so kind.");
        }

        [Command("Clap"), Summary("Adds clap to your sentence message.")]
        public Task ClapAsync([Remainder] string Message) => ReplyAsync(Message.Replace(" ", " 👏 "));

        async Task<string> GraveAsync(IGuildUser User)
        {
            var Get = await Context.HttpClient.GetByteArrayAsync(User.GetAvatarUrl(ImageFormat.Png, 2048)).ConfigureAwait(false);
            using (var UserImage = File.Create($"{SavePath}/user.png"))
                await UserImage.WriteAsync(Get, 0, Get.Length).ConfigureAwait(false);

            using (var Grave = SixLabors.ImageSharp.Image.Load<Rgba32>($"{SavePath}/grave.png"))
            {
                var UserImage = SixLabors.ImageSharp.Image.Load<Rgba32>($"{SavePath}/user.png");
                UserImage.Mutate(x => x.Grayscale());
                Grave.Mutate(x =>
                {
                    var Font = new FontCollection().Install($"{SavePath}/Clone.ttf");
                    x.DrawImage(UserImage, 1, new Size(170, 170), new Point(150, 180));
                    x.DrawText($"{User.JoinedAt.Value.Date.Year} - {DateTime.Now.Year}", Font.CreateFont(25), Rgba32.Black, new PointF(150, 350));
                    x.DrawText($"Type F to pay respect.", Font.CreateFont(25), Rgba32.Black, new PointF(80, 380));
                });
                Grave.Save($"{SavePath}/user.png");
            }

            return $"{SavePath}/user.png";
        }

        string TextBitmap(string Text)
        {
            using (var Image = new Image<Rgba32>(500, 50))
            {
                var Font = new FontCollection().Install($"{SavePath}/Clone.ttf");
                Image.Mutate(x => x.DrawText(Text, Font.CreateFont(28), Rgba32.Plum, new PointF(10, 10)));
                Image.Save($"{SavePath}/image.png");
                return $"{SavePath}/image.png";
            }
        }

        DateTime Time => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Eastern Standard Time");
    }
}