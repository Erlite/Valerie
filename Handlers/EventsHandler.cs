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
    public class EventsHandler
    {
        BotConfig BConfig;
        ServerConfig ServerConfig;
        public EventsHandler(ServerConfig Config, BotConfig _BotConfig)
        {
            ServerConfig = Config;
            BConfig = _BotConfig;
        }

        internal Task Log(LogMessage Log) => Task.Run(() => Logger.Write(Status.KAY, Source.Client, Log.Message ?? Log.Exception.StackTrace));
        internal async Task LeftGuild(SocketGuild Guild) => await ServerConfig.LoadOrDeleteAsync(Actions.Delete, Guild.Id).ConfigureAwait(false);
        internal async Task GuildAvailable(SocketGuild Guild) => await ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id).ConfigureAwait(false);

        internal async Task JoinedGuild(SocketGuild Guild)
        {
            await ServerConfig.LoadOrDeleteAsync(Actions.Add, Guild.Id).ConfigureAwait(false);
            if (BotConfig.Config.ServerMessage == null) return;
            var Msg = await (await Guild.Owner.GetOrCreateDMChannelAsync())
                .SendMessageAsync(StringExtension.ReplaceWith(BotConfig.Config.ServerMessage, "?>", BotConfig.Config.Prefix));
            if (Msg == null)
                await Guild.DefaultChannel.SendMessageAsync(StringExtension.ReplaceWith(BotConfig.Config.ServerMessage, "?>", BotConfig.Config.Prefix));
        }

        internal async Task UserJoined(SocketGuildUser User)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            string WelcomeMessage = !Config.WelcomeMessages.Any() ? $"{User} just arrived. Seems OP - please nerf." :
                StringExtension.ReplaceWith(Config.WelcomeMessages[new Random().Next(0, Config.WelcomeMessages.Count)], User.Mention, User.Guild.Name);
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.JoinChannel));
            if (Channel != null) await Channel.SendMessageAsync(WelcomeMessage).ConfigureAwait(false);
            var Role = User.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole));
            if (Role != null) await User.AddRoleAsync(Role).ConfigureAwait(false);
        }

        internal async Task UserLeft(SocketGuildUser User)
        {
            _ = CleanupAsync(User).ConfigureAwait(false);
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.LeaveChannel));
            string LeaveMessage = !Config.LeaveMessages.Any() ? $"{User} has left {User.Guild.Name} :wave:" :
                StringExtension.ReplaceWith(Config.LeaveMessages[new Random().Next(0, Config.LeaveMessages.Count)], User.Username, User.Guild.Name);
            if (Channel != null) await Channel.SendMessageAsync(LeaveMessage);
        }

        internal Task UserBanned(SocketUser User, SocketGuild Guild)
        {
            var Config = ServerConfig.LoadConfig(Guild.Id);
            Config.ModLog.Cases += 1;
            return ServerConfig.SaveAsync(Config, Guild.Id);
        }

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerConfig.LoadConfig(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            if (Message == null || StarboardChannel == null || Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;
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
            await ServerConfig.SaveAsync(Config, Guild.Id).ConfigureAwait(false);
        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerConfig.LoadConfig(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Message == null || StarboardChannel == null) return;
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
            await ServerConfig.SaveAsync(Config, Guild.Id).ConfigureAwait(false);
        }

        internal Task LatencyUpdated(DiscordSocketClient Client, int Older, int Newer)
            => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal Task ReadyAsync(DiscordSocketClient Client)
        {
            string Game = BotConfig.Config.BotGames == null ? $"{BotConfig.Config.Prefix}Cmds" :
                BotConfig.Config.BotGames[new Random(Guid.NewGuid().GetHashCode()).Next(BotConfig.Config.BotGames.Count)];
            return Client.SetGameAsync(Game);
        }

        internal async Task MessageReceivedAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg) || !(Message.Author is SocketGuildUser User)) return;
            if (Msg.Source != MessageSource.User | Msg.Author.IsBot || BotConfig.Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            await AFKHandlerAsync(Msg).ConfigureAwait(false);
            _ = CleverbotHandlerAsync(Msg);
            _ = EridiumHandlerAsync(User, Msg.Content.Length);
            _ = AutoModAsync(Msg);
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
            var BlacklistedRoles = new List<ulong>(Config.EridiumHandler.BlacklistedRoles.Select(x => Convert.ToUInt64(x)));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (!Config.EridiumHandler.IsEnabled || HasRole || BotConfig.Config.UsersBlacklist.ContainsKey(User.Id)) return;
            var EridiumToGive = IntExtension.GiveEridium(Eridium);
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                Config.EridiumHandler.UsersList.TryAdd(User.Id, EridiumToGive);
                await ServerConfig.SaveAsync(Config, User.Guild.Id).ConfigureAwait(false);
                return;
            }
            int Old = Config.EridiumHandler.UsersList[User.Id];
            Config.EridiumHandler.UsersList[User.Id] += EridiumToGive;
            var New = Config.EridiumHandler.UsersList[User.Id];
            await ServerConfig.SaveAsync(Config, User.Guild.Id);
            _ = LevelUpAsync(User, Old, New);
        }

        async Task LevelUpAsync(SocketGuildUser User, int OldEridium, int NewEridium)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            int OldLevel = IntExtension.GetLevel(OldEridium);
            int NewLevel = IntExtension.GetLevel(NewEridium);
            if (!(NewLevel > OldLevel) || NewLevel > Config.EridiumHandler.MaxRoleLevel || !Config.EridiumHandler.LevelUpRoles.Any()) return;
            if (!string.IsNullOrWhiteSpace(Config.EridiumHandler.LevelUpMessage))
                await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync(StringExtension.ReplaceWith(Config.EridiumHandler.LevelUpMessage, User.Mention, $"{NewLevel}"));
            var Role = User.Guild.GetRole(Config.EridiumHandler.LevelUpRoles.Where(x => x.Value == NewLevel).FirstOrDefault().Key);
            if (User.Roles.Contains(Role) || !User.Guild.Roles.Contains(Role)) return;
            await User.AddRoleAsync(Role);
            foreach (var lvlrole in Config.EridiumHandler.LevelUpRoles)
                if (lvlrole.Value < NewLevel)
                    if (!User.Roles.Contains(User.Guild.GetRole(lvlrole.Key)))
                        await User.AddRoleAsync(User.Guild.GetRole(lvlrole.Key));
        }

        async Task AutoModAsync(SocketUserMessage Message)
        {
            var Config = ServerConfig.LoadConfig((Message.Channel as SocketGuildChannel).Guild.Id);
            if (!Config.ModLog.IsAutoModEnabled || !BoolExtension.Advertisement(Message.Content) ||
                Message.Author == (Message.Channel as SocketGuildChannel).Guild.Owner) return;
            await Message.DeleteAsync();
            await Message.Channel.SendMessageAsync($"{Message.Author.Mention}, please don't post invite links.");
            Config.ModLog.Cases += 1;
            if (!Config.ModLog.Warnings.ContainsKey(Message.Author.Id))
            {
                Config.ModLog.Warnings.TryAdd(Message.Author.Id, 1);
                await ServerConfig.SaveAsync(Config, (Message.Channel as SocketGuildChannel).Guild.Id).ConfigureAwait(false);
                return;
            }
            Config.ModLog.Warnings.TryGetValue(Message.Author.Id, out int PreviousWarns);
            if (!(PreviousWarns >= Config.ModLog.MaxWarnings))
                Config.ModLog.Warnings.TryUpdate(Message.Author.Id, PreviousWarns + 1, PreviousWarns);
            else
            {
                await (Message.Author as SocketGuildUser).KickAsync("Kicked by Auto Mod.");
                ITextChannel Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ModLog.TextChannel));
                IUserMessage Msg = null;
                if (Channel != null)
                    Msg = await Channel.SendMessageAsync($"**Kick** | Case {Config.ModLog.Cases}\n**User:** {Message.Author} ({Message.Author.Id})\n**Reason:** Maxed out warnings.\n" +
                    $"**Responsible Moderator:** Action Taken by Auto Moderator.");
                if (Msg == null)
                    await (await (Message.Author as IGuildUser).Guild.GetDefaultChannelAsync()).SendMessageAsync($"*{Message.Author} was kicked by Auto Moderator.*  :cop:");

                Config.ModLog.ModCases.Add(new CaseWrapper()
                {
                    CaseType = "Kick",
                    CaseNumber = Config.ModLog.Cases,
                    Reason = "Maxed out warnings.",
                    ResponsibleMod = $"Auto Moderator",
                    UserId = $"{Message.Author.Id}",
                    User = $"{Message.Author}",
                    MessageId = $"{Msg.Id}"
                });
            }
            await ServerConfig.SaveAsync(Config, (Message.Channel as SocketGuildChannel).Guild.Id).ConfigureAwait(false);
        }

        async Task CleanupAsync(SocketGuildUser User)
        {
            var Config = ServerConfig.LoadConfig(User.Guild.Id);
            if (!(Config.AFKList.ContainsKey(User.Id) || Config.EridiumHandler.UsersList.ContainsKey(User.Id))) return;
            Config.AFKList.Remove(User.Id, out string SomeString);
            Config.EridiumHandler.UsersList.Remove(User.Id, out int Eridium);
            Config.TagsList.RemoveAll(x => x.Owner == $"{User.Id}");
            await ServerConfig.SaveAsync(Config, User.Guild.Id).ConfigureAwait(false);
        }
    }
}