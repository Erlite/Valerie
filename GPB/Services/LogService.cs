using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Discord;
using Discord.WebSocket;
using GPB.Handlers;

namespace GPB.Services
{
    public class LogService
    {
        private DiscordSocketClient _client;
        private ConfigHandler Config;

        public ulong ServerLogChannelId { get; set; }
        public ulong ModLogChannelId { get; set; }
        public bool JoinsLogged { get; private set; }
        public bool LeavesLogged { get; private set; }
        public bool NameChangesLogged { get; private set; }
        public bool NickChangesLogged { get; private set; }
        public bool UserBannedLogged { get; private set; }


        #region Server Log Command Methods
        public void EnableJoinLogging()
        {
            _client.UserJoined += _client_UserJoined;
            JoinsLogged = true;
        }

        public void DisableJoinLogging()
        {
            _client.UserJoined -= _client_UserJoined;
            JoinsLogged = false;
        }

        public void EnableLeaveLogging()
        {
            _client.UserLeft += _client_UserLeft;
            LeavesLogged = true;
        }

        public void DisableLeaveLogging()
        {
            _client.UserLeft -= _client_UserLeft;
            LeavesLogged = false;
        }

        public void EnableNameChangeLogging()
        {
            _client.UserUpdated += _client_UserUpdated_NameChange;
            NameChangesLogged = true;
        }

        public void DisableNameChangeLogging()
        {
            _client.UserUpdated -= _client_UserUpdated_NameChange;
            NameChangesLogged = false;
        }

        public void EnableNickChangeLogging()
        {
            _client.GuildMemberUpdated += _client_GuildMemberUpdated_NickChange;
            NickChangesLogged = true;
        }

        public void DisableNickChangeLogging()
        {
            _client.GuildMemberUpdated -= _client_GuildMemberUpdated_NickChange;
            NickChangesLogged = false;
        }

        public void EnableUserBannedLogging()
        {
            _client.UserBanned += _client_UserBanned;
            UserBannedLogged = true;
        }

        public void DisableUserBannedLogging()
        {
            _client.UserBanned -= _client_UserBanned;
            UserBannedLogged = false;
        }


        #endregion

        public async Task LogServerMessageAsync(string message)
        {
            if (ServerLogChannelId == 0) return;
            var channel = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await channel.SendMessageAsync(message);
        }

        public async Task LogModMessageAsync(string message)
        {
            if (ModLogChannelId == 0) return;
            var channel = _client.GetChannel(ModLogChannelId) as ITextChannel;
            await channel.SendMessageAsync(message);
        }

        public async Task<bool> SaveConfigurationAsync()
        {
            var config = new LogHandler(this);

            var serializedConfig = JsonConvert.SerializeObject(config);

            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json")))
            {
                using (var configWriter = new StreamWriter(configStream))
                {
                    await configWriter.WriteAsync(serializedConfig);
                    return true;
                }
            }
        }

        public async Task<bool> LoadConfigurationAsync()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json"))) return false;

            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json")))
            {
                using (var configReader = new StreamReader(configStream))
                {
                    var serializedConfig = await configReader.ReadToEndAsync();
                    var config = JsonConvert.DeserializeObject<LogHandler>(serializedConfig);
                    if (config == null) return false;

                    ServerLogChannelId = config.ServerLog;
                    JoinsLogged = config.JoinsLogged;
                    if (JoinsLogged) EnableJoinLogging();
                    LeavesLogged = config.LeavesLogged;
                    if (LeavesLogged) EnableLeaveLogging();
                    NameChangesLogged = config.NameChangesLogged;
                    if (NameChangesLogged) EnableNameChangeLogging();
                    NickChangesLogged = config.NickChangesLogged;
                    if (NickChangesLogged) EnableNickChangeLogging();
                    UserBannedLogged = config.UserBannedLogged;
                    if (UserBannedLogged) EnableUserBannedLogging();

                    return true;
                }
            }
        }

        #region Server Log Event Handlers

        private async Task _client_UserJoined(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{u.Username}#{u.Discriminator}\n{Config.WelcomeMessage}";
            embed.Color = new Color(83, 219, 207);
            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await LogServer.SendMessageAsync("", embed: embed);
        }

        private async Task _client_UserLeft(SocketGuildUser u)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Left ===";
            embed.Description = $"{u.Username}#{u.Discriminator} has left the server! :wave";
            embed.Color = new Color(223, 229, 48);
            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await LogServer.SendMessageAsync("", embed: embed);
        }

        private async Task _client_UserUpdated_NameChange(SocketUser author, SocketUser a)
        {
            if (author.Username == a.Username) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Username Change ====";
            embed.Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await LogServer.SendMessageAsync("", embed: embed);
        }

        private async Task _client_GuildMemberUpdated_NickChange(SocketGuildUser author, SocketGuildUser a)
        {
            if (author.Nickname == a.Nickname) return;
            var embed = new EmbedBuilder();
            embed.Title = "=== Nickname Change ====";
            embed.Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}";
            embed.Color = new Color(193, 60, 144);
            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
            await LogServer.SendMessageAsync("", embed: embed);
        }

        private async Task _client_UserBanned(SocketUser user, SocketGuild gld)
        {
            await LogServerMessageAsync($"{user.Username + user.Discriminator} was banned from from {gld.Name}");
        }
        #endregion

        public LogService(DiscordSocketClient c, ConfigHandler config)
        {
            _client = c;
            Config = config;
        }
    }
}