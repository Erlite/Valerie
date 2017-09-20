# pragma warning disable 1998, 4014
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Cookie.Cleverbot;
using Valerie.Enums;
using Valerie.Services;
using Valerie.Extensions;
using Valerie.Handlers.Config;
using Valerie.Handlers.Server;
using Valerie.Handlers.Server.Models;

namespace Valerie.Handlers
{
    class EventsHandler
    {
        public static IServiceProvider Provider { get; set; }
        internal static Task Log(LogMessage Log) => Task.Run(() => Logger.Write(Logger.Status.KAY, Logger.Source.Client, Log.Message));
        internal static Task LeftGuild(SocketGuild Guild) => Task.Run(() => ServerConfig.LoadOrDeleteAsync(Actions.Delete, Guild.Id));
        internal static Task GuildAvailable(SocketGuild Guild) => Task.Run(() => ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id));

        internal static async Task JoinedGuild(SocketGuild Guild)
        {
            Task.Run(() => ServerConfig.LoadOrDeleteAsync(Actions.Delete, Guild.Id));
            var Msg = await (await Guild.Owner.GetOrCreateDMChannelAsync())
                .SendMessageAsync(BotConfig.Config.ServerMessage.ReplaceWith("?>", BotConfig.Config.Prefix));
            if (Msg == null)
                await Guild.DefaultChannel.SendMessageAsync(BotConfig.Config.ServerMessage.ReplaceWith("?>", BotConfig.Config.Prefix));
        }

