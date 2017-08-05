﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers.GuildHandler;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Services;
using Valerie.Handlers;
using Valerie.Handlers.ConfigHandler;

namespace Valerie.Modules
{
    public class GeneralModule : ModuleBase
    {
        [Command("Ping"), Summary("Pings Discord Gateway")]
        public async Task PingAsync()
            => await ReplyAsync($"Latency: {(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Rank"), Summary("Shows your current rank and how much Karma is needed for next level.")]
        public async Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.KarmaList.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User.Username} doesn't exist in Karma list.");
                return;
            }
            var UserKarma = Config.KarmaList.TryGetValue(User.Id, out int Karma);
            var embed = Vmbed.Embed(VmbedColors.Pastel, ThumbUrl: User.GetAvatarUrl());
            embed.AddInlineField("Karma", Karma);
            embed.AddInlineField("Level", IntExtension.GetLevel(Karma));
            embed.AddInlineField("Previous Level", IntExtension.GetKarmaForLastLevel(IntExtension.GetLevel(Karma)));
            embed.AddInlineField("Next Level", IntExtension.GetKarmaForNextLevel(IntExtension.GetLevel(Karma)));
            await ReplyAsync("", embed: embed);
        }

        [Command("Top"), Summary("Shows top 10 users in the Karma list.")]
        public async Task KarmaAsync()
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (Config.KarmaList.Count == 0)
            {
                await ReplyAsync("There are no top users for this guild.");
                return;
            }
            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"{Context.Guild.Name.ToUpper()} | Top 10 Users");
            var Karmalist = Config.KarmaList.OrderByDescending(x => x.Value).Take(10);
            foreach (var Value in Karmalist)
            {
                var User = await Context.Guild.GetUserAsync(Value.Key) as IGuildUser;
                if (User == null)
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.KarmaDelete, $"{User.Id}");
                embed.AddInlineField(User.Username, $"Rank: {Value.Value} | Level: {IntExtension.GetLevel(Value.Value)}");
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("AFK"), Summary("Adds Or Removes you from AFK list.")]
        public async Task AFKAsync(CommandEnums Action, [Remainder] string AFKMessage = "I'm busy.")
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            switch (Action)
            {
                case CommandEnums.Add:
                    if (Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are already in the AFK list.");
                        return;
                    }
                    await ServerDB.AFKHandlerAsync(Context.Guild.Id, ModelEnum.AFKAdd, Context.User.Id, AFKMessage);
                    await ReplyAsync("You have been added to the AFK list.");
                    break;
                case CommandEnums.Remove:
                    if (!Config.AFKList.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are not in the AFK list.");
                        return;
                    }
                    await ServerDB.AFKHandlerAsync(Context.Guild.Id, ModelEnum.AFKRemove, Context.User.Id);
                    await ReplyAsync("You have been removed from the AFK list.");
                    break;
                case CommandEnums.Modify:
                    await ServerDB.AFKHandlerAsync(Context.Guild.Id, ModelEnum.AFKModify, Context.User.Id, AFKMessage);
                    await ReplyAsync("Your AFK message have been modified.");
                    break;
            }
        }

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list.")]
        public async Task IAmAsync(IRole Role)
        {
            var GuildConfig = ServerDB.GuildConfig(Context.Guild.Id);
            var User = Context.User as SocketGuildUser;
            if (!GuildConfig.AssignableRoles.Contains(Role.Id.ToString()))
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
            var GuildConfig = ServerDB.GuildConfig(Context.Guild.Id);
            var User = Context.User as SocketGuildUser;

            if (!GuildConfig.AssignableRoles.Contains(Role.Id.ToString()))
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

        [Command("Slotmachine"), Summary("Want to earn quick karma? That's how you earn some."), Cooldown(5)]
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
            var Guild = ServerDB.GuildConfig(Context.Guild.Id);
            var UserKarma = Guild.KarmaList[Context.User.Id];
            if (Guild.IsKarmaEnabled == false)
            {
                await ReplyAsync("Chat Karma is disabled! Ask Admin or server owner to enable Chat Karma!");
                return;
            }

            if (UserKarma <= 0 || UserKarma < Bet)
            {
                await ReplyAsync("You don't have enough karma for slot machine!");
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
                await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaSubtract, Context.User.Id, Bet);
                embed.Description = $"You lost {Bet}. Better luck next time! :weary:";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaUpdate, Context.User.Id, Bet);
                embed.Description = $"You won {Bet} :tada:";
                embed.Color = new Color(0x93ff89);
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!"), Cooldown(5)]
        public async Task FlipAsync(string Side, int Bet = 50)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            int UserKarma = Config.KarmaList[Context.User.Id];
            if (Config.IsKarmaEnabled == false)
            {
                await ReplyAsync("Chat Karma is disabled! Ask the admin to enable ChatKarma!");
                return;
            }

            if (int.TryParse(Side, out int res))
            {
                await ReplyAsync("Side can't be a number. Use help command for more information!");
                return;
            }

            if (UserKarma < Bet || UserKarma <= 0)
            {
                await ReplyAsync("You don't have enough karma!");
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
                await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaUpdate, Context.User.Id, Bet * 2);
                await ReplyAsync($"Congratulations! You won {Bet}!");
            }
            else
            {
                await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaSubtract, Context.User.Id, Bet);
                await ReplyAsync($"You lost {Bet}! :frowning:");
            }
        }

        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            string Desc =
                $"**ID:** {Context.Guild.Id}\n" +
                $"**Owner:** {Context.Guild.GetOwnerAsync().GetAwaiter().GetResult().Username}\n" +
                $"**Default Channel:** {Context.Guild.GetDefaultChannelAsync().GetAwaiter().GetResult().Name}\n" +
                $"**Voice Region:** {Context.Guild.VoiceRegionId}\n" +
                $"**Created At:** {Context.Guild.CreatedAt}\n" +
                $"**Roles:** {Context.Guild.Roles.Count}\n```{string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position))}```\n" +
                $"**Users:** {(await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false)}\n" +
                $"**Bots:** {(await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true)}\n" +
                $"**Text Channels:** {(Context.Guild as SocketGuild).TextChannels.Count}\n" +
                $"**Voice Channels:** {(Context.Guild as SocketGuild).VoiceChannels.Count}\n" +
                $"**AFK Timeout:** {Context.Guild.AFKTimeout}\n";

            var embed = Vmbed.Embed(VmbedColors.Cyan, Title: $"INFORMATION | {Context.Guild.Name}", Description: Desc,
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");
            await ReplyAsync("", false, embed);
        }

        [Command("Roleinfo"), Alias("RI"), Summary("Displays information about a role")]
        public async Task RoleInfoAsync(IRole role)
        {
            var gld = Context.Guild;
            var chn = Context.Channel;
            var msg = Context.Message;
            var grp = role;
            if (grp == null)
                throw new ArgumentException("You must supply a role.");
            var grl = grp as SocketRole;
            var gls = gld as SocketGuild;

            var embed = new EmbedBuilder()
            {
                Title = "Role"
            };
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Name";
                x.Value = grl.Name;
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "ID";
                x.Value = grl.Id.ToString();
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Color";
                x.Value = grl.Color.RawValue.ToString("X6");
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Hoisted?";
                x.Value = grl.IsHoisted ? "Yes" : "No";
            });

            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Mentionable?";
                x.Value = grl.IsMentionable ? "Yes" : "No";
            });

            var perms = new List<string>(23);
            if (grl.Permissions.Administrator)
                perms.Add("Administrator");
            if (grl.Permissions.AttachFiles)
                perms.Add("Can attach files");
            if (grl.Permissions.BanMembers)
                perms.Add("Can ban members");
            if (grl.Permissions.ChangeNickname)
                perms.Add("Can change nickname");
            if (grl.Permissions.Connect)
                perms.Add("Can use voice chat");
            if (grl.Permissions.CreateInstantInvite)
                perms.Add("Can create instant invites");
            if (grl.Permissions.DeafenMembers)
                perms.Add("Can deafen members");
            if (grl.Permissions.EmbedLinks)
                perms.Add("Can embed links");
            if (grl.Permissions.KickMembers)
                perms.Add("Can kick members");
            if (grl.Permissions.ManageChannels)
                perms.Add("Can manage channels");
            if (grl.Permissions.ManageMessages)
                perms.Add("Can manage messages");
            if (grl.Permissions.ManageNicknames)
                perms.Add("Can manage nicknames");
            if (grl.Permissions.ManageRoles)
                perms.Add("Can manage roles");
            if (grl.Permissions.ManageGuild)
                perms.Add("Can manage guild");
            if (grl.Permissions.MentionEveryone)
                perms.Add("Can mention everyone group");
            if (grl.Permissions.MoveMembers)
                perms.Add("Can move members between voice channels");
            if (grl.Permissions.MuteMembers)
                perms.Add("Can mute members");
            if (grl.Permissions.ReadMessageHistory)
                perms.Add("Can read message history");
            if (grl.Permissions.ReadMessages)
                perms.Add("Can read messages");
            if (grl.Permissions.SendMessages)
                perms.Add("Can send messages");
            if (grl.Permissions.SendTTSMessages)
                perms.Add("Can send TTS messages");
            if (grl.Permissions.Speak)
                perms.Add("Can speak");
            if (grl.Permissions.UseVAD)
                perms.Add("Can use voice activation");
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Permissions";
                x.Value = string.Join(", ", perms);
            });

            await chn.SendMessageAsync("", false, embed);
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
            await ReplyAsync((JObject.Parse((
                await (await new HttpClient()
                .GetAsync("https://api.tronalddump.io/random/quote"))
                .Content.ReadAsStringAsync())))["value"].ToString());
        }

        [Command("Avatar"), Summary("Shows users avatar in higher resolution.")]
        public async Task UserAvatarAsync(SocketGuildUser User)
            => await ReplyAsync(User.GetAvatarUrl(size: 2048));

        [Command("Yomama"), Summary("Gets a random Yomma Joke")]
        public async Task YommaAsync()
        {
            var Get = await new HttpClient().GetAsync("http://api.yomomma.info/");

            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync("Yomomma so fat that she crashed yomomma API.");
                return;
            }
            await ReplyAsync(JObject.Parse(await Get.Content.ReadAsStringAsync())["joke"].ToString());
        }

        [Command("Probe"), Summary("Probes someone or yourself.")]
        public async Task ProbeAsync(SocketGuildUser User = null)
        {
            SocketGuildUser GetUser = null;
            if (User != null)
            {
                GetUser = User;
                await ReplyAsync($"**Probes {GetUser.Username} anus with a massive black dildo** :eggplant:\nYou dirty lilttle slut.");
            }
            else
            {
                GetUser = Context.User as SocketGuildUser;
                await ReplyAsync($"**Probes {GetUser.Username} anus with a massive black dildo** :eggplant:\nYou dumb cunt! Don't know how to use a fucking command?!");
            }
        }

        [Command("Discrim"), Summary("Gets all users who match a certain discrim")]
        public async Task DiscrimAsync(IGuildUser User)
        {
            var Guilds = (Context.Client as DiscordSocketClient).Guilds;
            var sb = new StringBuilder();
            foreach (var gld in Guilds)
            {
                var dis = gld.Users.Where(x => x.Discriminator == User.Discriminator && x.Id != User.Id);
                foreach (var d in dis)
                {
                    sb.AppendLine(d.Username);
                }
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                await ReplyAsync($"Users matching **{User.Discriminator}** Discriminator:\n{sb.ToString()}");
            else
                await ReplyAsync($"No usernames found matching **{User.Discriminator}** discriminator.");
        }

        [Command("Stats"), Alias("About"), Summary("Shows information about Bot.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            string Changes = null;
            using (var Http = new HttpClient())
            {
                Http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                using (var Response = await Http.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits"))
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
                }
                Http.Dispose();
            }

            var embed = Vmbed.Embed(VmbedColors.Snow, Client.CurrentUser.GetAvatarUrl(), $"{Client.CurrentUser.ToString()} Official Invite",
                "https://discordapp.com/oauth2/authorize?client_id=261561347966238721&scope=bot&permissions=2146958591",
                Description: $"**Latest Changes:**\n{Changes}");
            embed.AddInlineField("Members",
                $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Users.Count)}");
            embed.AddInlineField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}");
            embed.AddInlineField("Guilds", Client.Guilds.Count);
            embed.AddInlineField(":space_invader:",
                $"Commands Ran: {BotDB.Config.CommandsUsed}\n" +
                $"Messages Received: {BotDB.Config.MessagesReceived.ToString("#,##0,,M", CultureInfo.InvariantCulture)}");
            embed.AddInlineField(":hammer_pick:",
                $"Cache: {Convert.ToInt32((Misc.DirSize(new DirectoryInfo(MainHandler.CacheFolder)) / 1024) / 1024.0)} MB\n" +
                $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB");
            embed.AddInlineField(":beginner:", "Written by: [Yucked](https://github.com/Yucked)\nLibrary: Discord.Net 1.0");
            await ReplyAsync("", embed: embed);
        }
    }
}
