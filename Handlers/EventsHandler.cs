using System;
using Discord;
using System.Linq;
using Valerie.Services;
using System.Reflection;
using Valerie.Extensions;
using Discord.Commands;
using Valerie.JsonModels;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        Random Random;
        IServiceProvider Provider;
        DiscordSocketClient Client;
        ConfigHandler ConfigHandler;
        ServerHandler ServerHandler;
        CommandService CommandService;
        public EventsHandler(ServerHandler ServerParam, ConfigHandler ConfigParam, DiscordSocketClient ClientParam, CommandService CmdParam, Random RandomParam)
        {
            Random = RandomParam;
            Client = ClientParam;
            ConfigHandler = ConfigParam;
            CommandService = CmdParam;
            ServerHandler = ServerParam;
        }

        public async Task InitializeAsync(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), IServiceProvider);
        }

        internal async Task ReadyAsync()
        {
            LogClient.Write(Source.DISCORD, "Ready to rock n roll !");
            await Client.SetActivityAsync(new Game(!ConfigHandler.Config.Games.Any() ?
                $"{ConfigHandler.Config.Prefix}Help" : $"{ConfigHandler.Config.Games[Random.Next(ConfigHandler.Config.Games.Count)]}", ActivityType.Playing));
        }

        internal Task LatencyUpdatedAsync(int Old, int Newer)
            => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 500) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 200) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            ServerHandler.AddServer(Guild.Id, Guild.Name);
            await Guild.DefaultChannel.SendMessageAsync("Thank you for inviting me to your server! Guild prefix is `!!`. Type `!!Cmds` for commands.");
        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {
            _ = CleanupAsync(User);
            var Config = ServerHandler.GetServer(User.Guild.Id);
            string WelcomeMessage = !Config.LeaveMessages.Any() ? $"We have lost **{User}**. Type F to pay respects." :
                StringExt.Replace(Config.LeaveMessages[Random.Next(0, Config.LeaveMessages.Count)], User.Guild.Name, User.Username);
            var Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.LeaveChannel));
            if (Channel != null) await Channel.SendMessageAsync(WelcomeMessage).ConfigureAwait(false);
        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = ServerHandler.GetServer(User.Guild.Id);
            string WelcomeMessage = !Config.JoinMessages.Any() ? $"Welcome {User}. We were expecting you ( ͡° ͜ʖ ͡°)" :
                StringExt.Replace(Config.JoinMessages[Random.Next(0, Config.JoinMessages.Count)], User.Guild.Name, User.Mention);
            var Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.JoinChannel));
            if (Channel != null) await Channel.SendMessageAsync(WelcomeMessage).ConfigureAwait(false);
            var Role = User.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole));
            if (Role != null) await User.AddRoleAsync(Role).ConfigureAwait(false);
        }

        internal Task HandleMessageAsync(SocketMessage Message)
        {
            var Config = ServerHandler.GetServer((Message.Channel as SocketGuildChannel).Guild.Id);
            if (!(Message is SocketUserMessage Msg) || !(Message.Author is SocketGuildUser User)) return Task.CompletedTask;
            if (Msg.Source != MessageSource.User || Msg.Author.IsBot || ConfigHandler.Config.UsersBlacklist.ContainsKey(User.Id) ||
                (Config.Profiles.ContainsKey(User.Id) && Config.Profiles[User.Id].IsBlacklisted)) return Task.CompletedTask;
            _ = AutoModAsync(Msg, Config);
            _ = AFKHandlerAsync(Msg, Config);
            _ = CleverbotHandlerAsync(Msg, Config);
            _ = XpHandlerAsync(Message, Config);
            return Task.CompletedTask;
        }

        internal async Task HandleCommandAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new IContext(Client, Msg, Provider);
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User || Msg.Author.IsBot ||
                Context.Config.UsersBlacklist.ContainsKey(Msg.Author.Id) || (Context.Server.Profiles.ContainsKey(Msg.Author.Id) && Context.Server.Profiles[Msg.Author.Id].IsBlacklisted)) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            switch (Result.Error)
            {
                case CommandError.Exception: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.Unsuccessful: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
            }
        }

        internal Task LeftGuildAsync(SocketGuild Guild) => Task.Run(() => ServerHandler.RemoveServer(Guild.Id));

        internal Task GuildAvailableAsync(SocketGuild Guild) => Task.Run(() => ServerHandler.AddServer(Guild.Id, Guild.Name));

        internal Task LogAsync(LogMessage Log) => Task.Run(() => LogClient.Write(Source.DISCORD, Log.Message ?? Log.Exception.Message));

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerHandler.GetServer(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Message == null || StarboardChannel == null || Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content)) Embed.WithDescription(Message.Content);
            if (Message.Attachments.FirstOrDefault() != null) Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);

            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == $"{Message.Id}");
            if (Config.Starboard.StarboardMessages.Contains(Exists))
            {
                Exists.Stars++;
                var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
                await SMsg.ModifyAsync(x =>
                {
                    x.Content =
                    $"{StringExt.StarType(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            }
            else
            {
                var Msg = await StarboardChannel.SendMessageAsync(
                    $"{StringExt.StarType(Message.Reactions.Count)}{Message.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention} ID: {Reaction.MessageId}", embed: Embed.Build());
                Config.Starboard.StarboardMessages.Add(new StarboardMessages
                {
                    ChannelId = $"{Message.Channel.Id}",
                    StarboardMessageId = $"{Msg.Id}",
                    MessageId = $"{Message.Id}",
                    Stars = 1
                });
            }
            ServerHandler.Save(Config, Guild.Id);
        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = ServerHandler.GetServer(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Message == null || StarboardChannel == null) return;
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
            if (!string.IsNullOrWhiteSpace(Message.Content)) Embed.WithDescription(Message.Content);
            if (Message.Attachments.FirstOrDefault() != null) Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);

            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == $"{Message.Id}");
            if (!Config.Starboard.StarboardMessages.Contains(Exists)) return;
            Exists.Stars--;
            var SMsg = await StarboardChannel.GetMessageAsync(Convert.ToUInt64(Exists.StarboardMessageId), CacheMode.AllowDownload) as IUserMessage;
            if (Message.Reactions.Count > 0) await SMsg.ModifyAsync(x =>
                 {
                     x.Content =
                     $"{StringExt.StarType(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                     x.Embed = Embed.Build();
                 });
            else
            {
                Config.Starboard.StarboardMessages.Remove(Exists);
                await SMsg.DeleteAsync();
            }
            ServerHandler.Save(Config, Guild.Id);
        }

        Task CleanupAsync(SocketGuildUser User)
        {
            var Config = ServerHandler.GetServer(User.Guild.Id);
            if (!(Config.Profiles.ContainsKey(User.Id))) return Task.CompletedTask;
            Config.Profiles.Remove(User.Id);
            Config.Tags.RemoveAll(x => x.Owner == $"{User.Id}");
            ServerHandler.Save(Config, User.Guild.Id);
            return Task.CompletedTask;
        }

        Task XpHandlerAsync(SocketMessage Message, ServerModel Config)
        {
            var User = Message.Author as IGuildUser;
            var BlacklistedRoles = new List<ulong>(Config.ChatXP.ForbiddenRoles.Select(x => Convert.ToUInt64(x)));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (HasRole || !Config.ChatXP.IsEnabled) return Task.CompletedTask;
            if (!Config.Profiles.ContainsKey(User.Id))
            {
                Config.Profiles.Add(User.Id, new UserProfile { ChatXP = Random.Next(Message.Content.Length), DailyReward = DateTime.Now });
                ServerHandler.Save(Config, User.Guild.Id);
                return Task.CompletedTask;
            }
            int Old = Config.Profiles[User.Id].ChatXP;
            Config.Profiles[User.Id].ChatXP += Random.Next(Message.Content.Length);
            var New = Config.Profiles[User.Id].ChatXP;
            ServerHandler.Save(Config, User.Guild.Id);
            return LevelUpAsync(Message, Config, Old, New);
        }

        async Task WarnAsync((bool, string) TupleParam, SocketMessage Message)
        {
            await Message.DeleteAsync();
            await Message.Channel.SendMessageAsync(TupleParam.Item2);
        }

        async Task AFKHandlerAsync(SocketMessage Message, ServerModel Config)
        {
            if (!Message.MentionedUsers.Any(x => Config.AFKUsers.ContainsKey(x.Id))) return;
            string Reason = null;
            var User = Message.MentionedUsers.FirstOrDefault(u => Config.AFKUsers.TryGetValue(u.Id, out Reason));
            if (User != null) await Message.Channel.SendMessageAsync($"**{User.Username} has left an AFK Message:**  {Reason}");
        }

        async Task AutoModAsync(SocketUserMessage Message, ServerModel Config)
        {
            if (!Config.ModLog.IsAutoModEnabled || Message.Author.Id == (Message.Channel as SocketGuildChannel).Guild.Owner.Id || Config.ModLog.MaxWarnings == 0) return;
            var BadWords = BoolExt.IsMatch(Config.ModLog.BadWords, Message.Content, $"{Message.Author.Mention}, your message has been removed for containing a banned word.");
            var BadUrls = BoolExt.IsMatch(Config.ModLog.BlockedUrls, Message.Content, $"{Message.Author.Mention}, your message has been removed for containing a blocked url.");
            if (!BadWords.Item1 || !BadUrls.Item1) return;
            _ = WarnAsync(BadWords, Message);
            _ = WarnAsync(BadUrls, Message);
            if (!Config.Profiles.ContainsKey(Message.Author.Id))
            {
                Config.Profiles.Add(Message.Author.Id, new UserProfile { Warnings = 1, DailyReward = DateTime.Now });
                ServerHandler.Save(Config, (Message.Channel as SocketGuildChannel).Guild.Id);
                return;
            }

            if (!(Config.Profiles[Message.Author.Id].Warnings >= Config.ModLog.MaxWarnings)) Config.Profiles[Message.Author.Id].Warnings++;
            else
            {
                await (Message.Author as SocketGuildUser).KickAsync("Kicked by Auto Mod.");
                var Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ModLog.TextChannel));
                IUserMessage Msg = null;
                if (Channel != null)
                    Msg = await Channel.SendMessageAsync($"**Kick** | Case {Config.ModLog.Cases.Count + 1 }\n**User:** {Message.Author} ({Message.Author.Id})\n**Reason:** Maxed out warnings.\n" +
                    $"**Responsible Moderator:** Action Taken by Auto Moderator.");
                if (Msg == null)
                    await (await (Message.Author as IGuildUser).Guild.GetDefaultChannelAsync()).SendMessageAsync($"*{Message.Author} was kicked by Auto Moderator.*  :cop:");

                Config.ModLog.Cases.Add(new CaseWrapper
                {
                    MessageId = $"{Msg.Id}",
                    CaseType = CaseType.AutoMod,
                    Reason = "Maxed out warnings.",
                    ResponsibleMod = $"Auto Moderator",
                    CaseNumber = Config.ModLog.Cases.Count + 1,
                    UserInfo = $"{Message.Author} ({Message.Author.Id})"
                });
            }
            ServerHandler.Save(Config, (Message.Channel as SocketGuildChannel).Guild.Id);
        }

        async Task CleverbotHandlerAsync(SocketMessage Message, ServerModel Config)
        {
            if (string.IsNullOrWhiteSpace(Config.ChatterChannel)) return;
            var Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ChatterChannel));
            if (Channel == null || Message.Channel != Channel || !Message.Content.ToLower().StartsWith("Valerie")) return;
            var Clever = await ConfigHandler.Cookie.Cleverbot.TalkAsync(Message.Content);
            await Channel.SendMessageAsync(Clever.Ouput ?? "Mehehehe, halo m8.").ConfigureAwait(false);
        }

        async Task LevelUpAsync(SocketMessage Message, ServerModel Config, int OldXp, int NewXp)
        {
            var User = Message.Author as SocketGuildUser;
            int OldLevel = IntExt.GetLevel(OldXp);
            int NewLevel = IntExt.GetLevel(NewXp);
            if (!(NewLevel > OldLevel)) return;
            ServerHandler.MemoryUpdate(User.Guild.Id, User.Id, (int)Math.Sqrt(NewXp) / NewLevel);
            if (!string.IsNullOrWhiteSpace(Config.ChatXP.LevelMessage))
                await Message.Channel.SendMessageAsync(StringExt.Replace(Config.ChatXP.LevelMessage, User: $"{User}", Level: NewLevel, Bytes: Math.Pow(Math.Sqrt(NewXp), NewLevel)));
            if (!Config.ChatXP.LevelRoles.Any()) return;
            var Role = User.Guild.GetRole(Config.ChatXP.LevelRoles.Where(x => x.Value == NewLevel).FirstOrDefault().Key);
            if (User.Roles.Contains(Role) || !User.Guild.Roles.Contains(Role)) return;
            await User.AddRoleAsync(Role);
            foreach (var lvlrole in Config.ChatXP.LevelRoles)
                if (lvlrole.Value < NewLevel)
                    if (!User.Roles.Contains(User.Guild.GetRole(lvlrole.Key)))
                        await User.AddRoleAsync(User.Guild.GetRole(lvlrole.Key));
        }
    }
}