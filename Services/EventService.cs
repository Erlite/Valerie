using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Models;
using System.Linq;
using Rick.Handlers;

namespace Rick.Services
{
    public class EventService
    {
        private DiscordSocketClient client;
        private GuildModel GuildModel;

        public EventService(DiscordSocketClient c, GuildModel gldhndler)
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

        public void EnableAutoRespond()
        {
            client.MessageReceived += MessageReceivedAsync;
        }

        public void DisableAutoRespond()
        {
            client.MessageReceived -= MessageReceivedAsync;
        }

        public void EnableLatencyMonitor()
        {
            client.LatencyUpdated += LatencyUpdateAsync;
        }

        public void DisableLatencyMonitor()
        {
            client.LatencyUpdated -= LatencyUpdateAsync;
        }

        private async Task UserJoinedAsync(SocketGuildUser user)
        {
            var getGuild = GuildModel.GuildConfigs[user.Guild.Id];
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{user.Username}#{user.Discriminator}\n{getGuild.WelcomeMessage}";
            embed.Color = new Color(83, 219, 207);
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Left ===";
            embed.Description = $"{user.Username}#{user.Discriminator} has left the server! :wave:";
            embed.Color = new Color(223, 229, 48);
            var getGuild = GuildModel.GuildConfigs[user.Guild.Id];
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NameChangeAsync(SocketUser author, SocketUser a)
        {
            var SockGuildUser = author as SocketGuildUser;
            var Guild = SockGuildUser.Guild;
            if (author.Username == a.Username) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Username Change ====";
            embed.Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var getGuild = GuildModel.GuildConfigs[Guild.Id];
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NickChangeAsync(SocketGuildUser author, SocketGuildUser a)
        {
            if (author.Nickname == a.Nickname) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Nickname Change ====";
            embed.Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var getGuild = GuildModel.GuildConfigs[author.Guild.Id];
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
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
            if (msg.Author.IsBot) return;
            var SocChan = msg.Channel as SocketGuildChannel;
            var Guild = SocChan.Guild;            
            var GetResponses = GuildModel.GuildConfigs[Guild.Id].Responses;
            var hasValue = GetResponses.FirstOrDefault(resp => msg.Content.Contains(resp.Key));
            if (msg.Content.Contains(hasValue.Key))
            {
                await msg.Channel.SendMessageAsync(hasValue.Value);
            }
        }
    }
}