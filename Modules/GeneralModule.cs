using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Discord.WebSocket;
using System.Diagnostics;
using System.Collections.Generic;
using Discord.Addons.InteractiveCommands;
using System.Linq;
using Rick.Handlers;
using Rick.Attributes;
using Rick.Classes;
using Rick.Services;
using System.Web;
using System.Text;

namespace Rick.Modules
{
    [CheckBlacklist]
    public class GeneralModule : ModuleBase
    {
        private InteractiveService Interactive;
        public GeneralModule(InteractiveService Inter)
        {
            Interactive = Inter;
        }

        [Command("GuildInfo"), Summary("Normal Command"), Remarks("Displays information about a guild"), Alias("Gi")]
        public async Task GuildInfoAsync(ulong ID = 0)
        {
            var gld = Context.Guild;
            var client = Context.Client as DiscordSocketClient;
            if (ID != 0)
                gld = client.GetGuild(ID);

            var GuildID = gld.Id;
            var GuildOwner = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            var GuildDefault = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention.ToUpper();
            var GuildVoice = gld.VoiceRegionId;
            var GuildCreated = gld.CreatedAt;
            var GuildAvailable = gld.Available;
            var GuildNotification = gld.DefaultMessageNotifications;
            var GuildEmbed = gld.IsEmbeddable;
            var GuildMfa = gld.MfaLevel;
            var GuildRoles = gld.Roles;
            var GuildAfak = gld.AFKTimeout;
            var GuildVeri = gld.VerificationLevel;
            var users = await gld.GetUsersAsync();
            var OnlineUsers = users.Count(x => x.Status == UserStatus.Online);
            var OfflineUsers = users.Count(x => x.Status == UserStatus.Offline);
            var InvisibleUsers = users.Count(x => x.Status == UserStatus.Invisible);
            var DndUsers = users.Count(x => x.Status == UserStatus.DoNotDisturb);
            var IdleUsers = users.Count(x => x.Status == UserStatus.Idle);
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = gld.Name;
                    x.IconUrl = gld.IconUrl;
                })
                .WithColor(new Color(153, 30, 87))
                .AddInlineField("Guild ID", GuildID)
                .AddInlineField("Guild Owner", GuildOwner)
                .AddInlineField("Default Channel", GuildDefault)
                .AddInlineField("Voice Region", GuildVoice)
                .AddInlineField("Created At", GuildCreated)
                .AddInlineField("Guild Available?", GuildAvailable)
                .AddInlineField("Default Notifcations", GuildNotification)
                .AddInlineField("Is Embedable?", GuildEmbed)
                .AddInlineField("MFA Level", GuildMfa)
                .AddInlineField("AFK Timeout", GuildAfak)
                .AddInlineField("Roles Count", GuildRoles.Count)
                .AddInlineField("Verification Level", GuildVeri)
                .AddInlineField("Total Guild Users", users.Count)
                .AddInlineField(":green_heart: Onlines Users", OnlineUsers)
                .AddInlineField(":black_circle: Offline Users", OfflineUsers)
                .AddInlineField(":black_circle: Invisble Users", InvisibleUsers)
                .AddInlineField(":red_circle: DND Users", DndUsers)
                .AddInlineField(":yellow_heart: Idle Users", IdleUsers)
                .AddInlineField(":robot: Bot Users", users.Where(u => u.IsBot).Count());
            await ReplyAsync("", false, embed);
        }

        [Command("Roleinfo"), Summary("Roleinfo RoleNameGoesHere"), Remarks("Displays information about given Role"), Alias("RI")]
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

        [Command("Userinfo"), Summary("Userinfo @Username"), Remarks("Displays information about a User"), Alias("UI")]
        public async Task UserInfoAsync(IUser user)
        {
            var usr = user as IGuildUser ?? Context.Message.Author as IGuildUser;
            var userNick = usr.Nickname ?? usr.Nickname;
            var userDisc = usr.DiscriminatorValue;
            var Userid = usr.Id;
            var isbot = usr.IsBot;
            var UserStatus = usr.Status;
            var UserGame = usr.Game.Value.Name;
            var UserCreated = usr.CreatedAt;
            var UserJoined = usr.JoinedAt;
            var UserPerms = usr.GuildPermissions;

            if (string.IsNullOrEmpty(userNick))
                userNick = "User has no nickname!";

            string descrption = $"**Nickname: **{userNick}\n**Discriminator: **{userDisc}\n**ID: **{Userid}\n**Is Bot: **{isbot}\n**Status: **{UserStatus}\n**Game: **{UserGame}\n**Created At: **{UserCreated}\n**Joined At: **{UserJoined}\n**Guild Permissions: **{UserPerms}";
            var embed = EmbedService.Embed(EmbedColors.White, user.Username, user.GetAvatarUrl(), Description: descrption);
            await ReplyAsync("", embed: embed);
        }

        [Command("Ping"), Summary("Normal Command"), Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            string descrption = $"**Gateway Latency:** { Gateway} ms\n**Response Latency:** {sw.ElapsedMilliseconds} ms\n**Delta:** {sw.ElapsedMilliseconds - Gateway} ms";
            var embed = EmbedService.Embed(EmbedColors.Blurple, "Ping Results", Context.Client.CurrentUser.GetAvatarUrl(), Description: descrption);
            await ReplyAsync("", embed: embed);

        }

        [Command("Ping"), Summary("Ping Google.com"), Remarks("Pings a website")]
        public async Task PwnedAsync(string search)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Clear();
                    http.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.MashapeAPIKey);
                    http.DefaultRequestHeaders.Add("Accept", "application/json");
                    var get = JObject.Parse(await http.GetStringAsync($"https://igor-zachetly-ping-uin.p.mashape.com/pinguin.php?address={search}"));
                    var time = get["time"].ToString();
                    var embed = EmbedService.Embed(EmbedColors.Blurple, Context.Client.CurrentUser.Username, Context.Client.CurrentUser.GetAvatarUrl(), Description: $"Ping Result: **{time} ms**");
                    await ReplyAsync("", embed: embed);
                }
            }
            catch (Exception e)
            { await ReplyAsync(e.Message); }
        }

        [Command("Embed"), Summary("Embed This is an embeded msg"), Remarks("Embeds a user message")]
        public async Task EmbedAsync(int Color1 = 255, int Color2 = 255, int Color3 = 255, [Remainder] string msg = "Idk how to use Embed Command")
        {
            await Context.Message.DeleteAsync();
            var embed = new EmbedBuilder()
                .WithColor(new Color(Color1, Color2, Color3))
                .WithDescription($"{Format.Italics(msg)}");
            await ReplyAsync("", embed: embed);
        }

        [Command("GenID"), Summary("GenID"), Remarks("Generates a UUID")]
        public async Task CreateUuidAsync()
        {
            var id = Guid.NewGuid().ToString();
            var embed = EmbedService.Embed(EmbedColors.Blurple, Context.User.Username, Context.User.GetAvatarUrl(), Description: $"Your unique UUID is: {id}");
            await ReplyAsync("", embed: embed);
        }

        [Command("Coinflip"), Summary("Coinflip"), Remarks("Flips a coin")]
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

        [Command("Afk"), Summary("Afk Add Reason"), Remarks("Adds you to afk list")]
        public async Task SetAfkAsync(GlobalEnums prop, [Remainder] string msg = "No reason provided!")
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            var List = gldConfig.AfkList;


            switch (prop)
            {
                case GlobalEnums.Add:
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
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("About"), Summary("Normal Command"), Remarks("Shows info about Bot")]
        public async Task InfoAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var AppInfo = await client.GetApplicationInfoAsync();
            string Description = $"Hello! I'm {AppInfo.Name} written by {AppInfo.Owner}! I'm also open source on Github! [https://github.com/ExceptionDev/Rick].\n" +
                $"If you plan to copy code from my source please do put {AppInfo.Owner} in your credits/help/info/about command/website and if you copy/clone my repo please don't remove this command!\n" +
                $"Also, leaving a star on my repo won't hurt you!\n" +
                $"Please user the {BotHandler.BotConfig.DefaultPrefix}Cmds for commands list and {BotHandler.BotConfig.DefaultPrefix}Help CommandName for more info on a command!\n" +
                $"**Services**\n" +
                $"I offer wide range of commands for admins and the users! Ranging from Basic commands such as Getting user/guild info to Google/Bing search commands! " +
                $"Wanna get naughty and keep your hands busy?! I've some NSFW commands as well to keep you entertained! " +
                $"Want to have some sort of rankings based on how much you talk?! I got Karma! Talk and recieve random karma based on your chat activity! + MANY MORE COMMANDS!\n" +
                $"**Invite URL:** https://discordapp.com/oauth2/authorize?client_id={AppInfo.Id}&scope=bot&permissions=2146958591\n" +
                $"**Help Guild:** https://discord.me/Noegenesis";
            var embed = EmbedService.Embed(EmbedColors.White, client.CurrentUser.Username, client.CurrentUser.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Encrypt"), Summary("Encrypt Password Some String"), Remarks("Encrypts a string for you")]
        public async Task EncryptAsync([Remainder] string Text)
        {
            string Get = EncryptionService.EncryptString(Text);
            if (string.IsNullOrWhiteSpace(Get))
            {
                await ReplyAsync("Null result");
                return;
            }
            await ReplyAsync(Get);
        }

        [Command("Decrypt"), Summary("Decrypt Password Some string"), Remarks("Decrypt a string for you")]
        public async Task DecryptAsync([Remainder] string Text)
        {
            string Get = EncryptionService.DecryptString(Text);
            if (string.IsNullOrWhiteSpace(Get))
            {
                await ReplyAsync("Null result");
                return;
            }
            await ReplyAsync(Get);
        }

        [Command("Rate"), Summary("Rate Pizza"), Remarks("Rates something out of 10")]
        public async Task RateAsync([Remainder] string text)
        {
            await ReplyAsync($":thinking: I would rate '{text}' a {new Random().Next(11)}/10");
        }

        [Command("Translate"), Summary("Translate Spanish What the Pizza?"), Remarks("Translates a Given Sentence")]
        public async Task TranslateAsync(string Language, [Remainder] string Text)
        {
            var result = await StaticMethodService.Translate(Language, Text);
            string Description = $"**Input:** {Text}\n" +
                $"**In {Language}:** {result.Translations[0].Translation}";
            var embed = EmbedService.Embed(EmbedColors.Blurple, "Translation Service!", Context.User.GetAvatarUrl(), Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Slotmachine"), Summary("Slotmachine 100"), Remarks("Slot machine!")]
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
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            gldConfig.Karma.TryGetValue(Context.User.Id, out int Credits);

            if (Credits <= 0 || Credits < Bet)
            {
                await ReplyAsync("You don't have enough karma for slot machine!");
                return;
            }
            if (Bet <= 0)
            {
                await ReplyAsync("Bet can't be lower than 0! Default bet is set to 666!");
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
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync("", embed: embed);
        }

        [Command("Trump"), Summary("Normal Command"), Remarks("Gives random trump quote")]
        public async Task TrumpAsync()
        {
            var Http = (JObject.Parse((await (await new HttpClient().GetAsync("https://api.tronalddump.io/random/quote")).Content.ReadAsStringAsync())))["value"];
            var embed = EmbedService.Embed(EmbedColors.Maroon, "TRUMMMP!", Context.Client.CurrentUser.GetAvatarUrl(), Description: Http.ToString());
            await ReplyAsync("", embed: embed);

        }

        [Command("Docs"), Summary("Docs Attributes"), Remarks("Searches Microsoft docs for terms")]
        public async Task MSDocsAsync([Remainder] string Search)
        {
            var client = new HttpClient();
            var Builder = new StringBuilder();
            string response = await client.GetStringAsync($"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={Search}");
            var Convert = JToken.Parse(response).ToObject<DocsMain>();
            if (Convert.count < 0)
            {
                await ReplyAsync("Nothing found!");
                return;
            }
            foreach(var Obj in Convert.results)
            {
                Builder.AppendLine($"**{Obj.displayName}**\n" +
                    $"**Type: **{Obj.itemType} || **Kind: **{Obj.itemKind}\n" +
                    $"**Description:** {Obj.description}\n({Obj.url})\n");
            }
            var embed = EmbedService.Embed(EmbedColors.White, Search, "https://exceptiondev.github.io/media/Book.png", Description: Builder.ToString(), FooterText: $"Total Results: {Convert.count.ToString()}");
            await ReplyAsync("", embed: embed);
        }
    }
}