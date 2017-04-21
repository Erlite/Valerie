using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Diagnostics;
using Rick.Handlers;
using System.Collections.Generic;
using System.Text;
using Discord.Addons.InteractiveCommands;
using System.Linq;
using Newtonsoft.Json;

namespace Rick.Modules
{
    public class GeneralModule : ModuleBase
    {
        private InteractiveService Interactive;
        private ConfigHandler Config;

        public GeneralModule(InteractiveService Inter, ConfigHandler ConfigHandler)
        {
            Interactive = Inter;
            Config = ConfigHandler;
        }

        [Command("GuildInfo"), Summary("Normal Command"), Remarks("Displays information about a guild"), Alias("Gi")]
        public async Task GuildInfoAsync()
        {
            var embed = new EmbedBuilder();
            var gld = Context.Guild;
            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embed.ThumbnailUrl = gld.IconUrl;
            var GuildID = gld.Id;
            var GuildOwner = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            var GuildDefault = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention;
            var GuildVoice = gld.VoiceRegionId;
            var GuildCreated = gld.CreatedAt;
            var GuildAvailable = gld.Available;
            var GuildNotification = gld.DefaultMessageNotifications;
            var GuildEmbed = gld.IsEmbeddable;
            var GuildMfa = gld.MfaLevel;
            var GuildRoles = gld.Roles;
            var GuildVeri = gld.VerificationLevel;
            embed.Color = new Color(153, 30, 87);
            embed.Title = $"{gld.Name} Information";
            embed.Description = $"**Guild ID: **{GuildID}\n**Guild Owner: **{GuildOwner}\n**Default Channel: **{GuildDefault}\n**Voice Region: **{GuildVoice}\n**Created At: **{GuildCreated}\n**Available? **{GuildAvailable}\n" +
                $"**Default Msg Notif: **{GuildNotification}\n**Embeddable? **{GuildEmbed}\n**MFA Level: **{GuildMfa}\n**Verification Level: **{GuildVeri}\n";
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

            var embed = new EmbedBuilder();
            embed.Title = "Role";

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

        [Command("Gif"), Summary("Gif Cute kittens"), Remarks("Searches gif for your Gifs??")]
        public async Task GifsAsync([Remainder] string keywords = null)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                throw new NullReferenceException("Please enter what you are trying to search for!");

            var getUrl = new Uri("http://api.giphy.com/");
            using (var client = new HttpClient())
            {
                client.BaseAddress = getUrl;
                var response = await client.GetAsync(Uri.EscapeDataString($"v1/gifs/random?api_key=dc6zaTOxFJmzC&tag={Uri.UnescapeDataString(keywords)}"));
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(jsonResponse);

                var embed = new EmbedBuilder();
                embed.Author = new EmbedAuthorBuilder()
                {
                    Name = $"{Context.User.Username} searched for {keywords}",
                    IconUrl = Context.User.GetAvatarUrl()
                };
                embed.ImageUrl = obj["data"]["image_original_url"].ToString();
                embed.Color = new Color(153, 30, 87);

                await ReplyAsync("", false, embed);
            }
        }

        [Command("Urban"), Summary("Urban IE"), Remarks("Searches urban dictionary for your word")]
        public async Task UrbanAsync([Remainder] string urban = null)
        {
            if (string.IsNullOrWhiteSpace(urban))
                throw new NullReferenceException("A search term should be provided for me to search!");
            var embed = new EmbedBuilder();
            var vc = new HttpClient();
            embed.WithAuthor(x =>
            {
                x.Name = "Urban Dictionary";
                x.WithIconUrl("https://lh3.googleusercontent.com/4hpSJ4pAfwRUg-RElZ2QXNh_pV01Z96iJGT2BFuk_RRsNc-AVY7cZhbN2g1zWII9PBQ=w170");
            });
            string req = await vc.GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + urban);
            embed.WithColor(new Color(153, 30, 87));

            MatchCollection col = Regex.Matches(req, @"(?<=definition"":"")[ -z~-🧀]+(?="",""permalink)");
            MatchCollection col2 = Regex.Matches(req, @"(?<=example"":"")[ -z~-🧀]+(?="",""thumbs_down)");
            if (col.Count == 0)
            {
                await ReplyAsync("Couldn't find anything dammit");
                return;
            }
            Random r = new Random();
            string outpt = "Failed fetching embed from Urban Dictionary, please try later!";
            string outpt2 = "No Example";
            int max = r.Next(0, col.Count);
            for (int i = 0; i <= max; i++)
            {
                outpt = urban + "\r\n\r\n" + col[i].Value;
            }

            for (int i = 0; i <= max; i++)
            {
                outpt2 = "\r\n\r\n" + col2[i].Value;
            }

            outpt = outpt.Replace("\\r", "\r");
            outpt = outpt.Replace("\\n", "\n");
            outpt2 = outpt2.Replace("\\r", "\r");
            outpt2 = outpt2.Replace("\\n", "\n");

            embed.AddField(x =>
            {
                x.Name = $"Definition";
                x.Value = outpt;
            });

            embed.AddField(x =>
            {
                x.Name = "Example";
                x.Value = outpt2;
            });

            await ReplyAsync("", embed: embed);
        }

