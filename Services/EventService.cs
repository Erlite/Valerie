using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using System.Linq;
using System;
using System.IO;

namespace Rick.Services
{
    public class EventService
    {
        private static DiscordSocketClient client;
        private GuildHandler GuildModel;

        public EventService(DiscordSocketClient c, GuildHandler gldhndler)
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
            var getGuild = GuildHandler.GuildConfigs[user.Guild.Id];
            var embed = new EmbedBuilder()
            {
                Title = "=== User Joined ===",
                Description = $"**Username: **{user.Username}#{user.Discriminator}\n{getGuild.WelcomeMessage}",
                Color = new Color(83, 219, 207)
            };
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            var embed = new EmbedBuilder()
            {
                Title = "=== User Left ===",
                Description = $"{user.Username}#{user.Discriminator} has left the server! :wave:",
                Color = new Color(223, 229, 48)
            };
            var getGuild = GuildHandler.GuildConfigs[user.Guild.Id];
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NameChangeAsync(SocketUser author, SocketUser a)
        {
            var SockGuildUser = author as SocketGuildUser;
            var Guild = SockGuildUser.Guild;
            if (author.Username == a.Username) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== Username Change ====",
                Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}",
                Color = new Color(193, 60, 144)
            };
            var getGuild = GuildHandler.GuildConfigs[Guild.Id];
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NickChangeAsync(SocketGuildUser author, SocketGuildUser a)
        {
            if (author.Nickname == a.Nickname) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== Nickname Change ====",
                Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}",
                Color = new Color(193, 60, 144)
            };
            var getGuild = GuildHandler.GuildConfigs[author.Guild.Id];
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

        public async static Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            var CreateConfig = new GuildHandler();
            GuildHandler.GuildConfigs.Add(Guild.Id, CreateConfig);
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs).ConfigureAwait(false);
        }

        public async static Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            ConsoleService.Log(LogSeverity.Warning, Guild.Name, "Config Deleted!");
            if (GuildHandler.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildHandler.GuildConfigs.Remove(Guild.Id);
            }
            var path = GuildHandler.configPath;
            await GuildHandler.SaveAsync(path, GuildHandler.GuildConfigs);
        }

        public async static Task OnReady()
        {
            await client.SetGameAsync(BotHandler.BotConfig.BotGame);
        }
    }
}