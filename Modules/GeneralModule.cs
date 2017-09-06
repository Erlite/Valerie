﻿using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Services;
using Cookie.FOAAS;
using Valerie.Handlers.Config;
using Valerie.Handlers.Server.Models;
using Valerie.Handlers.Server;
using Valerie.Modules.Enums;
using Tweetinvi;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : CommandBase
    {
        ServerModel GuildConfig => ServerConfig.ConfigAsync(Context.Guild.Id).GetAwaiter().GetResult();
        ServerModel Config => ServerConfig.Config;

        [Command("Ping"), Summary("Pings Discord Gateway")]
        public Task PingAsync() => ReplyAsync($"Latency: {(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Rank"), Summary("Shows your current rank and how much Eridium is needed for next level.")]
        public Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                return ReplyAsync($"{User.Username} isn't ranked yet! :weary:");
            }
            var UserEridium = Config.EridiumHandler.UsersList.TryGetValue(User.Id, out int Eridium);
            string Reply =
                $"{User} Stats:\n" +
                $"**TOTAL ERIDIUM:** {Eridium} | **LEVEL:** {IntExtension.GetLevel(Eridium)} | " +
                $"**ERIDIUM:** {Eridium}/{IntExtension.GetEridiumForNextLevel(IntExtension.GetLevel(Eridium))}";
            return ReplyAsync(Reply);
        }

        [Command("Top"), Summary("Shows top 10 users in the Eridium list.")]
        public async Task EridiumAsync()
        {
            if (Config.EridiumHandler.UsersList.Count == 0)
            {
                await ReplyAsync("There are no top users for this guild.");
                return;
            }
            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"{Context.Guild.Name.ToUpper()} | Top 10 Users");
            var Eridiumlist = Config.EridiumHandler.UsersList.OrderByDescending(x => x.Value).Take(10);
            foreach (var Value in Eridiumlist)
            {
                var User = await Context.Guild.GetUserAsync(Value.Key) as IGuildUser;
                string Username, Id = null;
                if (User == null)
                {
                    Username = "Unknown User";
                    Id = $"\nID: {Value.Key}";
                }
                else
                    Username = User.Username;
                embed.AddInlineField(Username, $"Eridium: {Value.Value}\nLevel: {IntExtension.GetLevel(Value.Value)}{Id}");
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("AFK"), Summary("Adds Or Removes you from AFK list.")]
        public async Task AFKAsync(CommandEnums Action, [Remainder] string AFKMessage = "I'm busy.")
        {
            switch (Action)
            {
                case CommandEnums.Add:
                    if (Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are already in the AFK list.");
                        return;
                    }
                    Config.AFKList.TryAdd(Context.User.Id, AFKMessage);
                    await ReplyAsync("You have been added to the AFK list.");
                    break;
                case CommandEnums.Remove:
                    if (!Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are not in the AFK list.");
                        return;
                    }
                    Config.AFKList.TryRemove(Context.User.Id, out string Value);
                    await ReplyAsync("You have been removed from the AFK list.");
                    break;
                case CommandEnums.Modify:
                    Config.AFKList.TryUpdate(Context.User.Id, AFKMessage, Config.AFKList[Context.User.Id]);
                    await ReplyAsync("Your AFK message have been modified.");
                    break;
            }
        }

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list.")]
        public async Task IAmAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (!Config.AssignableRoles.Contains($"{Role.Id}"))
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
            if (!Config.AssignableRoles.Contains($"{Role.Id}"))
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
        public async Task SlotMachineAsync(int Bet = 50)
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
            var UserEridium = Config.EridiumHandler.UsersList[Context.User.Id];
            if (Config.EridiumHandler.IsEnabled == false)
            {
                await ReplyAsync("Chat Eridium is disabled! Ask Admin or server owner to enable Chat Eridium!");
                return;
            }

            if (UserEridium <= 0 || UserEridium < Bet)
            {
                await ReplyAsync("You don't have enough Eridium for slot machine!");
                return;
            }

            if (Bet <= 0)
            {
                await ReplyAsync("Bet can't be lower than 0! Default bet is set to 50!");
                return;
            }

            if (Bet > 5000)
            {
                await ReplyAsync("Bet is too high! Bet needs to be lower than 5000.");
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
                UserEridium += Bet;
                embed.Description = $"You lost {Bet}. Better luck next time! :weary:";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                UserEridium -= Bet;
                embed.Description = $"You won {Bet} :tada:";
                embed.Color = new Color(0x93ff89);
            }
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!")]
        public async Task FlipAsync(string Side, int Bet = 50)
        {
            int UserEridium = Config.EridiumHandler.UsersList[Context.User.Id];
            if (Config.EridiumHandler.IsEnabled == false)
            {
                await ReplyAsync("Chat Eridium is disabled! Ask the admin to enable ChatEridium!");
                return;
            }

            if (int.TryParse(Side, out int res))
            {
                await ReplyAsync("Side can't be a number. Use help command for more information!");
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

            if (Bet > 5000)
            {
                await ReplyAsync("Bet is too high! Bet needs to be lower than 5000.");
                return;
            }

            string[] Sides = { "Heads", "Tails" };
            var GetSide = Sides[new Random().Next(0, Sides.Length)];

            if (Side.ToLower() == GetSide.ToLower())
            {
                UserEridium += Bet;
                await ReplyAsync($"Congratulations! You won {Bet}!");
            }
            else
            {
                UserEridium -= Bet;
                await ReplyAsync($"You lost {Bet}! :frowning:");
            }
        }

        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = Vmbed.Embed(VmbedColors.Cyan, Title: $"INFORMATION | {Context.Guild.Name}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");

            embed.AddInlineField("ID", Context.Guild.Id);
            embed.AddInlineField("Owner", await Context.Guild.GetOwnerAsync());
            embed.AddInlineField("Default Channel", await Context.Guild.GetDefaultChannelAsync());
            embed.AddInlineField("Voice Region", Context.Guild.VoiceRegionId);
            embed.AddInlineField("Created At", Context.Guild.CreatedAt);
            embed.AddInlineField("Roles", $"{Context.Guild.Roles.Count }\n{string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position))}");
            embed.AddInlineField("Users", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false));
            embed.AddInlineField("Bots", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true));
            embed.AddInlineField("Text Channels", (Context.Guild as SocketGuild).TextChannels.Count);
            embed.AddInlineField("Voice Channels", (Context.Guild as SocketGuild).VoiceChannels.Count);
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
            embed.AddInlineField("ID", Role.Id);
            embed.AddInlineField("Color", Role.Color);
            embed.AddInlineField("Creation Date", Role.CreatedAt);
            embed.AddInlineField("Is Hoisted?", Role.IsHoisted ? "Yes" : "No");
            embed.AddInlineField("Is Mentionable?", Role.IsMentionable ? "Yes" : "No");
            embed.AddInlineField("Is Managed?", Role.IsManaged ? "Yes" : "No");
            embed.AddInlineField("Permissions", string.Join(", ", Role.Permissions.ToList()));
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("UserInfo"), Alias("UI"), Summary("Displays information about a user.")]
        public async Task UserInfoAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var embed = Vmbed.Embed(VmbedColors.Pastel, Title: $"INFORMATION | {User}", ThumbUrl: User.GetAvatarUrl());
            List<IRole> Roles = new List<IRole>();
            foreach (var role in User.RoleIds)
            {
                var Role = Context.Guild.GetRole(role);
                Roles.Add(Role);
            }
            embed.AddInlineField("Username", User.Username);
            embed.AddInlineField("ID", User.Id);
            embed.AddInlineField("Muted?", User.IsMuted ? "Yes" : "No");
            embed.AddInlineField("Is Bot?", User.IsBot ? "Yes" : "No");
            embed.AddInlineField("Creation Date", User.CreatedAt);
            embed.AddInlineField("Join Date", User.JoinedAt);
            embed.AddInlineField("Status", User.Status);
            embed.AddInlineField("Permissions", string.Join(", ", User.GuildPermissions.ToList()));
            embed.AddInlineField("Roles", string.Join(", ", Roles.OrderByDescending(x => x.Position)));
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
            var Get = await HTTPExtension.HttpClient.GetAsync("https://api.tronalddump.io/random/quote").ConfigureAwait(false);
            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Using TrumpDump API was the worse trade, maybe ever.");
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
            var Get = await HTTPExtension.HttpClient.GetAsync("http://api.yomomma.info/").ConfigureAwait(false);
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
            string Changes = null;
            HTTPExtension.HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            using (var Response = await HTTPExtension.HttpClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits"))
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
            var embed = Vmbed.Embed(VmbedColors.Snow, Client.CurrentUser.GetAvatarUrl(), $"{Client.CurrentUser.Username}'s Official Invite",
                $"https://discordapp.com/oauth2/authorize?client_id={Client.CurrentUser.Id}&scope=bot&permissions=2146958591",
                Description: $"**Latest Changes:**\n{Changes}");
            embed.AddInlineField("Members",
                    $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                    $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                    $"Total: {Client.Guilds.Sum(x => x.Users.Count)}");
            embed.AddInlineField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}");
            embed.AddInlineField("Guilds", $"{Client.Guilds.Count}\n[Support Guild](https://discord.gg/nzYTzxD)");
            embed.AddInlineField(":space_invader:",
                $"Commands Ran: {BotConfig.Config.CommandsUsed}\n" +
                $"Messages Received: {BotConfig.Config.MessagesReceived.ToString("#,##0,,M", CultureInfo.InvariantCulture)}");
            embed.AddInlineField(":hammer_pick:",
                $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB");
            embed.AddInlineField(":beginner:", $"Written by: [Yucked](https://github.com/Yucked)\nDiscord.Net {DiscordConfig.Version}");
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Fuck"), Summary("Gives a random fuck about your useless fucking command.")]
        public async Task FoaasAsync(IGuildUser User = null)
            => await ReplyAsync(await FOAAS.RandomAsync(From: Context.User.Username, Name: User != null ? User.Username : "Bob").ConfigureAwait(false));

        [Command("Tweet"), Summary("Tweets from @Vuxey account!"), RequireSchmeckles(.35)]
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

            var embed = Vmbed.Embed(VmbedColors.Green, Description:
                $"**Tweet:** {TweetMessage}\n" +
                $"**Tweet ID:** {UserTweet.Id}\n" +
                $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})");
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Reply"), Summary("Replies back to a tweet!"), RequireSchmeckles(.15)]
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

                var embed = Vmbed.Embed(VmbedColors.Green, Description:
                    $"**Tweet:** {TweetMessage}\n" +
                    $"**Tweet ID:** {UserTweet.Id}\n" +
                    $"[Follow @Vuxey](https://twitter.com/Vuxey) | [Tweet Link]({UserTweet.Url})");
                await ReplyAsync("", embed: embed.Build());
            }
        }

        [Command("DeleteTweet"), Summary("Deletes a specified tweet!"), RequireSchmeckles(.5)]
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
            if (!Config.EridiumHandler.UsersList.ContainsKey(Context.User.Id))
            {
                return ReplyAsync("Woopsie. Couldn't find you Eridium leaderboards.");
            }
            return ReplyAsync($"You have {IntExtension.ConvertToSchmeckles(Config.EridiumHandler.UsersList[Context.User.Id])} Schmeckles.");
        }
    }
}
