﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Valerie.Handlers.GuildHandler;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Handlers.ConfigHandler;
using Valerie.Extensions;
using Cleverbot;
using Cleverbot.Models;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        static List<ulong> Waitlist = new List<ulong>();
        static Timer timer;

        internal static async Task GuildAvailableAsync(SocketGuild Guild)
        {
            await ServerDB.LoadGuildConfigsAsync(Guild.Id);
        }

        internal static async Task JoinedGuildAsync(SocketGuild Guild)
        {
            await ServerDB.LoadGuildConfigsAsync(Guild.Id);
            var Config = ServerDB.GuildConfig(Guild.Id);
            string JoinMessage = null;
            if (string.IsNullOrWhiteSpace(BotDB.Config.GuildJoinMessage))
                JoinMessage = $"Thank you for inviting me to your guild!\n" +
                    $"Your Guild Prefix: {Config.Prefix}  | Default Prefix: {BotDB.Config.Prefix}";
            else
                JoinMessage = StringExtension.JoinReplace(BotDB.Config.GuildJoinMessage, Config.Prefix, BotDB.Config.Prefix);
            await (await Guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync(JoinMessage);
        }

        internal static async Task LeftGuildAsync(SocketGuild Guild)
        {
            await ServerDB.DeleteGuildConfigAsync(Guild.Id);
        }

        internal static async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = ServerDB.GuildConfig(User.Guild.Id);
            if (!Config.JoinEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string WelcomeMessage = null;
            ulong Id = Convert.ToUInt64(Config.JoinEvent.TextChannel);
            var JoinChannel = User.Guild.GetChannel(Id);

            if (Config.WelcomeMessages.Count <= 0)
                WelcomeMessage = $"{User} just joined {User.Guild.Name}! WELCOME!";
            else
            {
                var ConfigMsg = Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)];
                WelcomeMessage = StringExtension.ReplaceWith(ConfigMsg, User.Mention, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Id) != null)
            {
                Channel = JoinChannel as ITextChannel;
                await Channel.SendMessageAsync(WelcomeMessage);
            }
            else
            {
                Channel = User.Guild.DefaultChannel as ITextChannel;
                await Channel.SendMessageAsync(WelcomeMessage);
            }
        }

        internal static async Task UserLeftAsync(SocketGuildUser User)
        {
            await CleanUpAsync(User);
            var Config = ServerDB.GuildConfig(User.Guild.Id);
            if (!Config.LeaveEvent.IsEnabled) return;

            ITextChannel Channel = null;
            string LeaveMessage = null;
            ulong Id = Convert.ToUInt64(Config.LeaveEvent.TextChannel);
            var LeaveChannel = User.Guild.GetChannel(Id);

            if (Config.LeaveMessages.Count <= 0)
                LeaveMessage = $"{User} has left {User.Guild.Name} :wave:";
            else
            {
                var configMsg = Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)];
                LeaveMessage = StringExtension.ReplaceWith(configMsg, User.Username, User.Guild.Name);
            }

            if (User.Guild.GetChannel(Id) != null)
            {
                Channel = LeaveChannel as ITextChannel;
                await Channel.SendMessageAsync(LeaveMessage);
            }
            else
            {
                Channel = User.Guild.DefaultChannel as ITextChannel;
                await Channel.SendMessageAsync(LeaveMessage);
            }
        }

        internal static async Task MessageReceivedAsync(SocketMessage Message)
        {
            await BotDB.UpdateConfigAsync(ConfigHandler.Enum.ConfigValue.MessageReceived);
            await KarmaHandlerAsync(Message.Author as SocketGuildUser, Message.Content.Length);
            await AFKHandlerAsync((Message.Author as SocketGuildUser).Guild, Message);
            await CleverbotHandlerAsync((Message.Author as SocketGuildUser).Guild, Message);
            await AntiAdvertisementAsync((Message.Author as SocketGuildUser).Guild, Message);
        }

        internal static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerDB.GuildConfig(Guild.Id);
            if (Reaction.Emote.Name != "⭐" || !Config.Starboard.IsEnabled || Config.Starboard.TextChannel == null || Message == null) return;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            var Embed = Vmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);
            var Exists = Config.StarredMessages.FirstOrDefault(x => x.MessageId == Message.Id.ToString());
            if (Config.StarredMessages.Contains(Exists))
            {
                await ServerDB.StarboardHandlerAsync(Guild.Id, ModelEnum.StarAdd, Message.Id, Message.Channel.Id, Reaction.MessageId);
                var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
                await SMsg.ModifyAsync(x =>
                {
                    x.Content =
                    $"{StringExtension.StarType(Exists.Stars + 1)}{Exists.Stars + 1} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            }
            else
            {
                var msg = await StarboardChannel.SendMessageAsync(
                    $"{StringExtension.StarType(Message.Reactions.Count)}{Message.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention} ID: {Reaction.MessageId}", embed: Embed);
                await ServerDB.StarboardHandlerAsync(Guild.Id, ModelEnum.StarNew, Message.Id, Message.Channel.Id, msg.Id);
            }
        }

        internal static async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerDB.GuildConfig(Guild.Id);
            if (Reaction.Emote.Name != "⭐" || !Config.Starboard.IsEnabled || Config.Starboard.TextChannel == null || Message == null) return;
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            var Embed = Vmbed.Embed(VmbedColors.Gold, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content))
                Embed.WithDescription(Message.Content);
            else if (Message.Attachments.FirstOrDefault() != null)
                Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);

            var Exists = Config.StarredMessages.FirstOrDefault(x => x.MessageId == Message.Id.ToString());
            if (!Config.StarredMessages.Contains(Exists)) return;

            await ServerDB.StarboardHandlerAsync(Guild.Id, ModelEnum.StarSubtract, Message.Id, Message.Channel.Id, Reaction.MessageId);

            var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
            while (true)
            {
                if (Message.Reactions.Count > 0)
                    await SMsg.ModifyAsync(x =>
                    {
                        x.Content =
                        $"{StringExtension.StarType(Exists.Stars - 1)}{Exists.Stars - 1} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                        x.Embed = Embed.Build();
                    });
                else
                {
                    await ServerDB.StarboardHandlerAsync(Guild.Id, ModelEnum.StarDelete, Message.Id, Message.Channel.Id, SMsg.Id);
                    await SMsg.DeleteAsync();
                }
            }

        }

        // Not Events
        static void RemoveUser(ulong Id)
        {
            timer = new Timer(_ =>
            {
                Waitlist.Remove(Id);
            },
            null,
            TimeSpan.FromSeconds(30.0),
            TimeSpan.FromSeconds(0.0));
        }

        static async Task KarmaHandlerAsync(SocketGuildUser User, int Karma)
        {
            RemoveUser(User.Id);
            var GuildID = User.Guild.Id;
            var GuildConfig = ServerDB.GuildConfig(GuildID);

            var HasRole = (User as IGuildUser).RoleIds.Intersect(GuildConfig.KarmaHandler.BlacklistRoles.Select(x => UInt64.Parse(x))).Any();

            if (User == null || User.IsBot || !GuildConfig.KarmaHandler.IsKarmaEnabled ||
                BotDB.Config.Blacklist.ContainsKey(User.Id) || Waitlist.Contains(User.Id) || HasRole) return;

            var RandomKarma = IntExtension.GiveKarma(Karma, User.Guild.Users.Count);
            if (!GuildConfig.KarmaHandler.UsersList.ContainsKey(User.Id))
            {
                await ServerDB.KarmaHandlerAsync(GuildID, ModelEnum.KarmaNew, User.Id, RandomKarma);
                return;
            }

            int OldLevel = IntExtension.GetLevel(GuildConfig.KarmaHandler.UsersList[User.Id]);
            int NewLevel = IntExtension.GetLevel(GuildConfig.KarmaHandler.UsersList[User.Id] + RandomKarma);

            await ServerDB.KarmaHandlerAsync(GuildID, ModelEnum.KarmaUpdate, User.Id, RandomKarma);
            Waitlist.Add(User.Id);

            await AssignRole(BoolExtension.HasLeveledUp(OldLevel, NewLevel), GuildID, User);
        }

        static async Task AFKHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            string afkReason = null;
            SocketUser gldUser = Message.MentionedUsers.FirstOrDefault(u => Config.AFKList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await Message.Channel.SendMessageAsync($"**Message left from {gldUser.Username}:** {afkReason}");
        }

        static async Task CleanUpAsync(SocketGuildUser User)
        {
            var GuildConfig = ServerDB.GuildConfig(User.Guild.Id);
            if (!(GuildConfig.AFKList.ContainsKey(User.Id) || GuildConfig.KarmaHandler.UsersList.ContainsKey(User.Id))) return;
            await ServerDB.AFKHandlerAsync(User.Guild.Id, ModelEnum.AFKRemove, User.Id);
            await ServerDB.KarmaHandlerAsync(User.Guild.Id, ModelEnum.KarmaDelete, User.Id);
            await ServerDB.TagsHandlerAsync(User.Guild.Id, ModelEnum.TagPurge, Owner: User.Id.ToString());
        }

        static async Task CleverbotHandlerAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            var Channel = Guild.GetChannel(Convert.ToUInt64(Config.Chatterbot.TextChannel)) as IMessageChannel;
            if (Message.Author.IsBot || !Config.Chatterbot.IsEnabled || !Message.Content.StartsWith("Valerie") || Message.Channel != Channel) return;
            string UserMsg = Message.Content.Replace("Valerie", "");
            CleverbotResponse Response = null;
            Response = await Main.TalkAsync(UserMsg, Response);
            if (Channel != null)
                await Channel.SendMessageAsync(Response.Output);
            else
                await Message.Channel.SendMessageAsync(Response.Output);
        }

        static async Task AntiAdvertisementAsync(SocketGuild Guild, SocketMessage Message)
        {
            var Config = ServerDB.GuildConfig(Guild.Id);
            if (!Config.AntiAdvertisement || Guild == null) return;
            if (BoolExtension.Advertisement(Message.Content))
            {
                await Message.DeleteAsync();
                await Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            }
        }

        static async Task AssignRole(bool CheckLevel, ulong GuildId, SocketGuildUser User)
        {
            var KarmaHandler = ServerDB.GuildConfig(GuildId).KarmaHandler;
            if (!CheckLevel || !KarmaHandler.LevelUpRoles.Any() ||
                IntExtension.GetLevel(KarmaHandler.UsersList[User.Id]) > KarmaHandler.MaxRoleLevel) return;
            int GetLevel = IntExtension.GetLevel(KarmaHandler.UsersList[User.Id]);
            var GetRole = KarmaHandler.LevelUpRoles.FirstOrDefault(x => x.Value >= GetLevel).Key;
            await User.AddRoleAsync(User.Guild.GetRole(GetRole));
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
            var Config = BotDB.Config;
            var GetGame = Config.Games[new Random().Next(Config.Games.Count)];
            if (Client == null) return;
            if (Config.Games.Count <= 0)
            {
                await Client.SetGameAsync(Config.Prefix + "Cmds");
                return;
            }
            else
                await Client.SetGameAsync(GetGame);
        }

    }
}
