using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace Rick.Services
{
    public class LogService
    {
        private DiscordSocketClient client;
        private ConfigHandler Config;
        private GuildHandler GuildHandler;

        public const string DictPath = "./Config/Response.json";
        private Regex _issueRegex = new Regex(@">>([0-9]+)");

        public LogService(DiscordSocketClient c, ConfigHandler config, GuildHandler gldhndler)
        {
            client = c;
            Config = config;
            GuildHandler = gldhndler;
        }

        public static Dictionary<string, string> GetResponses()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(DictPath));
        }

        public void EnableJoinLogging()
        {
            client.UserJoined +=  UserJoinedAsync ;
            GuildHandler.JoinLogs = true;
        }

        public void DisableJoinLogging()
        {
            client.UserJoined -= UserJoinedAsync;
            GuildHandler.JoinLogs = false;
        }

        public void EnableLeaveLogging()
        {
            client.UserLeft += UserLeftAsync;
            GuildHandler.LeaveLogs = true;
        }

        public void DisableLeaveLogging()
        {
            client.UserLeft += UserLeftAsync;
            GuildHandler.LeaveLogs = false;
        }

        public void EnableNameChangeLogging()
        {
            client.UserUpdated += NameChangeAsync;
            GuildHandler.NameChangesLogged = true;
        }

        public void DisableNameChangeLogging()
        {
            client.UserUpdated += NameChangeAsync;
            GuildHandler.NameChangesLogged = false;
        }

        public void EnableNickChangeLogging()
        {
            client.GuildMemberUpdated += NickChangeAsync;
            GuildHandler.NickChangesLogged = true;
        }

        public void DisableNickChangeLogging()
        {
            client.GuildMemberUpdated -= NickChangeAsync;
            GuildHandler.NickChangesLogged = false;
        }

        public void EnableUserBannedLogging()
        {
            client.UserBanned += UserBannedAsync;
            GuildHandler.UserBannedLogged = true;
        }

        public void DisableUserBannedLogging()
        {
            client.UserBanned -= UserBannedAsync;
            GuildHandler.UserBannedLogged = false;
        }

        public void EnableLatencyMonitor()
        {
            client.LatencyUpdated += LatencyUpdateAsync;
            GuildHandler.ClientLatency = true;
        }

        public void DisableLatencyMonitor()
        {
            client.LatencyUpdated -= LatencyUpdateAsync;
            GuildHandler.ClientLatency = false;
        }

        public void EnableMessageRecieve()
        {
            client.MessageReceived += MessageReceivedAsync;
            GuildHandler.MessageRecieve = true;
        }

        public void DisableMessageRecieve()
        {
            client.MessageReceived -= MessageReceivedAsync;
            GuildHandler.MessageRecieve = false;
        }
   

        private async Task UserJoinedAsync(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{u.Username}#{u.Discriminator}";
            embed.Color = new Color(83, 219, 207);
            var LogChannel = client.GetChannel(GuildHandler.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task UserLeftAsync(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Left ===";
            embed.Description = $"{u.Username}#{u.Discriminator} has left the server! :wave:";
            embed.Color = new Color(223, 229, 48);
            var LogChannel = client.GetChannel(GuildHandler.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NameChangeAsync(SocketUser author, SocketUser a)
        {
            if (author.Username == a.Username) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Username Change ====";
            embed.Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogChannel = client.GetChannel(GuildHandler.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NickChangeAsync(SocketGuildUser author, SocketGuildUser a)
        {
            if (author.Nickname == a.Nickname) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Nickname Change ====";
            embed.Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogChannel = client.GetChannel(GuildHandler.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task UserBannedAsync(SocketUser user, SocketGuild gld)
        {
            await gld.DefaultChannel.SendMessageAsync($"{user.Username} was banned from {gld.Name}");
        }

        private async Task LatencyUpdateAsync(int older, int newer)
        {
            if (client == null) return;
            var newStatus = (client.ConnectionState == ConnectionState.Disconnected || newer > 100) ? UserStatus.DoNotDisturb
                    : (client.ConnectionState == ConnectionState.Connecting || newer > 60)
                        ? UserStatus.Idle
                        : UserStatus.Online;

            await client.SetStatusAsync(newStatus);
        }

        private async Task MessageReceivedAsync(SocketMessage msg)
        {
            if (msg.Author.Id == client.CurrentUser.Id) return;
            var response = GetResponses();
            foreach (KeyValuePair<string, string> item in response.Where(x => x.Key.Contains(msg.ToString())))
            {
                if (msg.Content.Contains(item.Key))
                {
                    await msg.Channel.SendMessageAsync(item.Value);
                }
            }

            var matches = _issueRegex.Matches(msg.Content);
            if (matches.Count > 0)
            {
                var outStr = new StringBuilder();
                foreach (Match match in matches)
                {
                    outStr.AppendLine($"**{match.Value}** - https://github.com/ExceptionDev/DiscordExampleBot/issues/{match.Value.Substring(2)}");
                }
                await msg.Channel.SendMessageAsync(outStr.ToString());
            }
        }

    }
}