using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Models;
using Valerie.Handlers.ModuleHandler;
using Valerie.Services;
using Discord.Commands;
using System.Reflection;
using System.Linq;
using Valerie.Extensions;

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
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        internal Task LogAsync(LogMessage Log) => Task.Run(() => LogClient.Write(Source.DISCORD, Log.Message ?? Log.Exception.Message));

        internal async Task GuildAvailableAsync(SocketGuild Guild) => await ServerHandler.AddServerAsync(new ServerModel { Id = $"{Guild.Id}", Prefix = "." }).ConfigureAwait(false);

        internal async Task LeftGuildAsync(SocketGuild Guild) => await ServerHandler.DeleteServerAsync(Guild.Id).ConfigureAwait(false);

        internal async Task ReadyAsync()
        {
            var Config = await ConfigHandler.GetConfigAsync();
            string Game = !Config.Games.Any() ? $"{Config.Prefix}" : $"{Config.Games[Random.Next(0, Config.Games.Count)]}";
            await Client.SetGameAsync(Game);
        }

        internal Task LatencyUpdatedAsync(int Old, int Newer)
            => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 150) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 100) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            await ServerHandler.AddServerAsync(new ServerModel
            {
                Id = $"{Guild.Id}",
                Prefix = "."
            }).ConfigureAwait(false);
        }

        internal async Task HandleCommandAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new IContext(Client, Msg, Provider);
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User || Msg.Author.IsBot ||
                Context.Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, null, MultiMatchHandling.Best);
            switch (Result.Error)
            {
                case CommandError.Exception: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.Unsuccessful: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
            }
        }

        internal async Task HandleMessageAsync(SocketMessage Message)
        {
            var Config = await ConfigHandler.GetConfigAsync();
            if (!(Message is SocketUserMessage Msg) || !(Message.Author is SocketGuildUser User)) return;
            if (Msg.Source != MessageSource.User | Msg.Author.IsBot || Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            _ = AFKHandlerAsync(Msg);
            _ = CleverbotHandlerAsync(Msg);
        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = await ServerHandler.GetServerAsync(User.Guild.Id);
            string WelcomeMessage = !Config.JoinMessages.Any() ? $"Welcomoe {User}. We were expecting you ( ͡° ͜ʖ ͡°)" :
                StringExt.Replace(Config.JoinMessages[Random.Next(0, Config.JoinMessages.Count)], User.Guild.Name, User.Mention);
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.JoinChannel));
            if (Channel != null) await Channel.SendMessageAsync(WelcomeMessage).ConfigureAwait(false);
            var Role = User.Guild.GetRole(Convert.ToUInt64(Config.ModLog.AutoAssignRole));
            if (Role != null) await User.AddRoleAsync(Role).ConfigureAwait(false);
        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {
            _ = CleanupAsync(User);
            var Config = await ServerHandler.GetServerAsync(User.Guild.Id);
            string WelcomeMessage = !Config.LeaveMessages.Any() ? $"{User} down. I REPEAT! {User} DOWNN!" :
                StringExt.Replace(Config.LeaveMessages[Random.Next(0, Config.LeaveMessages.Count)], User.Guild.Name, User.Username);
            ITextChannel Channel = User.Guild.GetTextChannel(Convert.ToUInt64(Config.LeaveChannel));
            if (Channel != null) await Channel.SendMessageAsync(WelcomeMessage).ConfigureAwait(false);
        }

        internal Task UserBannedAsync(SocketUser User, SocketGuild Guild)
        {
            return Task.CompletedTask;
        }

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = await ServerHandler.GetServerAsync(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));

            if (Message == null || StarboardChannel == null || Reaction.Channel.Id == Convert.ToUInt64(Config.Starboard.TextChannel)) return;
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));

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
            await ServerHandler.UpdateServerAsync(Guild.Id, Config);
        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = await ServerHandler.GetServerAsync(Guild.Id);
            ITextChannel StarboardChannel = Guild.GetTextChannel(Convert.ToUInt64(Config.Starboard.TextChannel));
            if (Message == null || StarboardChannel == null) return;
            var Embed = ValerieEmbed.Embed(EmbedColor.Yellow, Message.Author.GetAvatarUrl(), Message.Author.Username, FooterText: Message.Timestamp.ToString("F"));
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
                    $"{StringExt.StarType(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            else
            {
                Config.Starboard.StarboardMessages.Remove(Exists);
                await SMsg.DeleteAsync();
            }
            await ServerHandler.UpdateServerAsync(Guild.Id, Config);
        }

        async Task AFKHandlerAsync(SocketMessage Message)
        {
            var Config = await ServerHandler.GetServerAsync((Message.Channel as SocketGuildChannel).Guild.Id);
            string Reason = null;
            var User = Message.MentionedUsers.FirstOrDefault(u => Config.AFKUsers.TryGetValue(u.Id, out Reason));
            if (User != null)
                await Message.Channel.SendMessageAsync($"Message left by {User}: {Reason}");
        }

        async Task CleverbotHandlerAsync(SocketMessage Message)
        {
            var Config = await ServerHandler.GetServerAsync((Message.Channel as SocketGuildChannel).Guild.Id);
            ITextChannel Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Convert.ToUInt64(Config.ChatterChannel));
            int ArgPos = 0;
            if (Message.Author.IsBot || Channel == null || !(Message as SocketUserMessage).HasMentionPrefix(Client.CurrentUser, ref ArgPos) || Message.Channel != Channel) return;
            string UserMsg = Message.Content.Replace($"<@{Client.CurrentUser.Id}>", string.Empty);
            var Clever = await MainHandler.Cookie.Cleverbot.TalkAsync(UserMsg);
            await Channel.SendMessageAsync(Clever.Ouput ?? "Mehehehe, halo m8.").ConfigureAwait(false);
        }

        async Task AutoRespondAsync(SocketMessage Message)
        {
            var Config = await ServerHandler.GetServerAsync((Message.Channel as SocketGuildChannel).Guild.Id);
            var Reply = Config.AutoResponds.FirstOrDefault(x => Message.Content.StartsWith(x.Key));
            await Message.Channel.SendMessageAsync(Reply.Value);
        }

        async Task CleanupAsync(SocketGuildUser User)
        {
            var Config = await ServerHandler.GetServerAsync(User.Guild.Id);
            if (!(Config.AFKUsers.ContainsKey(User.Id) || Config.ChatXP.Rankings.ContainsKey(User.Id))) return;
            Config.AFKUsers.Remove(User.Id);
            Config.ChatXP.Rankings.Remove(User.Id);
            Config.Tags.RemoveAll(x => x.Owner == $"{User.Id}");
            await ServerHandler.UpdateServerAsync(User.Guild.Id, Config);
        }
    }
}