using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Tweetinvi;
using Cookie.FOAAS;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Services;
using Valerie.Handlers;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Handlers.Server.Models;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : ValerieBase<ValerieContext>
    {
        readonly HttpClient HttpClient = new HttpClient();

        [Command("Ping"), Summary("Pings Discord Gateway")]
        public Task PingAsync() => ReplyAsync($"Latency: {(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Rank"), Summary("Shows your current rank and how much Eridium is needed for next level.")]
        public Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            if (!Context.Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                return ReplyAsync($"{User.Username} isn't ranked yet! :weary:");
            }
            var UserEridium = Context.Config.EridiumHandler.UsersList.TryGetValue(User.Id, out int Eridium);
            string Reply =
                $"{User} Stats:\n" +
                $"**TOTAL ERIDIUM:** {Eridium} | **LEVEL:** {IntExtension.GetLevel(Eridium)} | " +
                $"**ERIDIUM:** {Eridium}/{IntExtension.GetEridiumForNextLevel(IntExtension.GetLevel(Eridium))}";
            return ReplyAsync(Reply);
        }

        [Command("Top"), Summary("Shows top 10 users in the Eridium list.")]
        public async Task EridiumAsync()
        {
            if (Context.Config.EridiumHandler.UsersList.Count == 0)
            {
                await ReplyAsync("There are no top users for this guild.");
                return;
            }
            var embed = ValerieEmbed.Embed(EmbedColor.Gold, Title: $"{Context.Guild.Name.ToUpper()} | Top 10 Users");
            var Eridiumlist = Context.Config.EridiumHandler.UsersList.OrderByDescending(x => x.Value).Take(10);
            foreach (var Value in Eridiumlist)
            {
                var User = await Context.Guild.GetUserAsync(Value.Key) as IGuildUser;
                string Username = null;
                if (User == null)
                    Username = "Unknown User";
                else
                    Username = User.Username;
                embed.AddField(Username, $"Eridium: {Value.Value}\nLevel: {IntExtension.GetLevel(Value.Value)}", true);
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("AFK"), Summary("Adds Or Removes you from AFK list. Actions: Add/Remove/Modify")]
        public async Task AFKAsync(CommandEnums Action = CommandEnums.Add, [Remainder] string AFKMessage = "I'm busy.")
        {
            switch (Action)
            {
                case CommandEnums.Add:
                    if (Context.Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are already in the AFK list.");
                        return;
                    }
                    Context.Config.AFKList.TryAdd(Context.User.Id, AFKMessage);
                    await ReplyAsync("You have been added to the AFK list.");
                    break;
                case CommandEnums.Remove:
                    if (!Context.Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are not in the AFK list.");
                        return;
                    }
                    Context.Config.AFKList.TryRemove(Context.User.Id, out string Value);
                    await ReplyAsync("You have been removed from the AFK list.");
                    break;
                case CommandEnums.Modify:
                    Context.Config.AFKList.TryUpdate(Context.User.Id, AFKMessage, Context.Config.AFKList[Context.User.Id]);
                    await ReplyAsync("Your AFK message have been modified.");
                    break;
            }
        }

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list.")]
        public async Task IAmAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role.Name == "everyone") return;
            if (!Context.Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} doesn't exist in guild's assignable role list.");
                return;
            }

            if (User.Roles.Contains(Role))
            {
                await ReplyAsync($"You already have **{Role.Name}** role!");
                return;
            }
            await User.AddRoleAsync(Role);
            await ReplyAsync($"You have been added to **{Role.Name}** role!");
        }

        [Command("IamNot"), Summary("Removes you from the specified role.")]
        public async Task IAmNotAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role.Name == "everyone") return;
            if (!Context.Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} doesn't exist in guild's assignable role list.");
                return;
            }

            if (!User.Roles.Contains(Role))
            {
                await ReplyAsync($"You do not have **{Role.Name}** role!");
                return;
            }

            await User.RemoveRoleAsync(Role);
            await ReplyAsync($"You have been removed from **{Role.Name}** role!");
        }

        [Command("Slotmachine"), Summary("Want to earn quick Eridium? That's how you earn some.")]
        public async Task SlotMachineAsync(int Bet = 100)
        {
            string[] Slots = new string[]
            {
                ":heart:",
                ":eggplant:",
                ":poo:",
                ":eyes:",
                ":star2:",
                ":peach:",
                ":pizza:"
            };
            var Rand = new Random(DateTime.Now.Millisecond);
            var UserEridium = Context.Config.EridiumHandler.UsersList[Context.User.Id];

            if (UserEridium <= 0 || UserEridium < Bet)
            {
                await ReplyAsync("You don't have enough Eridium for slot machine!");
                return;
            }

            if (Bet <= 0)
            {
                await ReplyAsync("Bet is too low. :-1:");
                return;
            }

            var embed = new EmbedBuilder();

            int[] s = new int[]
            {
                Rand.Next(0, Slots.Length),
                Rand.Next(0, Slots.Length),
                Rand.Next(0, Slots.Length)
            };
            embed.AddField(x =>
            {
                x.Name = "Slot 1";
                x.Value = Slots[s[0]];
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Slot 2";
                x.Value = Slots[s[1]];
                x.IsInline = true;
            });

            embed.AddField(x =>
            {
                x.Name = "Slot 3";
                x.Value = Slots[s[2]];
                x.IsInline = true;
            });

            int win = 0;
            if (s[0] == s[1] & s[0] == s[2]) win = 10;
            else if (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) win = 2;

            if (win == 0)
            {
                Context.Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, UserEridium - Bet, UserEridium);
                embed.Description = $"You lost {Bet} Eridium. Better luck next time! :weary: ";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                Context.Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, UserEridium += Bet, UserEridium);
                embed.Description = $"You won {Bet} Eridium :tada: Your current Eridium is: {UserEridium + Bet}";
                embed.Color = new Color(0x93ff89);
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!")]
        public async Task FlipAsync(string Side, int Bet = 50)
        {
            int UserEridium = Context.Config.EridiumHandler.UsersList[Context.User.Id];

            if (int.TryParse(Side, out int res))
            {
                await ReplyAsync("Pick either heads or tails.");
                return;
            }

            if (UserEridium < Bet || UserEridium <= 0)
            {
                await ReplyAsync("You don't have enough Eridium!");
                return;
            }

            if (Bet <= 0)
            {
                await ReplyAsync("Bet can't be lower than 0! Default bet is set to 50!");
                return;
            }


            string[] Sides = { "Heads", "Tails" };
            var GetSide = Sides[new Random().Next(0, Sides.Length)];

            if (Side.ToLower() == GetSide.ToLower())
            {
                Context.Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, UserEridium += Bet, UserEridium);
                await ReplyAsync($"Congratulations! You won {Bet} Eridium!");
            }
            else
            {
                Context.Config.EridiumHandler.UsersList.TryUpdate(Context.User.Id, UserEridium -= Bet, UserEridium);
                await ReplyAsync($"You lost {Bet} Eridium! :frowning:");
            }
        }

        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = ValerieEmbed.Embed(EmbedColor.Cyan, Title: $"INFORMATION | {Context.Guild.Name}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");

            embed.AddField("ID", Context.Guild.Id, true);
            embed.AddField("Owner", await Context.Guild.GetOwnerAsync(), true);
            embed.AddField("Default Channel", await Context.Guild.GetDefaultChannelAsync(), true);
            embed.AddField("Voice Region", Context.Guild.VoiceRegionId, true);
            embed.AddField("Created At", Context.Guild.CreatedAt, true);
            embed.AddField("Roles", $"{Context.Guild.Roles.Count }\n{string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position))}", true);
            embed.AddField("Users", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false), true);
            embed.AddField("Bots", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true), true);
            embed.AddField("Text Channels", (Context.Guild as SocketGuild).TextChannels.Count, true);
            embed.AddField("Voice Channels", (Context.Guild as SocketGuild).VoiceChannels.Count, true);
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
            List<IRole> Roles = new List<IRole>();
            foreach (var role in User.RoleIds)
            {
                var Role = Context.Guild.GetRole(role);
                Roles.Add(Role);
            }
            embed.AddField("Username", User.Username, true);
            embed.AddField("ID", User.Id, true);
            embed.AddField("Muted?", User.IsMuted ? "Yes" : "No", true);
            embed.AddField("Is Bot?", User.IsBot ? "Yes" : "No", true);
            embed.AddField("Creation Date", User.CreatedAt, true);
            embed.AddField("Join Date", User.JoinedAt, true);
            embed.AddField("Status", User.Status, true);
            embed.AddField("Permissions", string.Join(", ", User.GuildPermissions.ToList()), true);
            embed.AddField("Roles", string.Join(", ", Roles.OrderByDescending(x => x.Position)), true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate)
            => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a {new Random().Next(11)}/10");

        [Command("Translate"), Summary("Translates a sentence into the specified language.")]
        public async Task TranslateAsync(string Language, [Remainder] string Text)
        {
            var result = await Misc.Translate(Language, Text);
            string Description = $"**Input:** {Text}\n" +
                $"**In {Language}:** {result.Translations[0].Translation}";
            await ReplyAsync(Description);
        }

        [Command("Trump"), Summary("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync()
        {
            var Get = await HttpClient.GetAsync("https://api.tronalddump.io/random/quote").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Using TrumpDump API was the worse trade deal, maybe ever.");
                return;
            }
            await ReplyAsync((JObject.Parse(await Get.Content.ReadAsStringAsync().ConfigureAwait(false)))["value"].ToString());
            Get.Dispose();
        }

        [Command("Avatar"), Summary("Shows users avatar in higher resolution.")]
        public Task UserAvatarAsync(SocketGuildUser User) => ReplyAsync(User.GetAvatarUrl(size: 2048));

        [Command("Yomama"), Summary("Gets a random Yomma Joke")]
        public async Task YommaAsync()
        {
            var Get = await HttpClient.GetAsync("http://api.yomomma.info/").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Yo mama so fat she crashed Yomomma's API.");
                return;
            }
            await ReplyAsync(JObject.Parse(await Get.Content.ReadAsStringAsync().ConfigureAwait(false))["joke"].ToString());
            Get.Dispose();
        }

        [Command("Probe"), Summary("Probes someone or yourself.")]
        public Task ProbeAsync(SocketGuildUser User = null)
        {
            SocketGuildUser GetUser = null;
            if (User != null)
            {
                GetUser = User;
                return ReplyAsync($"**Probes {GetUser.Username} anus with a massive black dildo** :eggplant:\nYou dirty lilttle slut.");
            }
            else
            {
                GetUser = Context.User as SocketGuildUser;
                return
                    ReplyAsync($"**Probes {GetUser.Username} anus with a massive black dildo** :eggplant:\nYou dumb cunt! Don't know how to use a fucking command?!");
            }
        }

        [Command("Discrim"), Summary("Gets all users who match a certain discrim")]
        public async Task DiscrimAsync(IGuildUser User)
        {
            var MatchList = new List<string>();
            foreach (var Guilds in (Context.Client as DiscordSocketClient).Guilds)
            {
                var Get = Guilds.Users.Where(x => x.Discriminator == User.Discriminator && x.Id != User.Id);
                foreach (var Value in Get)
                {
                    if (!MatchList.Contains(Value.Username))
                        MatchList.Add(Value.Username);
                }
            }
            string Msg = !MatchList.Any() ? "Couldn't find anything!"
                : $"Users Matching {User}'s Discrim: {string.Join(", ", MatchList)}";
            await ReplyAsync(Msg);
        }

        [Command("Stats"), Alias("About", "Info"), Summary("Shows information about Bot.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            var HClient = new HttpClient();
            string Changes = null;
            int Eridium, Cases, Stars = 0;

            HClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            using (var Response = await HClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits"))
            {
                if (!Response.IsSuccessStatusCode)
                    Changes = "There was an error fetching the latest changes.";
                else
                {
                    dynamic Result = JArray.Parse(await Response.Content.ReadAsStringAsync());
                    Changes =
                        $"[{((string)Result[0].sha).Substring(0, 7)}]({Result[0].html_url}) {Result[0].commit.message}\n" +
                        $"[{((string)Result[1].sha).Substring(0, 7)}]({Result[1].html_url}) {Result[1].commit.message}\n" +
                        $"[{((string)Result[2].sha).Substring(0, 7)}]({Result[2].html_url}) {Result[2].commit.message}";
                }
                Response.Dispose();
            }
            var embed = ValerieEmbed.Embed(EmbedColor.Snow, Client.CurrentUser.GetAvatarUrl(), $"{Client.CurrentUser.Username}'s Official Invite",
                $"https://discordapp.com/oauth2/authorize?client_id={Client.CurrentUser.Id}&scope=bot&permissions=2146958591",
                Description: Changes, Title: "Latest Changes");

            using (var Session = MainHandler.Store.OpenSession())
            {
                var Query = Session.Query<ServerModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
                Eridium = Query.Sum(x => x.EridiumHandler.UsersList.Sum(y => y.Value));
                Cases = Query.Sum(x => x.ModLog.Cases);
                Stars = Query.Sum(x => x.Starboard.StarboardMessages.Sum(y => y.Stars));
                Session.Dispose();
            }

            embed.AddField("Members",
                    $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                    $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                    $"Total: {Client.Guilds.Sum(x => x.Users.Count)}", true);
            embed.AddField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}", true);
            embed.AddField("Guilds", $"{Client.Guilds.Count}\n[Support Guild](https://discord.gg/nzYTzxD)", true);
            embed.AddField(":space_invader:",
                $"Commands Ran: {Context.ValerieConfig.CommandsUsed}\n" +
                $"Messages Received: {Context.ValerieConfig.MessagesReceived.ToString("#,##0,,M", CultureInfo.InvariantCulture)}", true);
            embed.AddField(":hammer_pick:",
                $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB\n" +
                $"Written by: [Yucked](https://github.com/Yucked)\nDiscord.Net {DiscordConfig.Version}", true);
            embed.AddField("Global Stats", $"Eridium Given: {Eridium}\n" +
                $"Mod Cases: {Cases}", true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Fuck"), Summary("Gives a random fuck about your useless fucking command.")]
        public async Task FoaasAsync(IGuildUser User = null)
        {
            var Guild = Context.Guild as SocketGuild;
            string RandUser = Guild.Users.ToArray()[new Random(Guid.NewGuid().GetHashCode()).Next(0, Guild.Users.Count)].Username;
            await ReplyAsync(await FOAAS.RandomAsync(From: Context.User.Username, Name: User != null ? User.Username : RandUser).ConfigureAwait(false));
        }

        [Command("Tweet"), Summary("Tweets from @Vuxey account!"), RequireSchmeckles(6)]
        public async Task TweetAsync([Remainder] string TweetMessage)
        {
            if (TweetMessage.Length >= 120 || TweetMessage.Length <= 25)
            {
                await ReplyAsync("Tweet can't be longer than 120 characters and can't be shorter than 25 characters!");
                return;
            }

            var Filter = StringExtension.Censor(TweetMessage);
            var Publish = $"{Filter} - {Context.User.Username}";
            if (Publish.Length > 140)
            {
                await ReplyAsync("Tweet's total length is greater than 140!");
                return;
            }

            var UserTweet = Tweet.PublishTweet(Filter);
            string ThumbImage = null;

            if (!string.IsNullOrWhiteSpace(User.GetAuthenticatedUser().ProfileImageUrlFullSize))
                ThumbImage = User.GetAuthenticatedUser().ProfileImageUrlFullSize;
            else
                ThumbImage = Context.Client.CurrentUser.GetAvatarUrl();

            var embed = ValerieEmbed.Embed(EmbedColor.Green, Description:
                $"**Tweet:** {TweetMessage}\n" +
                $"**Tweet ID:** {UserTweet.Id}\n" +
                $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})");
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Reply"), Summary("Replies back to a tweet!"), RequireSchmeckles(4)]
        public async Task TweetReplyAsync(long ID, [Remainder] string TweetMessage)
        {
            var ReplyTo = Tweet.GetTweet(ID);

            if (ReplyTo.IsTweetPublished)
            {
                if (TweetMessage.Length >= 120 || TweetMessage.Length <= 25)
                {
                    await ReplyAsync("Tweet can't be longer than 120 characters and can't be shorter than 25 characters!");
                    return;
                }

                var Filter = StringExtension.Censor(TweetMessage);
                var Publish = $"{Filter} - {Context.User.Username}";
                if (Publish.Length > 140)
                {
                    await ReplyAsync("Tweet's total length is greater than 140!");
                    return;
                }

                var UserTweet = Tweet.PublishTweetInReplyTo(Publish, ReplyTo);
                string ThumbImage = null;

                if (!string.IsNullOrWhiteSpace(User.GetAuthenticatedUser().ProfileImageUrlFullSize))
                    ThumbImage = User.GetAuthenticatedUser().ProfileImageUrlFullSize;
                else
                    ThumbImage = Context.Client.CurrentUser.GetAvatarUrl();

                var embed = ValerieEmbed.Embed(EmbedColor.Green, Description:
                    $"**Tweet:** {TweetMessage}\n" +
                    $"**Tweet ID:** {UserTweet.Id}\n" +
                    $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})");
                await ReplyAsync("", embed: embed.Build());
            }
        }

        [Command("DeleteTweet"), Summary("Deletes a specified tweet!"), RequireSchmeckles(1)]
        public async Task DeleteTweetAsync(long ID)
        {
            var GetTweet = Tweet.GetTweet(ID);
            if (GetTweet.IsTweetPublished)
            {
                var Success = Tweet.DestroyTweet(GetTweet);
                await ReplyAsync($"Tweet with {ID} ID has been deleted!");
            }
        }

        [Command("Schmeckles"), Summary("Shows how many Schmeckles you have.")]
        public Task SchmecklesAsync()
        {
            if (!Context.Config.EridiumHandler.UsersList.ContainsKey(Context.User.Id))
            {
                return ReplyAsync("Woopsie. Couldn't find you in Eridium leaderboards.");
            }
            return ReplyAsync($"You have {IntExtension.ConvertToSchmeckles(Context.Config.EridiumHandler.UsersList[Context.User.Id])} Schmeckles.");
        }

        [Command("Todo"), Summary("Creates a new Todo.")]
        public Task TodoAsync([Remainder] string TodoMessage)
        {
            if (!Context.Config.ToDo.ContainsKey(Context.User.Id))
            {
                var TodoList = new ConcurrentDictionary<int, string>();
                TodoList.TryAdd(TodoList.Count, TodoMessage);
                Context.Config.ToDo.TryAdd(Context.User.Id, TodoList);
                return ReplyAsync("Task added successfully. :v:");
            }
            Context.Config.ToDo.TryGetValue(Context.User.Id, out ConcurrentDictionary<int, string> CurrentTasks);
            if (CurrentTasks.Count == 5)
                return ReplyAsync("You have reached your max number of tasks.");
            var NewTasks = new ConcurrentDictionary<int, string>(CurrentTasks);
            NewTasks.TryAdd(NewTasks.Count, TodoMessage);
            Context.Config.ToDo.TryUpdate(Context.User.Id, NewTasks, CurrentTasks);
            return ReplyAsync("Task created successfully. :v:");
        }

        [Command("Todos"), Summary("Shows all of your Todo's.")]
        public Task TodoAsync()
        {
            Context.Config.ToDo.TryGetValue(Context.User.Id, out ConcurrentDictionary<int, string> TodoList);
            if (!Context.Config.ToDo.Any() || !Context.Config.ToDo.ContainsKey(Context.User.Id) || !TodoList.Any())
                return ReplyAsync("Woopsie, couldn't find any Todo's.");
            var Sb = new StringBuilder();
            foreach (var Item in TodoList)
                Sb.AppendLine($"**{Item.Key}:** {Item.Value}");
            return ReplyAsync($"**Here are your current tasks:**\n{Sb.ToString()}");
        }

        [Command("TodoRemove"), Summary("Removes a task from your todo list."), Alias("TDR")]
        public Task TodoAsync(int TaskNumber)
        {
            if (!Context.Config.ToDo.Any() || !Context.Config.ToDo.ContainsKey(Context.User.Id)
                || !Context.Config.ToDo[Context.User.Id].Any() || !Context.Config.ToDo[Context.User.Id].ContainsKey(TaskNumber))
                return ReplyAsync($"Woopsie, couldn't find todo # {TaskNumber}.");
            Context.Config.ToDo.TryGetValue(Context.User.Id, out ConcurrentDictionary<int, string> CurrentTasks);
            var NewTasks = new ConcurrentDictionary<int, string>(CurrentTasks);
            NewTasks.TryRemove(TaskNumber, out string NotNeededValue);
            if (NewTasks.Count == 0)
                Context.Config.ToDo.Remove(Context.User.Id, out ConcurrentDictionary<int, string> Useless);
            else
                Context.Config.ToDo.TryUpdate(Context.User.Id, NewTasks, CurrentTasks);
            return ReplyAsync($"Task {TaskNumber} has been removed.");
        }

        [Command("Feedback"), Summary("Give Feedback on Valerie's Performance.")]
        public async Task FeedbackAsync([Remainder]string Message)
        {
            if (Message.Length < 20) { await ReplyAsync("Please enter a detailed feedback."); return; }
            var ReportChannel = await Context.Client.GetChannelAsync(Convert.ToUInt64(Context.ValerieConfig.ReportChannel)) as ITextChannel;
            string Content =
                $"**User:** {Context.User.Username} ({Context.User.Id})\n" +
                $"**Server:** {Context.Guild} ({Context.Guild.Id})\n" +
                $"**Feedback:** {Message}";
            await ReportChannel.SendMessageAsync(Content);
            await ReplyAsync("Thank you for sumbitting your feedback. :v:");
        }

        [Command("Invite"), Summary("Invite link for Valerie.")]
        public Task InviteAsync() => 
            ReplyAsync($"Here is my invite link: https://discordapp.com/oauth2/authorize?client_id=261561347966238721&scope=bot&permissions=2146958591");

        [Command("Potd"), Summary("Retrives picture of the day from NASA.")]
        public async Task PotdAsync()
        {
            var Get = await HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTD>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Cyan, Title: Content.Title, Description: Content.Explanation, ImageUrl: Content.Hdurl).Build());
        }

        [Command("Potd"), Summary("Retrives picture of the day from NASA with a specific date.")]
        public async Task PotdAsync(int Year, int Month, int Day)
        {
            var Get = await HttpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key=DEMO_KEY&date={Year}-{Month}-{Day}").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("There was an error getting picture of the day from NASA.");
                return;
            }
            var Content = JsonConvert.DeserializeObject<POTD>(await Get.Content.ReadAsStringAsync());
            await ReplyAsync("", embed: ValerieEmbed.Embed(EmbedColor.Cyan, Title: Content.Title, Description: Content.Explanation, ImageUrl: Content.Hdurl).Build());
        }
    }
}