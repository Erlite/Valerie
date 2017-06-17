using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using System;
using System.Threading.Tasks;
using Rick.Enums;
using Rick.Models;
using System.Linq;

namespace Rick.Services
{
    public class EventService
    {
        private static DiscordSocketClient client;
        private static Random Random = new Random();

        public EventService(DiscordSocketClient c)
        {
            client = c;
        }

        public static void EnableJoinLogging()
        {
            client.UserJoined += UserJoinedAsync;
        }

        public static void DisableJoinLogging()
        {
            client.UserJoined -= UserJoinedAsync;
        }

        public static void EnableLeaveLogging()
        {
            client.UserLeft += UserLeftAsync;
        }

        public static void DisableLeaveLogging()
        {
            client.UserLeft -= UserLeftAsync;
        }

        public static void EnableLatencyMonitor()
        {
            client.LatencyUpdated += LatencyUpdateAsync;
        }

        public static void DisableLatencyMonitor()
        {
            client.LatencyUpdated -= LatencyUpdateAsync;
        }

        static async Task UserJoinedAsync(SocketGuildUser user)
        {
            var getGuild = GuildHandler.GuildConfigs[user.Guild.Id];
            if (!getGuild.JoinEvent.IsEnabled) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== User Joined ===",
                Description = $"**Username: **{user.Username}#{user.Discriminator}\n{getGuild.WelcomeMessage}",
                Color = new Color(83, 219, 207)
            };
            if (getGuild.JoinEvent.TextChannel != 0)
            {
                var LogChannel = client.GetChannel(getGuild.JoinEvent.TextChannel) as ITextChannel;
                await LogChannel.SendMessageAsync("", embed: embed);
            }
            else
                await user.Guild.DefaultChannel.SendMessageAsync("", embed: embed);
        }

        static  async Task UserLeftAsync(SocketGuildUser user)
        {
            var getGuild = GuildHandler.GuildConfigs[user.Guild.Id];
            if (!getGuild.LeaveEvent.IsEnabled) return;
            var embed = new EmbedBuilder()
            {
                Title = "=== User Left ===",
                Description = $"{user.Username}#{user.Discriminator} has left the server! :wave:",
                Color = new Color(223, 229, 48)
            };
            if (getGuild.LeaveEvent.TextChannel != 0)
            {
                var LogChannel = client.GetChannel(getGuild.LeaveEvent.TextChannel) as ITextChannel;
                await LogChannel.SendMessageAsync("", embed: embed);
            }
            else
                await user.Guild.DefaultChannel.SendMessageAsync("", embed: embed);
        }

        static async Task LatencyUpdateAsync(int older, int newer)
        {
            if (client == null) return;
            var newStatus = (client.ConnectionState == ConnectionState.Disconnected || newer > 100) ? UserStatus.DoNotDisturb
                    : (client.ConnectionState == ConnectionState.Connecting || newer > 60)
                        ? UserStatus.Idle
                        : UserStatus.Online;

            await client.SetStatusAsync(newStatus);
        }

        public static async Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            var CreateConfig = new GuildModel();
            GuildHandler.GuildConfigs.Add(Guild.Id, CreateConfig);
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        public static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            string Prefix = BotHandler.BotConfig.DefaultPrefix;
            string Msg = $"Hello! I'm {client.CurrentUser.Username} written by ExceptionDev!\n" +
                $"For commands list please type `{Prefix}Commands or {Prefix}Cmds`. If you need info on how to use a command please type `{Prefix}Help CommandName`\n" +
                $"Github: https://github.com/ExceptionDev/Rick\n" +
                $"Please help support this Bot's development by leaving a star on the repo! That would be great! " +
                $"If you come across any issues please feel free to join my guild: `https://discord.me/Noegenesis` or open a new issue on the repo! Thank you for choosing me!";
            await (await Guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync(Msg);
            await client.StopAsync();
            await Task.Delay(1000);
            await client.StartAsync();
        }

        public static async Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            Logger.Log(LogType.Warning, LogSource.Client, $"{Guild.Name} Config's deleted!");
            if (GuildHandler.GuildConfigs.ContainsKey(Guild.Id))
            {
                GuildHandler.GuildConfigs.Remove(Guild.Id);
            }
            var path = GuildHandler.configPath;
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }

        public static async Task MessageServicesAsync(SocketMessage msg)
        {
            
            var gld = (msg.Channel as SocketGuildChannel).Guild;
            var message = msg as SocketUserMessage;
            await MsgsService.AddToMessage(message).ConfigureAwait(false);
            await Task.Run(() => MsgsService.AfkAsync(message, gld)).ConfigureAwait(false);
            await Task.Run(() => MsgsService.ChatKarmaAsync(message, gld)).ConfigureAwait(false);
            await Task.Run(() => MsgsService.CleverBot(message, gld)).ConfigureAwait(false);
        }

        public static async Task OnReadyAsync()
        {
            var GameCount = BotHandler.BotConfig.Games.Count;
            var GetGame = BotHandler.BotConfig.Games[Random.Next(GameCount)];
            if (GameCount != 0)
            {
                await client.SetGameAsync(GetGame);
                Logger.Log(LogType.Info, LogSource.Client, $"Current Game: {GetGame}");
            }
            else
                await client.SetGameAsync($"{BotHandler.BotConfig.DefaultPrefix}About");
            Logger.Log(LogType.Info, LogSource.Client, $"Total Guilds: {client.Guilds.Count}");
            Logger.Log(LogType.Info, LogSource.Client, $"Total Users: {client.Guilds.Sum(x => x.Users.Count)}");
            Logger.Log(LogType.Info, LogSource.Client, $"Total Channels: {client.Guilds.Sum(x => x.Channels.Count)}");
        }

        public static async Task HandleGuildsTasks(SocketGuildUser User)
        {
            var GC = GuildHandler.GuildConfigs[User.Guild.Id];

            if (GC.Karma.ContainsKey(User.Id))
            {
                GC.Karma.Remove(User.Id);
                Logger.Log(LogType.Warning, LogSource.Configuration, $"{User.Username} removed from {User.Guild.Name}'s Karma List.");
            }
            foreach (var tag in GC.TagsList)
            {
                if (tag.OwnerId == User.Id)
                {
                    GC.TagsList.Remove(tag);
                    Logger.Log(LogType.Warning, LogSource.Configuration, $"Removed {tag.TagName} by {User.Username}.");
                }
            }
            await GuildHandler.SaveAsync(GuildHandler.GuildConfigs);
        }
    }
}