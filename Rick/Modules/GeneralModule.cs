using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rick.Enums;
using Rick.JsonModels;
using Rick.Extensions;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Functions;

namespace Rick.Modules
{
    [CheckBlacklist, RequireBotPermission(GuildPermission.SendMessages)]
    public class GeneralModule : ModuleBase
    {
        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about a guild."), Remarks("GI 1234567890123 OR GI")]
        public async Task GuildInfoAsync(ulong ID = 0)
        {
            var gld = Context.Guild;
            var client = Context.Client as DiscordSocketClient;
            if (ID != 0)
                gld = client.GetGuild(ID);

            string Desc =
                $"**ID:** {gld.Id}\n" +
                $"**Owner:** {gld.GetOwnerAsync().GetAwaiter().GetResult().Username}\n" +
                $"**Default Channel:** {gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Name}\n" +
                $"**Voice Region:** {gld.VoiceRegionId}\n" +
                $"**Created At:** {gld.CreatedAt}\n" +
                $"**Roles:** {gld.Roles.Count}\n" +
                $"**Users:** {(await gld.GetUsersAsync()).Count(x => x.IsBot == false)}\n" +
                $"**Bots:** {(await gld.GetUsersAsync()).Count(x => x.IsBot == true)}\n" +
                $"**AFK Timeout:** {gld.AFKTimeout}\n";
            var embed = EmbedExtension.Embed(EmbedColors.Teal, Title: $"{gld.Name} Information", Description: Desc, ThumbUrl: gld.IconUrl);
            await ReplyAsync("", false, embed);
        }

        [Command("Roleinfo"), Alias("RI"), Summary("Displays information about a role"), Remarks("Roleinfo @Rolename OR @RI \"RoleName\"")]
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

        [Command("Userinfo"), Alias("UI"), Summary("Displays information about a username."), Remarks("Userinfo OR Userinfo @Username")]
        public async Task UserInfoAsync(IGuildUser User = null)
        {
            SocketGuildUser usr = null;
            if (User != null)
                usr = User as SocketGuildUser;
            else
                usr = Context.User as SocketGuildUser;

            var userNick = usr.Nickname ?? usr.Nickname;
            var userDisc = usr.DiscriminatorValue;
            var Userid = usr.Id;
            var isbot = usr.IsBot ? "YEAAAA!" : "Nopeee";
            var UserStatus = usr.Status;
            var UserGame = usr.Game.Value.Name;
            var UserCreated = usr.CreatedAt;
            var UserJoined = usr.JoinedAt;
            var UserPerms = usr.GuildPermissions;

            if (string.IsNullOrEmpty(userNick))
                userNick = "User has no nickname!";

            string descrption = $"**Nickname: **{userNick}\n**Discriminator: **{userDisc}\n**ID: **{Userid}\n**Is Bot: **{isbot}\n**Status: **{UserStatus}" +
                $"\n**Game: **{UserGame}\n**Created At: **{UserCreated}\n**Joined At: **{UserJoined}\n**Guild Permissions: **{UserPerms}";

            var embed = EmbedExtension.Embed(EmbedColors.White, $"{usr.Username} Information", usr.GetAvatarUrl(), Description: descrption, ImageUrl: usr.GetAvatarUrl());
            await ReplyAsync("", embed: embed);
        }

        [Command("Ping"), Summary("Measures gateway ping and response time. If a destionation is provided then it pings the destionation."), Remarks("Ping Google.com")]
        public async Task PingAsync(string Destination = null)
        {
            string Time = null;
            if (!string.IsNullOrWhiteSpace(Destination) || Destination != null)
            {
                try
                {
                    var HttpClient = new HttpClient();
                    HttpClient.DefaultRequestHeaders.Clear();
                    HttpClient.DefaultRequestHeaders.Add("X-Mashape-Key", ConfigHandler.IConfig.APIKeys.MashapeKey);
                    HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
                    var get = JObject.Parse(await HttpClient.GetStringAsync($"https://igor-zachetly-ping-uin.p.mashape.com/pinguin.php?address={Destination}"));
                    var WebResponse = get["time"].ToString();
                    Time = WebResponse;
                }
                catch { }
            }
            if (string.IsNullOrWhiteSpace(Time))
                Time = "No IP/Web Address was provided to ping.";

            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            string descrption = $"**Gateway Latency:** { client.Latency } ms\n" +
                $"**Response Latency:** {sw.ElapsedMilliseconds} ms\n" +
                $"**Delta:** {sw.ElapsedMilliseconds - client.Latency} ms\n" +
                $"**IP/Web Response Time:** {Time}";
            var embed = EmbedExtension.Embed(EmbedColors.Blurple, "Ping Results", Context.Client.CurrentUser.GetAvatarUrl(), Description: descrption);
            await ReplyAsync("", embed: embed);
        }

