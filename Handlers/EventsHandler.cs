# pragma warning disable 1998, 4014
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        readonly ServerConfig ServerConfig;
        readonly BotConfig BotConfig;

        public EventsHandler(ServerConfig Config, BotConfig _Config)
        {
            ServerConfig = Config;
            BotConfig = _Config;
        }

        internal Task Log(LogMessage Log) => Task.Run(() => Logger.Write(Logger.Status.KAY, Logger.Source.Client, Log.Message ?? Log.Exception.StackTrace));
        internal Task LeftGuild(SocketGuild Guild) => ServerConfig.LoadOrDeleteAsync(Actions.Delete, Guild.Id);
        internal Task GuildAvailable(SocketGuild Guild) => ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id);

        internal async Task JoinedGuild(SocketGuild Guild)
        {
            ServerConfig.LoadOrDeleteAsync(Actions.Delete, Guild.Id);
            if (BotConfig.Config.ServerMessage == null) return;
            var Msg = await (await Guild.Owner.GetOrCreateDMChannelAsync())
                .SendMessageAsync(StringExtension.ReplaceWith(BotConfig.Config.ServerMessage, "?>", BotConfig.Config.Prefix));
            if (Msg == null)
                await Guild.DefaultChannel.SendMessageAsync(StringExtension.ReplaceWith(BotConfig.Config.ServerMessage, "?>", BotConfig.Config.Prefix));
        }

        internal Task UserJoined(SocketGuildUser User)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            if (Config == null) return Task.CompletedTask;
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

        internal Task UserLeft(SocketGuildUser User)
        {
            CleanupAsync(User);
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            if (Config == null) return Task.CompletedTask;
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

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerConfig.LoadConfig(Guild.Id);
            if (Config == null) return;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null ||
                Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;

            var Embed = ValerieEmbed.Embed(EmbedColor.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));

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
            ServerConfig.SaveAsync(Config, Guild.Id);
        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerConfig.LoadConfig(Guild.Id);
            if (Config == null) return;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null) return;

            var Embed = ValerieEmbed.Embed(EmbedColor.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
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
            ServerConfig.SaveAsync(Config, Guild.Id);
        }

        internal Task LatencyUpdated(DiscordSocketClient Client, int Older, int Newer)
            => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal Task ReadyAsync(DiscordSocketClient Client)
        {
            string Game = BotConfig.Config.BotGames.Count == 0 ? $"{BotConfig.Config.Prefix}Cmds" :
                BotConfig.Config.BotGames[new Random().Next(BotConfig.Config.BotGames.Count)];
            return Client.SetGameAsync(Game);
        }

        internal async Task MessageReceivedAsync(SocketMessage Message)
        {
            BotConfig.Config.MessagesReceived += 1;
            BotConfig.SaveAsync(BotConfig.Config);
            if (Message == null || (Message.Author as SocketGuildUser) == null) return;
            AFKHandlerAsync(Message);
            CleverbotHandlerAsync(Message);
            EridiumHandlerAsync(Message.Author as SocketGuildUser, Message.Content.Length);
            AutoModAsync(Message as SocketUserMessage);
        }

        Task AFKHandlerAsync(SocketMessage Message)
        {
            var Config = ServerConfig.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            if (Config == null) return Task.CompletedTask;
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => Config.AFKList.TryGetValue(u.Id, out afkReason));
            if (gldUser == null) return Task.CompletedTask;
            return Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        async Task CleverbotHandlerAsync(SocketMessage Message)
        {
            var Config = ServerConfig.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            if (Config == null) return;
            ITextChannel Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ChatterChannel));
            if (Message.Author.IsBot || Channel == null || !Message.Content.StartsWith("Valerie") || Message.Channel != Channel) return;
            string UserMsg = Message.Content.Replace("Valerie", "");
            CleverbotClient Client = new CleverbotClient(BotConfig.Config.APIKeys.CleverBotKey);
            await Channel.SendMessageAsync((await Client.TalkAsync(UserMsg).ConfigureAwait(false)).CleverOutput).ConfigureAwait(false);
        }

        async Task EridiumHandlerAsync(SocketGuildUser User, int Eridium)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            if (Config == null) return;
            var BlacklistedRoles = new List<ulong>(Config.EridiumHandler.BlacklistedRoles.Select(x => Convert.ToUInt64(x)));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (!Config.EridiumHandler.IsEnabled || HasRole || BotConfig.Config.UsersBlacklist.ContainsKey(User.Id)) return;
            var EridiumToGive = IntExtension.GiveEridium(Eridium, User.Guild.Users.Count);
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                Config.EridiumHandler.UsersList.TryAdd(User.Id, EridiumToGive);
                await ServerConfig.SaveAsync(Config, User.Guild.Id).ConfigureAwait(false);
                return;
            }
            Config.EridiumHandler.UsersList.TryGetValue(User.Id, out int Old);
            int OldLevel = IntExtension.GetLevel(Old);
            Config.EridiumHandler.UsersList.TryUpdate(User.Id, Old + EridiumToGive, Old);
            int NewLevel = IntExtension.GetLevel(Old + EridiumToGive);
            ServerConfig.SaveAsync(Config, User.Guild.Id);
            await AssignRoleAsync(Config, BoolExtension.HasLeveledUp(OldLevel, NewLevel), User);
            if (Config.EridiumHandler.LevelUpMessage == null || !BoolExtension.HasLeveledUp(OldLevel, NewLevel) || NewLevel == OldLevel) return;
            await (await User.GetOrCreateDMChannelAsync())
                .SendMessageAsync(StringExtension.ReplaceWith(Config.EridiumHandler.LevelUpMessage, User.Mention, $"{NewLevel}"));
        }

        Task AssignRoleAsync(ServerModel Config, bool CheckLevel, SocketGuildUser User)
        {
            int GetLevel = IntExtension.GetLevel(Config.EridiumHandler.UsersList[User.Id]);
            var GetRole = Config.EridiumHandler.LevelUpRoles.FirstOrDefault(x => x.Value == GetLevel).Key;
            if (!CheckLevel || !Config.EridiumHandler.LevelUpRoles.Any() || User.Roles.Contains(User.Guild.GetRole(GetRole)) ||
                IntExtension.GetLevel(Config.EridiumHandler.UsersList[User.Id]) > Config.EridiumHandler.MaxRoleLevel) return Task.CompletedTask;
            User.AddRoleAsync(User.Guild.GetRole(GetRole));
            return Task.CompletedTask;
        }

        Task AutoModAsync(SocketUserMessage Message)
        {
            var Config = ServerConfig.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            if (Config == null) return Task.CompletedTask;
            if (!Config.ModLog.IsAutoModEnabled || !BoolExtension.Advertisement(Message.Content))
                return Task.CompletedTask;
            Message.DeleteAsync();
            Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            Config.ModLog.Cases += 1;
            if (!Config.ModLog.Warnings.ContainsKey(Message.Author.Id))
            {
                Config.ModLog.Warnings.TryAdd(Message.Author.Id, 1);
                return Task.CompletedTask;
            }
            Config.ModLog.Warnings.TryGetValue(Message.Author.Id, out int PreviousWarns);
            if (!(PreviousWarns >= 3))
                Config.ModLog.Warnings.TryUpdate(Message.Author.Id, PreviousWarns += 1, PreviousWarns);
            else
            {
                (Message.Author as SocketGuildUser).KickAsync("Kicked by Auto Mod.");
                ITextChannel Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ModLog.TextChannel));
                var Embed = ValerieEmbed.Embed(EmbedColor.Red, ThumbUrl: Message.Author.GetAvatarUrl(), FooterText: $"Kick on {DateTime.Now}");
                Embed.AddField("User", $"{Message.Author}\n{Message.Author.Id}", true);
                Embed.AddField("Responsible Moderator", "Auto Moderator", true);
                Embed.AddField("Case No.", Config.ModLog.Cases, true);
                Embed.AddField("Case Type", "Kick", true);
                if (Channel != null)
                    return Channel.SendMessageAsync("", embed: Embed.Build());
            }
            ServerConfig.SaveAsync(Config, (Message.Channel as SocketGuildChannel).Guild.Id);
            return Task.CompletedTask;
        }

        async Task CleanupAsync(SocketGuildUser User)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            if (Config == null) return;
            if (!(Config.AFKList.ContainsKey(User.Id) || Config.EridiumHandler.UsersList.ContainsKey(User.Id))) return;
            Config.AFKList.Remove(User.Id, out string SomeString);
            Config.EridiumHandler.UsersList.Remove(User.Id, out int Eridium);
            Config.TagsList.RemoveAll(x => x.Owner == $"{User.Id}");
            ServerConfig.SaveAsync(Config, User.Guild.Id);
        }
    }
}