        [Command("Image"), Summary("Image rick and morty"), Remarks("Searches Bing for your image.")]
        public async Task ImageAsync([Remainder] string search = null)
        {
            if (string.IsNullOrWhiteSpace(search))
                throw new NullReferenceException("A search term should be provided for me to search!");
            using (var httpClient = new HttpClient())
            {
                var link = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?q={search}&count=10&offset=0&mkt=en-us&safeSearch=Moderate";
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Config.BingAPIKey);
                var res = await httpClient.GetAsync(link);
                if (!res.IsSuccessStatusCode)
                {
                    await ReplyAsync($"An error occurred: {res.ReasonPhrase}");
                    return;
                }
                JObject result = JObject.Parse(await res.Content.ReadAsStringAsync());
                JArray arr = (JArray)result["value"];
                if (arr.Count == 0)
                {
                    await ReplyAsync("No results found.");
                    return;
                }
                JObject image = (JObject)arr[0];
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = $"Search Term:   {search.ToUpper()}";
                        x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                        x.Url = (string)image["contentUrl"];
                    })
                    .WithColor(new Color(66, 244, 191))
                    .WithImageUrl((string)image["contentUrl"]);
                await ReplyAsync("", embed: embed);
            }
        }

        [Command("Lmgtfy"), Summary("Lmgtfy How To Google"), Remarks("Googles something for that special person who is crippled")]
        public async Task LmgtfyAsync([Remainder] string search = "How to use Lmgtfy")
        {
            await ReplyAsync($"**Your special URL: **<http://lmgtfy.com/?q={ Uri.EscapeUriString(search) }>");
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
            if (Color1 > 256 || Color2 > 256 || Color3 > 256) return;
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
                .WithColor(new Color(255,255,255))
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

        [Command("Catfacts"), Summary("Catfacts"), Remarks("Catfacts for cat lovers")]
        public async Task CatfactsAsync()
        {
            using (var http = new HttpClient())
            {
                var response = await http.GetStringAsync("http://catfacts-api.appspot.com/api/facts");
                if (response == null)
                    return;
                var fact = JObject.Parse(response)["facts"][0].ToString();
                await ReplyAsync($":feet: {fact}");
            }
        }

        //[Command("Gift"), Summary("Gift @Username 10"), Remarks("Gifts user X amount of monei")]
        //[Cooldown(60)]
        //public async Task GiftAsync(IGuildUser user, double points)
        //{
        //    if (user.Id == Context.Client.CurrentUser.Id) return;
        //    if (user == Context.User)
        //        await ReplyAsync("Can't gift yourself nub");
        //    else
        //    {
        //        var config = await new GiftsHandler(user.Id).Maintain<GiftsHandler>();
        //        uint givePoints = points > uint.MaxValue ? uint.MaxValue : (uint)points;
        //        config.GivePoints(user.Guild.Id, givePoints);
        //        await ReplyAsync($"Gifted {givePoints} XP to {user.Username}");
        //    }
        //}

        //[Command("Top"), Summary("Normal Command"), Remarks("Shows the top 10 rich people")]
        //public async Task WealthAsync()
        //{
        //    var configs = await GiftsHandler.GetAll();
        //    var filtered = configs.Where(x => x.XP.ContainsKey(Context.Guild.Id)).OrderByDescending(x => x.XP[Context.Guild.Id]).Take(11);
        //    await ReplyAsync($"Showing top 10 wealthiest people\n\n" +
        //                    string.Join("\n", filtered.Select(x => $"{Format.Bold(Context.Guild.GetUserAsync(x.UID).ToString() ?? "Not found")} with `{x.XP[Context.Guild.Id]}` XP")));
        //}


        //[Command("ResponseList"), Summary("Just run the command"), Remarks("Lists all the responses saved in the JSON file."), Alias("RL")]
        //public async Task ResponseListAsync()
        //{
        //    var guild = Context.Guild as SocketGuild;
        //    var resp = ar.LoadResponsesAsync(guild.Id);
        //    StringBuilder list = new StringBuilder();
        //    var embed = new EmbedBuilder()
        //        .WithAuthor(x =>
        //        {
        //            x.Name = Context.Client.CurrentUser.Username;
        //            x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
        //        })
        //        .WithColor(new Color(109, 242, 122))
        //        .WithTitle($"**Total Responses:** {resp.Count}");
        //    foreach (var values in resp)
        //    {
        //        embed.AddField(x =>
        //        {
        //            x.Name = values.Key;
        //            x.Value = values.Value;
        //            x.IsInline = true;
        //        });
        //    }
        //    await ReplyAsync("", embed: embed);
        //}

        //[Command("Response", RunMode = RunMode.Async), Summary("Normal Command"), Remarks("Uses Interactiveactive command to create a new response for you")]
        //public async Task ResponseAsync()
        //{
        //    await ReplyAsync("**What is the name of your response?** _'cancel' to cancel_");
        //    var nameResponse = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(10));
        //    if (nameResponse.Content == "cancel") return;
        //    string name = nameResponse.Content;

        //    await ReplyAsync("**Enter the response body:** _'cancel' to cancel_");
        //    var contentResponse = await Interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(10));
        //    if (contentResponse.Content == "cancel") return;
        //    string response = contentResponse.Content;

        //    Context.MainHandler.GuildResponseHandlerAsync(Context.Guild).CreateResponse(name, response, Context.User);
        //    var resp = ar.LoadResponsesAsync();
        //    if (!(Context.MainHandler.GuildResponseHandlerAsync(Context.Guild).ContainsResponse(name)))
        //    {
        //        var embed = new EmbedBuilder()
        //            .WithAuthor(x => { x.Name = "New response added!"; x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); })
        //            .WithDescription($"**Response Trigger:** {name}\n**Response: **{response}")
        //            .WithColor(new Color(109, 242, 122));
        //        await ReplyAsync("", embed: embed);
        //    }
        //    else
        //        await ReplyAsync("I wasn't able to add the response to the response list! :x:");
        //}
    }
}