        [Command("Embed"), Summary("Embeds a user message."), Remarks("Embed 123 123 123 This is embed message.")]
        public async Task EmbedAsync(int Color1 = 255, int Color2 = 255, int Color3 = 255, [Remainder] string msg = "Idk how to use Embed Command")
        {
            await Context.Message.DeleteAsync();
            var embed = new EmbedBuilder()
                .WithColor(new Color(Color1, Color2, Color3))
                .WithDescription($"{Format.Italics(msg)}");
            await ReplyAsync("", embed: embed);
        }

        [Command("GenID"), Summary("Generates a random GUID.")]
        public async Task CreateUuidAsync()
        {
            var id = Guid.NewGuid().ToString();
            var embed = EmbedExtension.Embed(EmbedColors.Blurple, Context.User.Username, Context.User.GetAvatarUrl(), Description: $"Your unique UUID is: {id}");
            await ReplyAsync("", embed: embed);
        }

        [Command("Coinflip"), Summary("Flips a coin like any other coin flip.")]
        public async Task CoinFlipAsync()
        {
            var rand = new Random().Next(2);
            switch (rand)
            {
                case 0:
                    await ReplyAsync("HEAAADS");
                    break;
                case 1:
                    await ReplyAsync("TAAAILS");
                    break;
            }
        }

        [Command("AFK"), Summary("Adds you to the Guild's AFK list."), Remarks("AFK Add This is a reason. OR AFK Remove OR AFK Modify New Reason")]
        public async Task SetAfkAsync(GlobalEnums prop, [Remainder] string msg = "No reason provided!")
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            var List = gldConfig.AFKList;

            switch (prop)
            {
                case GlobalEnums.Add:
                    if (List.ContainsKey(Context.User.Id))
                    {
                        await ReplyAsync("You are already in the AFK List! :anger:");
                        return;
                    }
                    List.Add(Context.User.Id, msg);
                    await ReplyAsync($"Added {Context.User.Username} to Guild's AFK list with message: **{msg}**");
                    break;

                case GlobalEnums.Remove:
                    List.Remove(Context.User.Id);
                    await ReplyAsync($"Removed {Context.User.Username} from the Guild's AFK list!");
                    break;

                case GlobalEnums.Modify:
                    List[Context.User.Id] = msg;
                    await ReplyAsync("Your message has been modified!");
                    break;
            }

            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("About"), Summary("Displays information about the bot.")]
        public async Task AboutAsync()
        {
            string Message = $"HENLO! I'm Ricky R-r-r-r-RICK! c: :eggplant:" +
                $"I'm better than SIRI and you! Yea! You heard that right! :100:" +
                $"Invite me to your server so I can molest all of your users or if you wanna get laid real quick. " +
                $"I'm all about them Girls bro.\n" +
                $"I'm written by Yucked and this is my ??9?? rewrite. Always trying to improve to provide better fucntionality.";
            string Misc = $"[My Website](https://Rickbot.cf) | [Command List](https://Rickbot.cf/Pages/Commands.html) | " +
                $"[My Support Server](https://discord.gg/S5CnhVY) | [Follow Me](https://twitter.com/Vuxey) | " +
                $"[Invite Me](https://discordapp.com/oauth2/authorize?client_id=261561347966238721&scope=bot&permissions=2146946175)";

            var embed = EmbedExtension.Embed(EmbedColors.Gold,
                "Ricky Rick", Context.Client.CurrentUser.GetAvatarUrl(),
                Description: $"{Message}\n{Misc}");
            await ReplyAsync("", embed: embed);
        }

