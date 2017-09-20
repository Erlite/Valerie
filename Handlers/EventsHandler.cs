﻿# pragma warning disable 1998, 4014
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Cookie.Cleverbot;
using Valerie.Modules.Enums;
using Valerie.Handlers.Server;
using Valerie.Handlers.Server.Models;
using Valerie.Handlers.Config;
using Valerie.Extensions;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        internal static async Task GuildAvailableAsync(SocketGuild Guild) => await ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id).ConfigureAwait(false);

        internal static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            await ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id).ConfigureAwait(false);
            var Config = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            string JoinMessage = null;
            if (string.IsNullOrWhiteSpace(BotConfig.Config.ServerMessage))
                JoinMessage = $"Thank you for inviting me to your guild!\n" +
                    $"Your Guild Prefix: {Config.Prefix}  | Default Prefix: {BotConfig.Config.Prefix}";
            else
                JoinMessage = StringExtension.ReplaceWith(BotConfig.Config.ServerMessage, Config.Prefix, BotConfig.Config.Prefix);
            var Msg = await (await Guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync(JoinMessage);
            if (Msg == null)
                await Guild.DefaultChannel.SendMessageAsync(JoinMessage);
        }

        internal static async Task LeftGuildAsync(SocketGuild Guild) => await ServerConfig.LoadOrDeleteAsync(Actions.Remove, Guild.Id).ConfigureAwait(false);

        internal static async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = await ServerConfig.ConfigAsync(User.Guild.Id).ConfigureAwait(false);
            string WelcomeMessage = null;
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.JoinChannel));
            if (Config.WelcomeMessages.Count <= 0)
                WelcomeMessage = $"{User} just joined {User.Guild.Name}! WELCOME!";
            else
            {
                var ConfigMsg = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
                WelcomeMessage = StringExtension.ReplaceWith(ConfigMsg, User.Mention, User.Guild.Name);
            }

            if (Channel == null) return;
            await Channel.SendMessageAsync(WelcomeMessage);
            var Role = User.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole));
            if (Role == null) return;
            await User.AddRoleAsync(Role);
        }

        internal static async Task UserLeftAsync(SocketGuildUser User)
        {
            await CleanUpAsync(User);
            var Config = await ServerConfig.ConfigAsync(User.Guild.Id).ConfigureAwait(false);
            string LeaveMessage = null;
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.LeaveChannel));
            if (Config.LeaveMessages.Count <= 0)
                LeaveMessage = $"{User} has left {User.Guild.Name} :wave:";
            else
            {
                var configMsg = Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)];
                LeaveMessage = StringExtension.ReplaceWith(configMsg, User.Username, User.Guild.Name);
            }
            if (Channel == null) return;
            await Channel.SendMessageAsync(LeaveMessage);
        }

        internal static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var GetConfig = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            var Config = ServerConfig.Config;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null ||
                Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;

            var Embed = ValerieEmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));

            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);
            else if (BoolExtension.IsMessageUrl(Message.Content))
                Embed.WithImageUrl(Message.Content);

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
            await ServerConfig.SaveAsync().ConfigureAwait(false);
        }

        internal static async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var GetConfig = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            var Config = ServerConfig.Config;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Reaction.Emote.Name != "⭐" || Message == null || StarboardChannel == null) return;

            var Embed = ValerieEmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);
            else if (BoolExtension.IsMessageUrl(Message.Content))
                Embed.WithImageUrl(Message.Content);

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
            await ServerConfig.SaveAsync().ConfigureAwait(false);
        }

        internal static async Task MessageReceivedAsync(SocketMessage Message)
        {
            BotConfig.Config.MessagesReceived += 1;
            await BotConfig.SaveAsync().ConfigureAwait(false);
            Task.Run(async () => await EridiumHandlerAsync(Message.Author as SocketGuildUser, Message.Content.Length));
            Task.Run(async () => await AFKHandlerAsync((Message.Author as SocketGuildUser).Guild, Message));
            Task.Run(async () => await CleverbotHandlerAsync((Message.Author as SocketGuildUser).Guild, Message));
            Task.Run(async () => await AntiAdvertisementAsync((Message.Author as SocketGuildUser).Guild, Message));
        }

        static async Task EridiumHandlerAsync(SocketGuildUser User, int Eridium)
        {
            var GuildID = User.Guild.Id;
            var GetConfig = await ServerConfig.ConfigAsync(User.Guild.Id).ConfigureAwait(false);
            var GuildConfig = ServerConfig.Config;
            var BlacklistedRoles = new List<ulong>(GuildConfig.EridiumHandler.BlacklistedRoles.Select(x => Convert.ToUInt64(x)));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (User == null || User.IsBot || !GuildConfig.EridiumHandler.IsEnabled || HasRole || BotConfig.Config.UsersBlacklist.ContainsKey(User.Id)) return;
            var EridiumToGive = IntExtension.GiveEridium(Eridium, User.Guild.Users.Count);
            if (!GuildConfig.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                GuildConfig.EridiumHandler.UsersList.TryAdd(User.Id, EridiumToGive);
                await ServerConfig.SaveAsync().ConfigureAwait(false);
                return;
            }
            GuildConfig.EridiumHandler.UsersList.TryGetValue(User.Id, out int Old);
            int OldLevel = IntExtension.GetLevel(Old);
            GuildConfig.EridiumHandler.UsersList.TryUpdate(User.Id, Old + EridiumToGive, Old);
            int NewLevel = IntExtension.GetLevel(Old + EridiumToGive);
            await ServerConfig.SaveAsync().ConfigureAwait(false);
            await AssignRole(BoolExtension.HasLeveledUp(OldLevel, NewLevel), GuildID, User);
            if (GuildConfig.EridiumHandler.LevelUpMessage == null || !BoolExtension.HasLeveledUp(OldLevel, NewLevel) || NewLevel == OldLevel) return;
            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(GuildConfig.EridiumHandler.LevelUpMessage.ReplaceWith(User.Mention, $"{NewLevel}"));
        }

        static async Task AssignRole(bool CheckLevel, ulong GuildId, SocketGuildUser User)
        {
            var EridiumHandler = (await ServerConfig.ConfigAsync(GuildId).ConfigureAwait(false)).EridiumHandler;

            int GetLevel = IntExtension.GetLevel(EridiumHandler.UsersList[User.Id]);
            var GetRole = EridiumHandler.LevelUpRoles.FirstOrDefault(x => x.Value == GetLevel).Key;

            if (!CheckLevel || !EridiumHandler.LevelUpRoles.Any() || User.Roles.Contains(User.Guild.GetRole(GetRole)) ||
                IntExtension.GetLevel(EridiumHandler.UsersList[User.Id]) > EridiumHandler.MaxRoleLevel) return;

            await User.AddRoleAsync(User.Guild.GetRole(GetRole));
        }

        static async Task AFKHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => Config.AFKList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async Task CleanUpAsync(SocketGuildUser User)
        {
            var GetConfig = await ServerConfig.ConfigAsync(User.Guild.Id).ConfigureAwait(false);
            var GuildConfig = ServerConfig.Config;
            if (!(GuildConfig.AFKList.ContainsKey(User.Id) || GuildConfig.EridiumHandler.UsersList.ContainsKey(User.Id))) return;
            GuildConfig.AFKList.Remove(User.Id, out string SomeString);
            GuildConfig.EridiumHandler.UsersList.Remove(User.Id, out int Eridium);
            GuildConfig.TagsList.RemoveAll(x => x.Owner == $"{User.Id}");
            await ServerConfig.SaveAsync().ConfigureAwait(false);
        }

        static async Task CleverbotHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            ITextChannel Channel = Guild.GetTextChannel(Convert.ToUInt64(Config.ChatterChannel));
            if (Message.Author.IsBot || Channel == null || !Message.Content.StartsWith("Valerie") || Message.Channel != Channel) return;
            string UserMsg = Message.Content.Replace("Valerie", "");
            CleverbotClient Client = new CleverbotClient(BotConfig.Config.APIKeys.CleverBotKey);
            await Channel.SendMessageAsync((await Client.TalkAsync(UserMsg).ConfigureAwait(false)).CleverOutput).ConfigureAwait(false);
        }

        static async Task AntiAdvertisementAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = await ServerConfig.ConfigAsync(Guild.Id).ConfigureAwait(false);
            if (!Config.ModLog.AntiAdvertisement || Guild == null) return;
            if (BoolExtension.Advertisement(Message.Content))
            {
                await Message.DeleteAsync();
                await Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            }
        }

        public static async Task LatencyUpdatedAsync(DiscordSocketClient Client, int Older, int Newer)
        {
            if (Client == null) return;

            var Status = (Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK;

            await Client.SetStatusAsync(Status);
        }

        public static async Task ReadyAsync(DiscordSocketClient Client)
        {
            var Config = BotConfig.Config;
            var GetGame = Config.BotGames[new Random().Next(Config.BotGames.Count)];
            if (Client == null) return;
            if (Config.BotGames.Count <= 0)
            {
                await Client.SetGameAsync(Config.Prefix + "Cmds");
                return;
            }
            else
                await Client.SetGameAsync(GetGame);
        }
    }
}