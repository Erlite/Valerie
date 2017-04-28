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
        public async Task GuildInfoAsync()
        {
            var gld = Context.Guild;
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
        public async Task UserInfoAsync(IUser user = null)
        {
            if (user == null)
                throw new NullReferenceException("You must mention a user for me to display information!");
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
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = usr.Username;
                    x.IconUrl = usr.GetAvatarUrl();
                })
                .WithDescription($"**Nickname: **{userNick}\n**Discriminator: **{userDisc}\n**ID: **{Userid}\n**Is Bot: **{isbot}\n**Status: **{UserStatus}\n**Game: **{UserGame}\n" +
                $"**Created At: **{UserCreated}\n**Joined At: **{UserJoined}\n**Guild Permissions: **{UserPerms}")
                .WithColor(new Color(255, 255, 255));
            await ReplyAsync("", embed: embed);
        }

        [Command("Ping"), Summary("Normal Command"), Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            var embed = new EmbedBuilder()
                .WithTitle("Ping Results")
                .WithDescription($"**Gateway Latency:** { Gateway} ms" +
                            $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms" +
                            $"\n**Delta:** {sw.ElapsedMilliseconds - Gateway} ms")
                .WithColor(new Color(244, 66, 125));
            await ReplyAsync("", embed: embed);

        }

        [Command("Embed"), Summary("Embed This is an embeded msg"), Remarks("Embeds a user message")]
        public async Task EmbedAsync(int Color1 = 255, int Color2 = 255, int Color3 = 255, [Remainder] string msg = "Sorry, I'm too dumb to use an embed command!")
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
            var embed = new EmbedBuilder()
                .WithColor(new Color(255, 255, 255))
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Username;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"Your unique UUID is: {id}");
            await ReplyAsync("", false, embed);
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

        [Command("Response"), Summary("Normal Command"), Remarks("Uses Interactiveactive command to create a new response for you")]
        public async Task ResponseAsync(string name, [Remainder]string response)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new NullReferenceException(":skull: Response name can't be empty!");
            if (string.IsNullOrWhiteSpace(response))
                throw new NullReferenceException(":skull: Response content can't be empty!");

            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var resp = gldConfig.Responses;
            if (resp.ContainsKey(name))
                await ReplyAsync("A response with the exact name already exist! :skull_crossbones:");
            resp.Add(name, response);
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            var embed = new EmbedBuilder()
                .WithAuthor(x => { x.Name = "New response added!"; x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); })
                .WithDescription($"**Response Trigger:** {name}\n**Response: **{response}")
                .WithColor(new Color(109, 242, 122));
            await ReplyAsync("", embed: embed);
        }

        [Command("Afk"), Summary("Afk Add Reason"), Remarks("Adds you to afk list")]
        public async Task SetAfkAsync(ListProperty prop, [Remainder] string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            var List = gldConfig.AfkList;

            switch (prop)
            {
                case ListProperty.Add:
                    List.Add(Context.User.Id, msg);
                    GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
                    await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                    await ReplyAsync($"Added {Context.User.Username} to Guild's AFK list with message: **{msg}**");
                    break;

                case ListProperty.Remove:
                    List.Remove(Context.User.Id);
                    GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
                    await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                    await ReplyAsync($"Removed {Context.User.Username} from the Guild's AFK list!");
                    break;

            }
        }

        [Command("Ping"), Summary("Ping Google.com"), Remarks("Pings a website")]
        public async Task PwnedAsync(string search)
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.DefaultRequestHeaders.Clear();
                    http.DefaultRequestHeaders.Add("X-Mashape-Key", BotHandler.BotConfig.MashapeKey);
                    http.DefaultRequestHeaders.Add("Accept", "application/json");
                    var get = JObject.Parse( await http.GetStringAsync($"https://igor-zachetly-ping-uin.p.mashape.com/pinguin.php?address={search}"));
                    var time = get["time"].ToString();
                    var embed = new EmbedBuilder()
                        .WithAuthor(x =>
                        {
                            x.Name = Context.Client.CurrentUser.Username;
                            x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        })
                        .WithColor(new Color(102, 255, 255))
                        .WithDescription($"Ping Result: **{time} ms**");
                    await ReplyAsync("", embed: embed);
                }
            }
            catch (Exception e)
            { await ReplyAsync(e.Message); }
        }
    }
}