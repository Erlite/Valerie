using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Rick.Handlers;

namespace Rick.Services
{
    public class EventService
    {
        private static DiscordSocketClient client;

        public EventService(DiscordSocketClient c)
        {
            client = c;
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
            client.UserLeft -= UserLeftAsync;
        }

        public void EnableNameChangeLogging()
        {
            client.UserUpdated += NameChangeAsync;
        }

        public void DisableNameChangeLogging()
        {
            client.UserUpdated -= NameChangeAsync;
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
            if (!getGuild.JoinLogs) return;
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
            var getGuild = GuildHandler.GuildConfigs[user.Guild.Id];
            if (!getGuild.LeaveLogs) return;
                var embed = new EmbedBuilder()
                {
                    Title = "=== User Left ===",
                    Description = $"{user.Username}#{user.Discriminator} has left the server! :wave:",
                    Color = new Color(223, 229, 48)
                };
                var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
                await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NameChangeAsync(SocketUser author, SocketUser a)
        {
            var SockGuildUser = author as SocketGuildUser;
            var Guild = SockGuildUser.Guild;
            var getGuild = GuildHandler.GuildConfigs[Guild.Id];
            if (author.Username == a.Username || !getGuild.NameChangesLogged) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== Username Change ====",
                Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}",
                Color = new Color(193, 60, 144)
            };
            var LogChannel = client.GetChannel(getGuild.ModChannelID) as ITextChannel;
            await LogChannel.SendMessageAsync("", embed: embed);
        }

        private async Task NickChangeAsync(SocketGuildUser author, SocketGuildUser a)
        {
            var getGuild = GuildHandler.GuildConfigs[author.Guild.Id];
            if (author.Nickname == a.Nickname || !getGuild.NickChangesLogged) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== Nickname Change ====",
                Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}",
                Color = new Color(193, 60, 144)
            };
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
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        public async static Task JoinedGuildAsync(SocketGuild Guild)
        {
            string Prefix = BotHandler.BotConfig.DefaultPrefix;
            string Msg = $"Hello! I'm {client.CurrentUser.Username} written by ExceptionDev!\n" +
                $"For commands list please type `{Prefix}Commands or {Prefix}Cmds`. If you need info on how to use a command please type `{Prefix}Help CommandName`\n" +
                $"Github: https://github.com/ExceptionDev/Rick\n" +
                $"Please help support this Bot's development by leaving a star on the repo! That would be great! " +
                $"If you come across any issues please feel free to join my guild: `https://discord.me/Noegenesis` or open a new issue on the repo! Thank you for choosing me!";
            await (await Guild.Owner.CreateDMChannelAsync()).SendMessageAsync(Msg);
            //GuildHandler.RestartApp();
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

        public async static Task MessageServicesAsync(SocketMessage msg)
        {
            var gld = (msg.Channel as SocketGuildChannel).Guild;
            var message = msg as SocketUserMessage;
            await Task.Run(() => MsgsService.AfkAsync(message, gld));
            await Task.Run(() => MsgsService.ChatKarmaAsync(message, gld));
            await MsgsService.CleverBotAsync(message, gld);
        }

        public async static Task OnReadyAsync()
        {
            await client.SetGameAsync(BotHandler.BotConfig.BotGame);
        }
    }
}