using System.Threading.Tasks;
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

namespace GPB.Modules
{
    public class GeneralModule : ModuleBase
    {
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

        [Command("Urban")]
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

        [Command("Ping")]
        public async Task PingAsync()
        {
            var sw = Stopwatch.StartNew();
            var client = Context.Client as DiscordSocketClient;
            var Gateway = client.Latency;
            var embed = new EmbedBuilder()
                .WithTitle("Ping Results")
                .WithDescription($"**Gateway Latency:** { Gateway}" +
                            $"\n**Response Latency:** {sw.ElapsedMilliseconds} ms" +
                            $"\n**Delta:** {sw.ElapsedMilliseconds - Gateway} ms" )
                .WithColor(new Color(244, 66, 125));
            var reply = await ReplyAsync("", false, embed.Build());

            await ReplyAsync("", embed: embed);

        }

        [Command("Gift")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Gift(double points)
        {
            var guild = Context.Guild;
            var configs = await GiftsHandler.GetAll();
            uint givePoints = points > uint.MaxValue ? uint.MaxValue : (uint)points;
            foreach (var config in configs)
            {
                config.GivePoints(Context.Guild.Id, givePoints);
            }
            await ReplyAsync($"Gifted {points} XP to {configs.Count} users.");
        }

        [Command("Gift")]
        [Cooldown(60)]
        public async Task Gift(IGuildUser user, double points)
        {
            if (points <= 0)
                await ReplyAsync("You must gibe some monei");
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

        [Command("Wealth")]
        public async Task Wealth()
        {
            var configs = await GiftsHandler.GetAll();
            var filtered = configs
                .Where(x => x.XP.ContainsKey(Context.Guild.Id))
                .OrderByDescending(x => x.XP[Context.Guild.Id])
                .Take(11);
            await ReplyAsync($"Showing top 10 wealthiest people\n\n" +
                            string.Join("\n", filtered.Select(x => $"{Format.Bold(Context.Guild.GetUserAsync(x.UID).ToString() ?? "Not found")} with `{x.XP[Context.Guild.Id]}` XP")));
        }
    }
}