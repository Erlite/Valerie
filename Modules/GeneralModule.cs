using Discord;
using System;
using System.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.JsonModels;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("General & Fun Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : ValerieBase
    {
        [Command("Ping"), Summary("Pings discord gateway.")]
        public Task PingAsync() => ReplyAsync($"{(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Avatar"), Summary("Shows users avatar in higher resolution.")]
        public Task UserAvatarAsync(IGuildUser User = null) => ReplyAsync((User ?? Context.User).GetAvatarUrl(size: 2048));

        [Command("Updoot"), Summary("Gives an updoot to a specified user.")]
        public Task UpdootAsync(IGuildUser User)
        {
            if (Context.User.Id == User.Id || User.IsBot) return ReplyAsync("Abahaha, nice try babes.");
            if (!Context.Server.Updoots.Any(x => x.Key == User.Id))
                Context.Server.Updoots.Add(User.Id, 1);
            else
                Context.Server.Updoots[User.Id]++;
            return SaveAsync(ModuleEnums.Server, $"Thanks for updooting {User}. ☺");
        }

        [Command("Updoot"), Summary("Shows your updoots in this server.")]
        public Task UpdootAsync()
        {
            if (!Context.Server.Updoots.ContainsKey(Context.User.Id))
                return ReplyAsync("No-one gave you updoots. 😶");
            return ReplyAsync($"You have {Context.Server.Updoots[Context.User.Id]} updoots. ☺");
        }

        [Command("AFK"), Summary("Adds Or Removes you from AFK list. Actions: Add/Remove/Modify")]
        public Task AFKAsync(ModuleEnums Action = ModuleEnums.Add, [Remainder] string AFKMessage = "I'm busy.")
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are already AFK.");
                    Context.Server.AFKUsers.Add(Context.User.Id, AFKMessage);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers.Remove(Context.User.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Modify:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers[Context.User.Id] = AFKMessage;
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("Slotmachine"), Summary("Want to earn quick XP? That's how you earn some.")]
        public Task SlotMachineAsync(int Bet = 100)
        {
            string[] Slots = new string[] { ":heart:", ":eggplant:", ":poo:", ":eyes:", ":star2:", ":peach:", ":pizza:" };
            var UserXP = Context.Server.ChatXP.Rankings[Context.User.Id];
            if (UserXP < Bet) return ReplyAsync("You don't have enough XP for slot machine!");
            if (Bet <= 0) return ReplyAsync("Bet is too low. :-1:");

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
                Context.Server.ChatXP.Rankings[Context.User.Id] -= Bet;
                embed.Description = $"You lost {Bet} XP. Your current XP is: {UserXP - Bet}. Better luck next time! :weary: ";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] += Bet;
                embed.Description = $"You won {Bet} XP :tada: Your current XP is: {UserXP + Bet}";
                embed.Color = new Color(0x93ff89);
            }
            return SendEmbedAsync(embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!")]
        public Task FlipAsync(string Side, int Bet = 100)
        {
            if (int.TryParse(Side, out int res)) return ReplyAsync("Side can either be Heads Or Tails.");
            int UserXP = Context.Server.ChatXP.Rankings[Context.User.Id];
            if (UserXP < Bet || UserXP <= 0) return ReplyAsync("You don't have enough XP!");
            if (Bet <= 0) return ReplyAsync("Bet can't be lower than 0! Default bet is set to 50!");
            string[] Sides = { "Heads", "Tails" };
            var GetSide = Sides[Context.Random.Next(0, Sides.Length)];
            if (Side.ToLower() == GetSide.ToLower())
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] += Bet;
                return ReplyAsync($"Congratulations! You won {Bet} XP! You have {UserXP + Bet} XP.");
            }
            else
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] -= Bet;
                return ReplyAsync($"You lost {Bet} XP! Your have {UserXP - Bet} XP left. :frowning:");
            }
        }

        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = ValerieEmbed.Embed(EmbedColor.Random, Title: $"INFORMATION | {Context.Guild.Name}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");

            embed.AddField("ID", Context.Guild.Id, true);
            embed.AddField("Owner", await Context.Guild.GetOwnerAsync(), true);
            embed.AddField("Default Channel", await Context.Guild.GetDefaultChannelAsync(), true);
            embed.AddField("Voice Region", Context.Guild.VoiceRegionId, true);
            embed.AddField("Created At", Context.Guild.CreatedAt, true);
            embed.AddField("Users", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false), true);
            embed.AddField("Bots", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true), true);
            embed.AddField("Text Channels", (Context.Guild as SocketGuild).TextChannels.Count, true);
            embed.AddField("Voice Channels", (Context.Guild as SocketGuild).VoiceChannels.Count, true);
            embed.AddField($"Roles - {Context.Guild.Roles.Count}", string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position)), true);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("RoleInfo"), Alias("RI"), Summary("Displays information about a role.")]
        public async Task RoleInfoAsync(IRole Role)
        {
            var embed = new EmbedBuilder
            {
                Color = Role.Color,
                Title = $"INFORMATION | {Role.Name}"
            };
            embed.AddField("ID", Role.Id, true);
            embed.AddField("Color", Role.Color, true);
            embed.AddField("Creation Date", Role.CreatedAt, true);
            embed.AddField("Is Hoisted?", Role.IsHoisted ? "Yes" : "No", true);
            embed.AddField("Is Mentionable?", Role.IsMentionable ? "Yes" : "No", true);
            embed.AddField("Is Managed?", Role.IsManaged ? "Yes" : "No", true);
            embed.AddField("Permissions", string.Join(", ", Role.Permissions.ToList()), true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("UserInfo"), Alias("UI"), Summary("Displays information about a user.")]
        public async Task UserInfoAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: $"INFORMATION | {User}", ThumbUrl: User.GetAvatarUrl());
            string Roles = null;
            foreach (var Role in (User as SocketGuildUser).Roles.OrderBy(x => x.Position)) Roles = string.Join(", ", Role.Name);
            embed.AddField("Username", User.Username, true);
            embed.AddField("ID", User.Id, true);
            embed.AddField("Muted?", User.IsMuted ? "Yes" : "No", true);
            embed.AddField("Is Bot?", User.IsBot ? "Yes" : "No", true);
            embed.AddField("Creation Date", User.CreatedAt, true);
            embed.AddField("Join Date", User.JoinedAt, true);
            embed.AddField("Status", User.Status, true);
            embed.AddField("Permissions", string.Join(", ", User.GuildPermissions.ToList()), true);
            embed.AddField("Roles", string.Join(", ", (User as SocketGuildUser).Roles.OrderBy(x => x.Position).Select(x => x.Name)), true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate)
            => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list.")]
        public Task IAmAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role == Context.Guild.EveryoneRole) return Task.CompletedTask;
            if (!Context.Server.AssignableRoles.Contains(Role.Id))
                return ReplyAsync($"{Role.Name} isn't an assignable role.");
            else if (User.Roles.Contains(Role))
                return ReplyAsync($"You already have **{Role.Name}** role.");
            return User.AddRoleAsync(Role);
        }

        [Command("IamNot"), Summary("Removes you from the specified role.")]
        public Task IAmNotAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role == Context.Guild.EveryoneRole) return Task.CompletedTask;
            if (User.Roles.Contains(Role)) return ReplyAsync($"You already have **{Role.Name}** role.");
            return User.AddRoleAsync(Role);
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

        [Command("Discrim"), Summary("Gets all users who match a certain user's discriminator.")]
        public async Task DiscrimAsync(IGuildUser User)
        {
            var MatchList = new Dictionary<ulong, string>();
            foreach (var Guilds in (Context.Client as DiscordSocketClient).Guilds)
            {
                var Get = Guilds.Users.Where(x => x.Discriminator == User.Discriminator && x.Id != User.Id);
                foreach (var user in Get)
                    if (!MatchList.ContainsKey(User.Id))
                        MatchList.Add(User.Id, User.Username);
            }
            string Msg = !MatchList.Any() ? $"Couldn't find any user's matching {User} discriminator.!"
                : $"**Users Matching {User}'s Discriminator:** {string.Join(", ", MatchList.Values)}";
            await ReplyAsync(Msg);
        }

        [Command("Potd"), Summary("Retrives picture of the day from NASA.")]
        public async Task PotdAsync()
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTDModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Yellow, AuthorName: $"{Content.Title} | {Content.Date}", AuthorUrl: Content.Url,
                Description: $"**Information: **{Content.Explanation}", ImageUrl: Content.Hdurl).Build());
        }

        [Command("Potd"), Summary("Retrives picture of the day from NASA with a specific date.")]
        public async Task PotdAsync(int Year, int Month, int Day)
        {
            var Get = await Context.HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={Year}-{Month}-{Day}").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTDModel>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Yellow, AuthorName: $"{Content.Title} | {Content.Date}", AuthorUrl: Content.Url,
                Description: $"**Information: **{Content.Explanation}", ImageUrl: Content.Hdurl).Build());

        }

        [Command("Feedback"), Summary("Give Feedback on Valerie's Performance.")]
        public async Task FeedbackAsync([Remainder]string Message)
        {
            if (Message.Length < 20) { await ReplyAsync("Please enter a detailed feedback."); return; }
            var ReportChannel = await Context.Client.GetChannelAsync(Convert.ToUInt64(Context.Config.ReportChannel)) as ITextChannel;
            string Content =
                $"**User:** {Context.User.Username} ({Context.User.Id})\n" +
                $"**Server:** {Context.Guild} ({Context.Guild.Id})\n" +
                $"**Feedback:** {Message}";
            await ReportChannel.SendMessageAsync(Content);
            await ReplyAsync("Thank you for sumbitting your feedback. 😊");
        }

        [Command("Show Warnings"), Alias("Sw"), Summary("Shows current number of warnings for specified user.")]
        public Task WarningsAsync(IGuildUser User = null)
        {
            User = Context.User as IGuildUser ?? User;
            if (!Context.Server.ModLog.Warnings.ContainsKey(User.Id) || !Context.Server.ModLog.Warnings.Any())
                return ReplyAsync($"{User} has no previous warnings.");
            return ReplyAsync($"{User} has been warned {Context.Server.ModLog.Warnings[User.Id]} times.");
        }

        [Command("Show Selfroles"), Alias("Ssr"), Summary("Shows a list of all assignable roles for this server.")]
        public Task ShowSelfRolesAsync()
        {
            if (!Context.Server.AssignableRoles.Any()) return ReplyAsync($"{Context.Guild} has no self roles.");
            return ReplyAsync($"**Self Assignable Roles**\n{string.Join(", ", Context.Server.AssignableRoles.Select(x => StringExt.CheckRole(Context, x)))}");
        }

        [Command("Show Case"), Summary("Shows information about a specific case.")]
        public Task CaseAsync(int CaseNumber = 0)
        {
            if (CaseNumber == 0) CaseNumber = Context.Server.ModLog.ModCases.LastOrDefault().CaseNumber;
            var Case = Context.Server.ModLog.ModCases.FirstOrDefault(x => x.CaseNumber == CaseNumber);
            if (Case == null || !Context.Server.ModLog.ModCases.Any())
                return ReplyAsync($"Case #{CaseNumber} doesn't exist.");

            return ReplyAsync(
                $"**Case Number:** {Case.CaseNumber} | **Case Type:** {Case.CaseType}\n" +
                $"**User:** {Case.UserInfo}\n" +
                $"**Responsible Mod:** {Case.ResponsibleMod}\n" +
                $"**Reason:** {Case.Reason}");
        }

        [Command("Show Stats"), Summary("Shows certain Valerie's stats.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            var Servers = await Context.ServerHandler.ServerListAsync();
            var Commits = await GitStatsAsync();
            string Description = null;
            if (!Commits.Any()) Description = "Error fetching commits.";
            foreach (var Commit in Commits.Take(3))
                Description += $"[[{Commit.Sha.Substring(0, 6)}]({Commit.HtmlUrl})] {Commit.Commit.Message}\n";

            var Embed = ValerieEmbed.Embed(EmbedColor.Red, Context.Client.CurrentUser.GetAvatarUrl(), $"{Context.Client.CurrentUser.Username} Statistics 🔰",
                Description: Description);
            Embed.AddField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}", true);
            Embed.AddField("Members",
                $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Users.Count)}", true);
            Embed.AddField("Database",
                $"Cases: {Servers.Sum(x => x.ModLog.ModCases.Count)}\n" +
                $"Tags: {Servers.Sum(x => x.Tags.Count)}\n" +
                $"Updoots: {Servers.Sum(x => x.Updoots.Count)}", true);
            Embed.AddField("Severs", $"{Client.Guilds.Count}", true);
            Embed.AddField("Memory", $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB", true);
            Embed.AddField("Programmer", $"[Yucked](https://github.com/Yucked)", true);
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Show Updates"), Summary("Shows the most recent changes made to Valerie. FYI: Some changes may not make sense.")]
        public async Task ShowUpdatesAsync()
        {
            var Commits = await GitStatsAsync();
            var Embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: "Latest Valerie Updates");
            foreach (var Commit in Commits.Take(10))
                Embed.AddField(Commit.Sha.Substring(0, 6), $"{Commit.Commit.Message}\n{Commit.HtmlUrl}");
            await ReplyAsync("", embed: Embed.Build());
        }

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

        async Task<IReadOnlyCollection<GitModel>> GitStatsAsync()
        {
            var httpClient = Context.HttpClient;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var Request = await httpClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits");
            var Content = JsonConvert.DeserializeObject<IReadOnlyCollection<GitModel>>(await Request.Content.ReadAsStringAsync());
            httpClient.DefaultRequestHeaders.Clear();
            return Content;
        }
    }
}