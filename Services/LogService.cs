//using System.IO;
//using System.Threading.Tasks;
//using Newtonsoft.Json;
//using Discord;
//using Discord.WebSocket;
//using Rick.Handlers;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Text;

//namespace Rick.Services
//{
//    public class LogService
//    {
//        private DiscordSocketClient _client;
//        public const string DictPath = "./Config/Response.json";
//        private Regex _issueRegex = new Regex(@">>([0-9]+)");

//        public ulong ServerLogChannelId { get; set; }
//        public ulong ModLogChannelId { get; set; }
//        public bool JoinsLogged { get; private set; }
//        public bool LeavesLogged { get; private set; }
//        public bool NameChangesLogged { get; private set; }
//        public bool NickChangesLogged { get; private set; }
//        public bool UserBannedLogged { get; private set; }
//        public bool ClientLatency { get; private set; }
//        public bool MessageRecieve { get; private set; }
//        public bool Starboard { get; private set; }


//        #region Server Log Command Methods
//        public void EnableJoinLogging()
//        {
//            _client.UserJoined += _client_UserJoined;
//            JoinsLogged = true;
//        }

//        public void DisableJoinLogging()
//        {
//            _client.UserJoined -= _client_UserJoined;
//            JoinsLogged = false;
//        }

//        public void EnableLeaveLogging()
//        {
//            _client.UserLeft += _client_UserLeft;
//            LeavesLogged = true;
//        }

//        public void DisableLeaveLogging()
//        {
//            _client.UserLeft -= _client_UserLeft;
//            LeavesLogged = false;
//        }

//        public void EnableNameChangeLogging()
//        {
//            _client.UserUpdated += _client_UserUpdated_NameChange;
//            NameChangesLogged = true;
//        }

//        public void DisableNameChangeLogging()
//        {
//            _client.UserUpdated -= _client_UserUpdated_NameChange;
//            NameChangesLogged = false;
//        }

//        public void EnableNickChangeLogging()
//        {
//            _client.GuildMemberUpdated += _client_GuildMemberUpdated_NickChange;
//            NickChangesLogged = true;
//        }

//        public void DisableNickChangeLogging()
//        {
//            _client.GuildMemberUpdated -= _client_GuildMemberUpdated_NickChange;
//            NickChangesLogged = false;
//        }

//        public void EnableUserBannedLogging()
//        {
//            _client.UserBanned += _client_UserBanned;
//            UserBannedLogged = true;
//        }

//        public void DisableUserBannedLogging()
//        {
//            _client.UserBanned -= _client_UserBanned;
//            UserBannedLogged = false;
//        }

//        public void EnableSmartConnection()
//        {
//            _client.LatencyUpdated += _client_LatencyUpdated;
//            ClientLatency = true;
//        }

//        public void DisableSmartConnection()
//        {
//            _client.LatencyUpdated -= _client_LatencyUpdated;
//            ClientLatency = false;
//        }

//        public void EnableMessageRecieve()
//        {
//            _client.MessageReceived += _client_MessageReceived;
//            MessageRecieve = true;
//        }

//        public void DisableMessageRecieve()
//        {
//            _client.MessageReceived -= _client_MessageReceived;
//            MessageRecieve = false;
//        }

//        public void EnableStarboard()
//        {
//            _client.ReactionAdded += _client_ReactionAdded;
//            Starboard = true;
//        }

//        public void DisableStarboard()
//        {
//            _client.ReactionAdded -= _client_ReactionAdded;
//            Starboard = false;
//        }

//        #endregion

//        #region Config Stuff
//        public async Task LogServerMessageAsync(string message)
//        {
//            if (ServerLogChannelId == 0) return;
//            var channel = _client.GetChannel(ServerLogChannelId) as ITextChannel;
//            await channel.SendMessageAsync(message);
//        }

//        public async Task LogModMessageAsync(string message)
//        {
//            if (ModLogChannelId == 0) return;
//            var channel = _client.GetChannel(ModLogChannelId) as ITextChannel;
//            await channel.SendMessageAsync(message);
//        }

//        public async Task<bool> SaveConfigurationAsync()
//        {
//            var config = new LogHandler(this);

//            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

//            using (var configStream = File.OpenWrite(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json")))
//            {
//                using (var configWriter = new StreamWriter(configStream))
//                {
//                    await configWriter.WriteAsync(serializedConfig);
//                    return true;
//                }
//            }
//        }

//        public async Task<bool> LoadConfigurationAsync()
//        {
//            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json"))) return false;

//            using (var configStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "Config", "log.json")))
//            {
//                using (var configReader = new StreamReader(configStream))
//                {
//                    var serializedConfig = await configReader.ReadToEndAsync();
//                    var config = JsonConvert.DeserializeObject<LogHandler>(serializedConfig);
//                    if (config == null) return false;

