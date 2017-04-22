using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Models;

namespace Rick.Services
{
    public class LogService
    {
        private DiscordSocketClient client;
        private GuildModel GuildModel;

        public LogService(DiscordSocketClient c, GuildModel gldhndler)
        {
            client = c;
            GuildModel = gldhndler;
        }

        public void EnableJoinLogging()
        {
            client.UserJoined += UserJoinedAsync;
        }

        public void DisableJoinLogging()
        {
            client.UserJoined -= UserJoinedAsync;
        }

        public void EnableLeaveLogging()
        {
            client.UserLeft += UserLeftAsync;
        }

        public void DisableLeaveLogging()
        {
            client.UserLeft += UserLeftAsync;
        }

        public void EnableNameChangeLogging()
        {
            client.UserUpdated += NameChangeAsync;
        }

        public void DisableNameChangeLogging()
        {
            client.UserUpdated += NameChangeAsync;
        }

        public void EnableNickChangeLogging()
        {
            client.GuildMemberUpdated += NickChangeAsync;
        }

        public void DisableNickChangeLogging()
        {
            client.GuildMemberUpdated -= NickChangeAsync;
        }

        public void EnableUserBannedLogging()
        {
            client.UserBanned += UserBannedAsync;
        }

        public void DisableUserBannedLogging()
        {
            client.UserBanned -= UserBannedAsync;
        }

        //public void EnableLatencyMonitor()
        //{
        //    client.LatencyUpdated += LatencyUpdateAsync;
        //    GuildModel.ClientLatency = true;
        //}

        //public void DisableLatencyMonitor()
        //{
        //    client.LatencyUpdated -= LatencyUpdateAsync;
        //    GuildModel.ClientLatency = false;
        //}

        public void EnableAutoRespond()
        {
            client.MessageReceived += MessageReceivedAsync;
        }

        public void DisableAutoRespond()
        {
            client.MessageReceived -= MessageReceivedAsync;
        }


        private async Task UserJoinedAsync(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{u.Username}#{u.Discriminator}";
            embed.Color = new Color(83, 219, 207);
            var LogChannel = client.GetChannel(GuildModel.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task UserLeftAsync(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Left ===";
            embed.Description = $"{u.Username}#{u.Discriminator} has left the server! :wave:";
            embed.Color = new Color(223, 229, 48);
            var LogChannel = client.GetChannel(GuildModel.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NameChangeAsync(SocketUser author, SocketUser a)
        {
            if (author.Username == a.Username) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Username Change ====";
            embed.Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogChannel = client.GetChannel(GuildModel.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NickChangeAsync(SocketGuildUser author, SocketGuildUser a)
        {
            if (author.Nickname == a.Nickname) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Nickname Change ====";
            embed.Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogChannel = client.GetChannel(GuildModel.ModChannelID) as ITextChannel;
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
            //if (msg.Author.Id == client.CurrentUser.Id) return;
            //var response = GuildModel.GuildConfigs[Guild.Id].Responses;
            //foreach (KeyValuePair<string, string> item in response.Where(x => x.Key.Contains(msg.ToString())))
            //{
            //    if (msg.Content.Contains(item.Key))
            //    {
            //        await msg.Channel.SendMessageAsync(item.Value);
            //    }
            //}
        }

    }
}