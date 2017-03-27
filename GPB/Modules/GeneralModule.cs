﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Discord.WebSocket;
using System.Diagnostics;
using GPB.Attributes;
using GPB.Handlers;
using System.Linq;
using System.Collections.Generic;
using GPB.Services;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Discord.Addons.InteractiveCommands;

namespace GPB.Modules
{
    public class GeneralModule : ModuleBase
    {
        private InteractiveService inter;
        public GeneralModule(InteractiveService inte)
        {
            inter = inte;
        }


        [Command("GuildInfo"), Summary("GI"), Remarks("Displays information about a guild")]
        public async Task GuildInfoAsync()
        {
            var embed = new EmbedBuilder();
            var gld = Context.Guild;
            if (!string.IsNullOrWhiteSpace(gld.IconUrl))
                embed.ThumbnailUrl = gld.IconUrl;
            var I = gld.Id;
            var O = gld.GetOwnerAsync().GetAwaiter().GetResult().Mention;
            var D = gld.GetDefaultChannelAsync().GetAwaiter().GetResult().Mention;
            var V = gld.VoiceRegionId;
            var C = gld.CreatedAt;
            var A = gld.Available;
            var N = gld.DefaultMessageNotifications;
            var E = gld.IsEmbeddable;
            var L = gld.MfaLevel;
            var R = gld.Roles;
            var VL = gld.VerificationLevel;
            embed.Color = new Color(153, 30, 87);
            embed.Title = $"{gld.Name} Information";
            embed.Description = $"**Guild ID: **{I}\n**Guild Owner: **{O}\n**Default Channel: **{D}\n**Voice Region: **{V}\n**Created At: **{C}\n**Available? **{A}\n" +
                $"**Default Msg Notif: **{N}\n**Embeddable? **{E}\n**MFA Level: **{L}\n**Verification Level: **{VL}\n";
            await ReplyAsync("", false, embed);
        }

        [Command("Gif"), Summary("Gif Cute kittens"), Remarks("Searches gif for your Gifs??")]
        public async Task GifsAsync([Remainder] string keywords)
        {
            if (string.IsNullOrWhiteSpace(keywords))
                throw new ArgumentException("What do you want me to search for?");

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
                throw new NullReferenceException("Please provide a search term");
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

        [Command("Ping"), Summary("Just run the command"), Remarks("Measures gateway ping and response time")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            var embed = new EmbedBuilder()
                .WithTitle("Ping Results")
                .WithDescription($"**Gateway Latency:** { Gateway} ms" +
                            $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms" +
                            $"\n**Delta:** {sw.ElapsedMilliseconds - Gateway} ms" )
                .WithColor(new Color(244, 66, 125));
            await ReplyAsync("", embed: embed);

        }

        [Command("Gift"), Summary("Gift @Username 10"), Remarks("Gifts user X amount of monei")]
        [Cooldown(60)]
        public async Task GiftAsync(IGuildUser user, double points)
        {
            if (user.Id == Context.Client.CurrentUser.Id) return;
            if (user == Context.User)
                await ReplyAsync("Can't gift yourself nub");
            else
            {
                var config = await new GiftsHandler(user.Id).Maintain<GiftsHandler>();
                uint givePoints = points > uint.MaxValue ? uint.MaxValue : (uint)points;
                config.GivePoints(user.Guild.Id, givePoints);
                await ReplyAsync($"Gifted {givePoints} XP to {user.Username}");
            }
        }

        [Command("Top"), Summary("Just run the command"), Remarks("Shows the top 10 rich people")]
        public async Task WealthAsync()
        {
            var configs = await GiftsHandler.GetAll();
            var filtered = configs.Where(x => x.XP.ContainsKey(Context.Guild.Id)).OrderByDescending(x => x.XP[Context.Guild.Id]).Take(11);
            await ReplyAsync($"Showing top 10 wealthiest people\n\n" +
                            string.Join("\n", filtered.Select(x => $"{Format.Bold(Context.Guild.GetUserAsync(x.UID).ToString() ?? "Not found")} with `{x.XP[Context.Guild.Id]}` XP")));
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

        [Command("ResponseList"), Summary("Just run the command"), Remarks("Lists all the responses saved in the JSON file."), Alias("RL")]
        public async Task ResponseListAsync()
        {
            var resp = LogService.GetResponses();
            StringBuilder list = new StringBuilder();
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = Context.Client.CurrentUser.Username;
                    x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
                .WithColor(new Color(109, 242, 122))
                .WithTitle($"**Total Responses:** {resp.Count}");
            foreach(var values in resp)
            {
                embed.AddField(x =>
                {
                    x.Name = values.Key;
                    x.Value = values.Value;
                    x.IsInline = true;
                });
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("Response", RunMode = RunMode.Async), Summary("Just run the command"), Remarks("Uses interactive command to create a new response for you")]
        public async Task ResponseAsync()
        {
            await ReplyAsync("**What is the name of your response?** _'cancel' to cancel_");
            var nameResponse = await inter.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(10));
            if (nameResponse.Content == "cancel") return;
            string name = nameResponse.Content;

            await ReplyAsync("**Enter the response body:** _'cancel' to cancel_");
            var contentResponse = await inter.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(10));
            if (contentResponse.Content == "cancel") return;
            string response = contentResponse.Content;

            var resp = LogService.GetResponses();
            if (!(resp.ContainsKey(name)))
            {
                resp.Add(name, response);
                File.WriteAllText(LogService.DictPath, JsonConvert.SerializeObject(resp, Formatting.Indented));
                var embed = new EmbedBuilder()
                    .WithAuthor(x => { x.Name = "New response added!"; x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); })
                    .WithDescription($"**Response Trigger:** {name}\n**Response: **{response}")
                    .WithColor(new Color(109, 242, 122));
                await ReplyAsync("", embed: embed);
            }
            else
                await ReplyAsync("I wasn't able to add the response to the response list! :x:");
        }
    }
}