//                    ServerLogChannelId = config.ServerLog;
//                    ModLogChannelId = config.ModLog;
//                    JoinsLogged = config.JoinsLogged;
//                    if (JoinsLogged) EnableJoinLogging();
//                    LeavesLogged = config.LeavesLogged;
//                    if (LeavesLogged) EnableLeaveLogging();
//                    NameChangesLogged = config.NameChangesLogged;
//                    if (NameChangesLogged) EnableNameChangeLogging();
//                    NickChangesLogged = config.NickChangesLogged;
//                    if (NickChangesLogged) EnableNickChangeLogging();
//                    UserBannedLogged = config.UserBannedLogged;
//                    if (UserBannedLogged) EnableUserBannedLogging();
//                    ClientLatency = config.ClientLatency;
//                    if (ClientLatency) EnableSmartConnection();
//                    MessageRecieve = config.MessageRecieve;
//                    if (MessageRecieve) EnableMessageRecieve();
//                    Starboard = config.Starboard;
//                    if (Starboard) EnableStarboard();
//                    return true;
//                }
//            }
//        }

//        #endregion

//        #region Server Log Event Handlers

//        private async Task _client_UserJoined(SocketGuildUser u)
//        {
//            var embed = new EmbedBuilder();
//            embed.Title = "=== User Joined ===";
//            embed.Description = $"**Username: **{u.Username}#{u.Discriminator}\n{Config.WelcomeMessage}";
//            embed.Color = new Color(83, 219, 207);
//            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
//            await LogServer.SendMessageAsync("", embed: embed);
//        }

//        private async Task _client_UserLeft(SocketGuildUser u)
//        {
//            var embed = new EmbedBuilder();
//            embed.Title = "=== User Left ===";
//            embed.Description = $"{u.Username}#{u.Discriminator} has left the server! :wave:";
//            embed.Color = new Color(223, 229, 48);
//            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
//            await LogServer.SendMessageAsync("", embed: embed);
//        }

//        private async Task _client_UserUpdated_NameChange(SocketUser author, SocketUser a)
//        {
//            if (author.Username == a.Username) return;
//            var embed = new EmbedBuilder();
//            embed.Title = "=== Username Change ====";
//            embed.Description = $"**Old Username: **{author.Username}#{author.Discriminator}\n**New Username: **{a.Username}\n**ID: **{author.Id}";
//            embed.Color = new Color(193, 60, 144);
//            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
//            await LogServer.SendMessageAsync("", embed: embed);
//        }

//        private async Task _client_GuildMemberUpdated_NickChange(SocketGuildUser author, SocketGuildUser a)
//        {
//            if (author.Nickname == a.Nickname) return;
//            var embed = new EmbedBuilder();
//            embed.Title = "=== Nickname Change ====";
//            embed.Description = $"**Old Nickname: **{author.Nickname ?? author.Username}\n**New Nickname: **{a.Nickname}\n**ID: **{author.Id}";
//            embed.Color = new Color(193, 60, 144);
//            var LogServer = _client.GetChannel(ServerLogChannelId) as ITextChannel;
//            await LogServer.SendMessageAsync("", embed: embed);
//        }

//        private async Task _client_UserBanned(SocketUser user, SocketGuild gld)
//        {
//            await LogServerMessageAsync($"{user.Username + user.Discriminator} was banned from from {gld.Name}");
//        }

//        private async Task _client_LatencyUpdated(int older, int newer)
//        {
//            if (_client == null) return;
//            var newStatus = (_client.ConnectionState == ConnectionState.Disconnected || newer > 100) ? UserStatus.DoNotDisturb
//                    : (_client.ConnectionState == ConnectionState.Connecting || newer > 60)
//                        ? UserStatus.Idle
//                        : UserStatus.Online;

//            await _client.SetStatusAsync(newStatus);
//        }

//        private async Task _client_MessageReceived(SocketMessage msg)
//        {
//            if (msg.Author.Id == _client.CurrentUser.Id) return;
//            var response = GetResponses();
//            foreach (KeyValuePair<string, string> item in response.Where(x => x.Key.Contains(msg.ToString())))
//            {
//                if (msg.Content.Contains(item.Key))
//                {
//                    await msg.Channel.SendMessageAsync(item.Value);
//                }
//            }

//            var matches = _issueRegex.Matches(msg.Content);
//            if (matches.Count > 0)
//            {
//                var outStr = new StringBuilder();
//                foreach (Match match in matches)
//                {
//                    outStr.AppendLine($"**{match.Value}** - https://github.com/ExceptionDev/DiscordExampleBot/issues/{match.Value.Substring(2)}");
//                }
//                await msg.Channel.SendMessageAsync(outStr.ToString());
//            }
//        }

//        private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> msgCacheable, ISocketMessageChannel msgChannel, SocketReaction reaction)
//        {
//            // work on this later
//        }


//        #endregion

//        public LogService(DiscordSocketClient c, ConfigHandlerOLD config)
//        {
//            _client = c;
//            Config = config;
//        }

//        public static Dictionary<string, string> GetResponses()
//        {
//            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(DictPath));
//        }
//    }
//}