        [Command("Rate"), Summary("Rates something for you out of 10."), Remarks("Rate Kendrick")]
        public async Task RateAsync([Remainder] string text)
        {
            await ReplyAsync($":thinking: I would rate '{text}' a {new Random().Next(11)}/10");
        }

        [Command("Translate"), Summary("Translates a sentence into the specified language."), Remarks("Translate Spanish What the Pizza?")]
        public async Task TranslateAsync(string Language, [Remainder] string Text)
        {
            var result = await Function.Translate(Language, Text);
            string Description = $"**Input:** {Text}\n" +
                $"**In {Language}:** {result.Translations[0].Translation}";
            var embed = EmbedExtension.Embed(EmbedColors.Blurple, "Translation Service!",
                Context.User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Slotmachine"), Summary("Want to earn quick karma? That's how you earn some."), Remarks("Slotmachine 100"), Cooldown(5)]
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
            var Guilds = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = Guilds.KarmaList;
            int Credits = karmalist[Context.User.Id];

            if (Guilds.IsKarmaEnabled == false)
            {
                await ReplyAsync("Chat Karma is disabled! Ask Admin or server owner to enable Chat Karma!");
                return;
            }

            if (Credits <= 0 || Credits < Bet)
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
                Credits -= Bet;
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = $"{Context.User.Username} Lost!",
                    IconUrl = Context.User.GetAvatarUrl()
                };
                embed.Description = $"Uhoh! It seems you weren't lucky this time! Better luck next time! :weary:\nYour current Karma is {Credits}";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                Credits += Bet;
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = $"{Context.User.Username} won!",
                    IconUrl = Context.User.GetAvatarUrl()
                };
                embed.Description = $":tada: Your current Karma is {Credits} :tada:";
                embed.Color = new Color(0x93ff89);
            }
            karmalist[Context.User.Id] = Credits;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
            await ReplyAsync("", embed: embed);
        }

        [Command("Trump"), Summary("Fetches random Quotes/Tweets said by Donald Trump.")]
        public async Task TrumpAsync()
        {
            var Http = (JObject.Parse((await (await new HttpClient().GetAsync("https://api.tronalddump.io/random/quote")).Content.ReadAsStringAsync())))["value"];
            string Pic = "http://abovethelaw.com/wp-content/uploads/2016/04/cartoon-trump-300x316.jpg";
            var embed = EmbedExtension.Embed(EmbedColors.Maroon, "TRUMMMP!",
                Pic, Description: Http.ToString());
            await ReplyAsync("", embed: embed);

        }

        [Command("Docs"), Summary("Searches Microsoft docs for terms"), Remarks("Docs Attributes")]
        public async Task DocsAsync([Remainder] string Search)
        {
            var Builder = new StringBuilder();
            var Response = await new HttpClient().GetAsync($"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={Search}");
            if (!Response.IsSuccessStatusCode)
            {
                await ReplyAsync(Response.ReasonPhrase);
                return;
            }
            var ConvertedJson = JsonConvert.DeserializeObject<DocsRoot>(await Response.Content.ReadAsStringAsync());
            foreach (var result in ConvertedJson.Results.Take(5).OrderBy(x => x.Name))
            {
                Builder.AppendLine($"**{result.Name}**\n" +
                    $"**Kind: **{result.Kind} || **Type: **{result.Type}\n" +
                    $"**Summary: **{result.Snippet}\n" +
                    $"**URL: ** {result.URL}\n");
            }
            var embed = EmbedExtension.Embed(EmbedColors.White, Search,
                "https://exceptiondev.github.io/media/Book.png", Description: Builder.ToString(), FooterText: $"Total Results: {ConvertedJson.Count.ToString()}");
            await ReplyAsync("", embed: embed);
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!"), Remarks("Flip Heads 100"), Cooldown(5)]
        public async Task FlipAsync(string Side, int Bet = 50)
        {
            var GC = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = GC.KarmaList;
            int Karma = karmalist[Context.User.Id];

            if (GC.IsKarmaEnabled == false)
            {
                await ReplyAsync("Chat Karma is disabled! Ask the admin to enable ChatKarma!");
                return;
            }

            if (int.TryParse(Side, out int res))
            {
                await ReplyAsync("Side can't be a number. Use help command for more information!");
                return;
            }

            if (Karma < Bet || Karma <= 0)
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
                Karma += Bet * 2;
                await ReplyAsync($"Congratulations! You won {Bet}! Your current karma is {Karma}.");
            }
            else
            {
                Karma -= Bet;
                await ReplyAsync($"You lost {Bet}! :frowning: Your current Karma is {Karma}.");
            }
            karmalist[Context.User.Id] = Karma;
            GuildHandler.GuildConfigs[Context.Guild.Id] = GC;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Stats"), Summary("Shows information about Bot.")]
        public async Task StatsAsync()
        {
            var length = new FileInfo(ConfigHandler.ConfigFile).Length + new FileInfo(GuildHandler.configPath).Length;
            var cache = Function.DirSize(new DirectoryInfo(ConfigHandler.CacheFolder));
            string Description =
                $"- Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}\n" +
                $"- Databse Size: {Convert.ToInt32(length)} Bytes\n" +
                $"- Cache Size: {Convert.ToInt32(cache)} Bytes\n" +
                $"- Total Command Used: {ConfigHandler.IConfig.CommandsUsed}\n" +
                $"- Total Messages Received: {ConfigHandler.IConfig.MessagesReceived}";

            var embed = EmbedExtension.Embed(EmbedColors.Teal, Title: "Rick Stats", Description: Description, ThumbUrl: 
                Context.Client.CurrentUser.GetAvatarUrl());
            await ReplyAsync("", embed: embed);
        }

        [Command("Avatar"), Summary("Shows users avatar in higher resolution."), Remarks("Avatar @Yucked")]
        public async Task UserAvatarAsync(SocketGuildUser User)
        {
            await ReplyAsync(User.GetAvatarUrl(size: 2048));
        }

        [Command("Karma"), Summary("Gives another user karma."), Remarks("Karma @Username 500")]
        public async Task KarmaAsync(IGuildUser user, int Karma)
        {
            if (user.Id == Context.Client.CurrentUser.Id || user.Id == Context.User.Id) return;
            var gldConfig = GuildHandler.GuildConfigs[user.GuildId];
            var karmalist = gldConfig.KarmaList;
            int UserKarma = karmalist[Context.User.Id];

            if (Karma <= 0)
            {
                await ReplyAsync("Karma can't be <= 0!");
                return;
            }

            if (UserKarma < Karma || UserKarma <= 0)
            {
                await ReplyAsync("You don't have enough karma!");
                return;
            }

            if (!karmalist.ContainsKey(user.Id))
            {
                karmalist.Add(user.Id, 1);
                await ReplyAsync($"Added {user.Username} to Karma List and gave 1 Karma to {user.Username}");
            }
            else
            {
                int getKarma = karmalist[user.Id];
                getKarma += Karma;
                UserKarma -= Karma;
                karmalist[user.Id] = getKarma;
                karmalist[Context.User.Id] = UserKarma;
                await ReplyAsync($"Gave {Karma} Karma to {user.Username}");
            }
            GuildHandler.GuildConfigs[user.GuildId] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        [Command("Rank"), Summary("Shows how much Karma you have")]
        public async Task GetKarmaAsync(SocketGuildUser User = null)
        {
            SocketGuildUser KarmaUser = null;
            if (User != null)
                KarmaUser = User;
            else
                KarmaUser = Context.User as SocketGuildUser;

            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.KarmaList;
            karmalist.TryGetValue(KarmaUser.Id, out int karma);
            if (karma <= 0 || !karmalist.ContainsKey(KarmaUser.Id))
            {
                await ReplyAsync("User doesn't exist or no Karma was found!");
                return;
            }

            int Level = Fomulas.GetLevel(karma);
            int KarmaLast = Fomulas.GetKarmaForLastLevel(Level);
            int KarmaNext = Fomulas.GetKarmaForNextLevel(Level);
            string Image = KarmaUser.GetAvatarUrl();
            var embed = EmbedExtension.Embed(EmbedColors.Gold, $"{KarmaUser.Username} Rankings", Image, ThumbUrl: Image);
            embed.AddInlineField("Level", Level);
            embed.AddInlineField("Karma", karma);
            embed.AddInlineField("Karma Required For Last Level", KarmaLast);
            embed.AddInlineField("Karma Required For Next Level", KarmaNext);
            await ReplyAsync("", embed: embed);
        }

        [Command("Top"), Summary("Shows users with top Karma")]
        public async Task TopAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.KarmaList;
            var filter = karmalist.OrderByDescending(x => x.Value).Take(10);
            var embed = EmbedExtension.Embed(EmbedColors.Pastle, $"Top 10 Users", Context.Guild.IconUrl);
            if (karmalist.Count <= 0)
            {
                await ReplyAsync("Guild's Karma list is empty!");
                return;
            }

            foreach (var val in filter)
            {
                var user = (await Context.Guild.GetUserAsync(val.Key)) as SocketGuildUser;
                var Level = Fomulas.GetLevel(val.Value);
                embed.AddInlineField(user.Username, $"**Karma:** {val.Value} | **Level:** {Level}");
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("Yomama"), Summary("Gets a random Yomma Joke")]
        public async Task YommaAsync()
        {
            var Get = await new HttpClient().GetAsync("http://api.yomomma.info/");

            if (!Get.IsSuccessStatusCode)
            {
                await ReplyAsync(Get.ReasonPhrase);
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

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list."), Remarks("Iam Ultimate-Meme-God")]
        public async Task IAmAsync(IRole Role)
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];

            if (!GuildConfig.AssignableRoles.Contains(Role.Name))
            {
                await ReplyAsync($"{Role.Name} doesn't exist in guild's assignable role list.");
                return;
            }

            var User = Context.User as SocketGuildUser;

            if (User.Roles.Contains(Role))
            {
                await ReplyAsync($"You already have **{Role.Name}** role!");
                return;
            }

            await User.AddRoleAsync(Role);
            await ReplyAsync($"You have been added to **{Role.Name}** role!");
        }

        [Command("IamNot"), Summary("Removes you from the specified role."), Remarks("IAmNot Ultimate-Meme-God")]
        public async Task IAmNotAsync(IRole Role)
        {
            var GuildConfig = GuildHandler.GuildConfigs[Context.Guild.Id];

            if (!GuildConfig.AssignableRoles.Contains(Role.Name))
            {
                await ReplyAsync($"{Role.Name} doesn't exist in guild's assignable role list.");
                return;
            }

            var User = Context.User as SocketGuildUser;

            if (!User.Roles.Contains(Role))
            {
                await ReplyAsync($"You do not have **{Role.Name}** role!");
                return;
            }

            await User.RemoveRoleAsync(Role);
            await ReplyAsync($"You have been removed from **{Role.Name}** role!");
        }

        [Command("Discrim"), Summary("Gets all users who match a certain discrim"), Remarks("Discrim 0001")]
        public async Task DiscrimAsync(string DiscrimValue)
        {
            var Guilds = (Context.Client as DiscordSocketClient).Guilds;
            var sb = new StringBuilder();
            foreach (var gld in Guilds)
            {
                var dis = gld.Users.Where(x => x.Discriminator == DiscrimValue && x.Username != Context.User.Username);
                foreach (var d in dis)
                {
                    sb.AppendLine(d.Username);
                }
            }
            if (!string.IsNullOrWhiteSpace(sb.ToString()))
                await ReplyAsync($"Users matching **{DiscrimValue}** Discriminator:\n{sb.ToString()}");
            else
                await ReplyAsync($"No usernames found matching **{DiscrimValue}** discriminator.");
        }
    }
}