        internal static Task UserJoined(SocketGuildUser User)
        {
            var Config = Provider.GetService<ServerConfig>().LoadConfig(User.Guild.Id);
            string WelcomeMessage = null;
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.JoinChannel));
            if (Config.WelcomeMessages.Count <= 0)
                WelcomeMessage = $"{User} just arrived. Seems OP - please nerf.";
            else
            {
                var ConfigMsg = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
                WelcomeMessage = StringExtension.ReplaceWith(ConfigMsg, User.Mention, User.Guild.Name);
            }

            if (Channel != null)
                Channel.SendMessageAsync(WelcomeMessage);
            var Role = User.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole));
            if (Role != null)
                User.AddRoleAsync(Role);
            return Task.CompletedTask;
        }

        internal static Task UserLeft(SocketGuildUser User)
        {
            var GetInstance = Provider.GetService<ServerConfig>();
            CleanupAsync(GetInstance, User);
            var Config = GetInstance.LoadConfig(User.Guild.Id);
            string LeaveMessage = null;
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.LeaveChannel));
            if (Config.LeaveMessages.Count <= 0)
                LeaveMessage = $"{User} has left {User.Guild.Name} :wave:";
            else
            {
                var configMsg = Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)];
                LeaveMessage = StringExtension.ReplaceWith(configMsg, User.Username, User.Guild.Name);
            }
            if (Channel != null)
                Channel.SendMessageAsync(LeaveMessage);
            return Task.CompletedTask;
        }

        internal static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var GetInstance = Provider.GetService<ServerConfig>();
            var Config = GetInstance.LoadConfig(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null ||
                Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;

            var Embed = ValerieEmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));

            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);

            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == $"{Message.Id}");
            if (Config.Starboard.StarboardMessages.Contains(Exists))
            {
                Exists.Stars += 1;

                var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
                await SMsg.ModifyAsync(x =>
                {
                    x.Content =
                    $"{StringExtension.StarType(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            }
            else
            {
                var Msg = await StarboardChannel.SendMessageAsync(
                    $"{StringExtension.StarType(Message.Reactions.Count)}{Message.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention} ID: {Reaction.MessageId}", embed: Embed.Build());
                Config.Starboard.StarboardMessages.Add(new StarboardMessages
                {
                    ChannelId = $"{Message.Channel.Id}",
                    StarboardMessageId = $"{Msg.Id}",
                    MessageId = $"{Message.Id}",
                    Stars = 1
                });
            }
            GetInstance.Save(Config, Guild.Id);
        }

        internal static async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var GetInstance = Provider.GetService<ServerConfig>();
            var Config = GetInstance.LoadConfig(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null) return;

            var Embed = ValerieEmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);

            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == $"{Message.Id}");
            if (!Config.Starboard.StarboardMessages.Contains(Exists)) return;
            Exists.Stars -= 1;
            var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
            if (Message.Reactions.Count > 0)
                await SMsg.ModifyAsync(x =>
                {
                    x.Content =
                    $"{StringExtension.StarType(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            else
            {
                Config.Starboard.StarboardMessages.Remove(Exists);
                await SMsg.DeleteAsync();
            }
            GetInstance.Save(Config, Guild.Id);
        }

        internal static Task LatencyUpdated(DiscordSocketClient Client, int Older, int Newer)
        {
            Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);
            return Task.CompletedTask;
        }

        public static Task ReadyAsync(DiscordSocketClient Client)
        {
            string Game = BotConfig.Config.BotGames.Count == 0 ? $"{BotConfig.Config.Prefix}Cmds" :
                BotConfig.Config.BotGames[new Random().Next(BotConfig.Config.BotGames.Count)];
            return Client.SetGameAsync(Game);
        }

        internal static async Task MessageReceivedAsync(SocketMessage Message)
        {
            BotConfig.Config.MessagesReceived += 1;
            BotConfig.SaveAsync();
            var GetInstance = Provider.GetService<ServerConfig>();
            AFKHandlerAsync(GetInstance, Message);
            CleverbotHandlerAsync(GetInstance, Message);
            EridiumHandlerAsync(GetInstance, Message.Author as SocketGuildUser, Message.Content.Length);
            AutoMod(GetInstance, Message as SocketUserMessage);
        }

        static async Task AFKHandlerAsync(ServerConfig GetInstance, SocketMessage Message)
        {
            var Config = GetInstance.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => Config.AFKList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async Task CleverbotHandlerAsync(ServerConfig GetInstance, SocketMessage Message)
        {
            var Config = GetInstance.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            ITextChannel Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ChatterChannel));
            if (Message.Author.IsBot || Channel == null || !Message.Content.StartsWith("Valerie") || Message.Channel != Channel) return;
            string UserMsg = Message.Content.Replace("Valerie", "");
            CleverbotClient Client = new CleverbotClient(BotConfig.Config.APIKeys.CleverBotKey);
            await Channel.SendMessageAsync((await Client.TalkAsync(UserMsg).ConfigureAwait(false)).CleverOutput).ConfigureAwait(false);
        }

        static async Task EridiumHandlerAsync(ServerConfig GetInstance, SocketGuildUser User, int Eridium)
        {
            var Config = GetInstance.LoadConfig(User.Guild.Id);
            var BlacklistedRoles = new List<ulong>(Config.EridiumHandler.BlacklistedRoles.Select(x => Convert.ToUInt64(x)));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (User == null || User.IsBot || !Config.EridiumHandler.IsEnabled || HasRole || BotConfig.Config.UsersBlacklist.ContainsKey(User.Id)) return;
            var EridiumToGive = IntExtension.GiveEridium(Eridium, User.Guild.Users.Count);
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                Config.EridiumHandler.UsersList.TryAdd(User.Id, EridiumToGive);
                await GetInstance.Save(Config, User.Guild.Id).ConfigureAwait(false);
                return;
            }
            Config.EridiumHandler.UsersList.TryGetValue(User.Id, out int Old);
            int OldLevel = IntExtension.GetLevel(Old);
            Config.EridiumHandler.UsersList.TryUpdate(User.Id, Old + EridiumToGive, Old);
            int NewLevel = IntExtension.GetLevel(Old + EridiumToGive);
            GetInstance.Save(Config, User.Guild.Id);
            await AssignRole(Config, BoolExtension.HasLeveledUp(OldLevel, NewLevel), User);
            if (Config.EridiumHandler.LevelUpMessage == null || !BoolExtension.HasLeveledUp(OldLevel, NewLevel) || NewLevel == OldLevel) return;
            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(Config.EridiumHandler.LevelUpMessage.ReplaceWith(User.Mention, $"{NewLevel}"));
        }

        static Task AssignRole(ServerModel Config, bool CheckLevel, SocketGuildUser User)
        {
            int GetLevel = IntExtension.GetLevel(Config.EridiumHandler.UsersList[User.Id]);
            var GetRole = Config.EridiumHandler.LevelUpRoles.FirstOrDefault(x => x.Value == GetLevel).Key;
            if (!CheckLevel || !Config.EridiumHandler.LevelUpRoles.Any() || User.Roles.Contains(User.Guild.GetRole(GetRole)) ||
                IntExtension.GetLevel(Config.EridiumHandler.UsersList[User.Id]) > Config.EridiumHandler.MaxRoleLevel) return Task.CompletedTask;
            User.AddRoleAsync(User.Guild.GetRole(GetRole));
            return Task.CompletedTask;
        }

        static Task AutoMod(ServerConfig GetInstance, SocketUserMessage Message)
        {
            var Config = GetInstance.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            if (!Config.ModLog.IsAutoModEnabled || !BoolExtension.Advertisement(Message.Content)) return Task.CompletedTask;
            Message.DeleteAsync();
            Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            return Task.CompletedTask;
        }

        static async Task CleanupAsync(ServerConfig GetInstance, SocketGuildUser User)
        {
            var Config = GetInstance.LoadConfig(User.Guild.Id);
            if (!(Config.AFKList.ContainsKey(User.Id) || Config.EridiumHandler.UsersList.ContainsKey(User.Id))) return;
            Config.AFKList.Remove(User.Id, out string SomeString);
            Config.EridiumHandler.UsersList.Remove(User.Id, out int Eridium);
            Config.TagsList.RemoveAll(x => x.Owner == $"{User.Id}");
            GetInstance.Save(Config, User.Guild.Id);
        }
    }
}
