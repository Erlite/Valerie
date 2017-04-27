using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Models;
using System.Linq;
using System;
using System.IO;

namespace Rick.Services
{
    public class EventService
    {
        private static DiscordSocketClient client;
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
            var getGuild = GuildModel.GuildConfigs[user.Guild.Id];
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
            var getGuild = GuildModel.GuildConfigs[Guild.Id];
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
            if (GuildModel.GuildConfigs[Guild.Id].AutoRespond)
            {
                var GetResponses = GuildModel.GuildConfigs[Guild.Id].Responses;
                var hasValue = GetResponses.FirstOrDefault(resp => msg.Content.Contains(resp.Key));
                if (msg.Content.Contains(hasValue.Key))
                {
                    await msg.Channel.SendMessageAsync(hasValue.Value);
                }
            }
        }

        public async static Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            var CreateConfig = new GuildModel();
            GuildModel.GuildConfigs.Add(Guild.Id, CreateConfig);
            await GuildModel.SaveAsync(GuildModel.configPath, GuildModel.GuildConfigs).ConfigureAwait(false);
        }

        public async static Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            ConsoleService.Log(LogSeverity.Warning, Guild.Name, "Config Deleted!");
            if (GuildModel.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildModel.GuildConfigs.Remove(Guild.Id);
            }
            var path = GuildModel.configPath;
            await GuildModel.SaveAsync(path, GuildModel.GuildConfigs);
        }

        public async static Task OnReady()
        {
            await client.SetGameAsync(BotModel.BotConfig.BotGame);
        }
    }
}