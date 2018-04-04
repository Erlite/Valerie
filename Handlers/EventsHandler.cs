using System;
using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Valerie.Services;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Valerie.Addons.Embeds;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        Random Random { get; }
        GuildHelper GuildHelper { get; }
        GuildHandler GuildHandler { get; }
        DiscordSocketClient Client { get; }
        ConfigHandler ConfigHandler { get; }
        IServiceProvider Provider { get; set; }
        CommandService CommandService { get; }
        public EventsHandler(GuildHandler guild, ConfigHandler config, DiscordSocketClient client, CommandService command, Random random, GuildHelper guildHelper)
        {
            Client = client;
            Random = random;
            GuildHandler = guild;
            ConfigHandler = config;
            GuildHelper = guildHelper;
            CommandService = command;
        }

        public async Task InitializeAsync(IServiceProvider ServiceProvider)
        {
            Provider = ServiceProvider;
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
        }

        internal Task Ready() => Task.Run(() =>
        {
            LogService.Write(nameof(Ready), "Ready to pound your pussy.", ConsoleColor.Green);
            Client.SetActivityAsync(new Game(!ConfigHandler.Config.Games.Any() ?
                            $"{ConfigHandler.Config.Prefix}Help" : $"{ConfigHandler.Config.Games[Random.Next(ConfigHandler.Config.Games.Count)]}", ActivityType.Playing));
        });
        internal Task LeftGuild(SocketGuild Guild) => Task.Run(() => GuildHandler.RemoveGuild(Guild.Id, nameof(LeftGuild), Guild.Name));
        internal Task GuildAvailable(SocketGuild Guild) => Task.Run(() => GuildHandler.AddGuild(Guild.Id, nameof(GuildAvailable), Guild.Name));
        internal Task Connected() => Task.Run(() => LogService.Write(nameof(Connected), "Givin thicc booty some of that succ.", ConsoleColor.Green));
        internal Task Log(LogMessage log) => Task.Run(() => LogService.Write(nameof(Log), log.Message ?? log.Exception.Message, ConsoleColor.Yellow));
        internal Task Disconnected(Exception Error) => Task.Run(() => LogService.Write(nameof(Disconnected), $"Disconnected from Discord: {Error.Message ?? Error.StackTrace}", ConsoleColor.Red));
        internal Task LatencyUpdated(int Old, int Newer) => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 500) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 250) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            GuildHandler.AddGuild(Guild.Id, nameof(JoinedGuildAsync).Remove(10), Guild.Name);
            await Guild.DefaultChannel.SendMessageAsync(ConfigHandler.Config.ServerMessage ?? "Thank you for inviting me to your server. Your guild prefix is `!`. Type `!Cmds` for commands.");
        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GetGuild(User.Guild.Id);
            string Message = !Config.LeaveMessages.Any() ? $"**{User.Username}** abandoned us! {Emotes.DEyes}"
                : StringHelper.Replace(Config.LeaveMessages[Random.Next(0, Config.LeaveMessages.Count)], User.Guild.Name, User.Username);
            var Channel = User.Guild.GetTextChannel(Config.LeaveChannel);
            if (Channel != null) await Channel.SendMessageAsync(Message).ConfigureAwait(false);
        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GetGuild(User.Guild.Id);
            string Message = !Config.JoinMessages.Any() ? $"**{User.Username}** is here to rock our world! Yeah, baby!"
                : StringHelper.Replace(Config.JoinMessages[Random.Next(0, Config.JoinMessages.Count)], User.Guild.Name, User.Mention);
            var Channel = User.Guild.GetTextChannel(Config.JoinChannel);
            if (Channel != null) await Channel.SendMessageAsync(Message).ConfigureAwait(false);
            var Role = User.Guild.GetRole(Config.Mod.JoinRole);
            if (Role != null) await User.AddRoleAsync(Role).ConfigureAwait(false);
        }

        internal Task HandleMessageAsync(SocketMessage Message)
        {
            var Guild = (Message.Channel as SocketGuildChannel).Guild;
            var Config = GuildHandler.GetGuild(Guild.Id);
            if (!(Message is SocketUserMessage Msg) || !(Message.Author is SocketGuildUser User)) return Task.CompletedTask;
            if (Msg.Source != MessageSource.User || Msg.Author.IsBot || ConfigHandler.Config.Blacklist.ContainsKey(User.Id) ||
                GuildHelper.GetProfile(Guild.Id, Msg.Author.Id).IsBlacklisted) return Task.CompletedTask;
            _ = AFKHandlerAsync(Msg, Config);
            _ = XpHandlerAsync(Message, Config);
            _ = CleverbotHandlerAsync(Msg, Config);
            _ = AutoTagAsync(Msg, Config);
            return Task.CompletedTask;
        }

        internal async Task CommandHandlerAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new IContext(Client, Msg, Provider);
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User || Msg.Author.IsBot) return;
            if (Context.Config.Blacklist.ContainsKey(Msg.Author.Id) || GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id).IsBlacklisted) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            switch (Result.Error)
            {
                case CommandError.Exception: LogService.Write("Command Exception", Result.ErrorReason, ConsoleColor.Red); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
            }
            _ = Task.Run(() => RecordCommand(Context, argPos));
        }

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            var Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = GuildHandler.GetGuild(Guild.Id);
            var StarboardChannel = Guild.GetTextChannel(Config.Starboard.TextChannel) as IMessageChannel;
            if (Message == null || StarboardChannel == null || Reaction.Channel.Id == Config.Starboard.TextChannel) return;
            var Embed = GetEmbed(Paint.Yellow)
                .WithAuthor(x =>
                {
                    x.Name = Message.Author.Username;
                    x.IconUrl = Message.Author.GetAvatarUrl();
                })
                .WithFooter(x => { x.Text = Message.Timestamp.ToString("F"); });
            if (!string.IsNullOrWhiteSpace(Message.Content)) Embed.WithDescription(Message.Content);
            if (Message.Attachments.FirstOrDefault() != null) Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);
            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == Message.Id);
            if (Config.Starboard.StarboardMessages.Contains(Exists))
            {
                Exists.Stars++;
                var SMsg = await StarboardChannel.GetMessageAsync(Exists.StarboardMessageId, CacheMode.AllowDownload) as IUserMessage;
                await SMsg.ModifyAsync(x =>
                {
                    x.Content = $"{StringHelper.Star(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                    x.Embed = Embed.Build();
                });
            }
            else
            {
                var Msg = await StarboardChannel.SendMessageAsync(
                    $"{StringHelper.Star(Message.Reactions.Count)}{Message.Reactions.Count} {(Reaction.Channel as ITextChannel).Mention} ID: {Reaction.MessageId}", embed: Embed.Build());
                Config.Starboard.StarboardMessages.Add(new StarboardMessage
                {
                    Stars = 1,
                    MessageId = Message.Id,
                    StarboardMessageId = Msg.Id,
                    AuthorId = Message.Author.Id,
                    ChannelId = Message.Channel.Id
                });
            }
            GuildHandler.Save(Config);
        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {
            if (Reaction.Emote.Name != "⭐") return;
            SocketGuild Guild = (Reaction.Channel as SocketGuildChannel).Guild;
            var Message = await Cache.GetOrDownloadAsync();
            var Config = GuildHandler.GetGuild(Guild.Id);
            var StarboardChannel = Guild.GetTextChannel(Config.Starboard.TextChannel) as IMessageChannel;
            if (Message == null || StarboardChannel == null) return;
            var Embed = GetEmbed(Paint.Yellow)
                .WithAuthor(x =>
                {
                    x.Name = Message.Author.Username;
                    x.IconUrl = Message.Author.GetAvatarUrl();
                })
                .WithFooter(x => { x.Text = Message.Timestamp.ToString("F"); });
            if (!string.IsNullOrWhiteSpace(Message.Content)) Embed.WithDescription(Message.Content);
            if (Message.Attachments.FirstOrDefault() != null) Embed.WithImageUrl(Message.Attachments.FirstOrDefault().Url);
            var Exists = Config.Starboard.StarboardMessages.FirstOrDefault(x => x.MessageId == Message.Id);
            if (!Config.Starboard.StarboardMessages.Contains(Exists)) return;
            Exists.Stars--;
            var SMsg = await StarboardChannel.GetMessageAsync(Exists.StarboardMessageId, CacheMode.AllowDownload) as IUserMessage;
            if (Message.Reactions.Count > 0) await SMsg.ModifyAsync(x =>
            {
                x.Content =
                $"{StringHelper.Star(Exists.Stars)}{Exists.Stars} {(Reaction.Channel as ITextChannel).Mention} ID: {Exists.StarboardMessageId}";
                x.Embed = Embed.Build();
            });
            else
            {
                Config.Starboard.StarboardMessages.Remove(Exists);
                await SMsg.DeleteAsync();
            }
            GuildHandler.Save(Config);
        }

        internal void RecordCommand(IContext Context, int ArgPos)
        {
            var Search = CommandService.Search(Context, ArgPos);
            if (!Search.IsSuccess) return;
            var Command = Search.Commands.FirstOrDefault().Command;
            var Profile = GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (!Profile.Commands.ContainsKey(Command.Name)) Profile.Commands.Add(Command.Name, 0);
            Profile.Commands[Command.Name]++;
            GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
        }

        Task XpHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            var User = Message.Author as IGuildUser;
            var BlacklistedRoles = new List<ulong>(Config.ChatXP.ForbiddenRoles.Select(x => x));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (HasRole || !Config.ChatXP.IsEnabled) return Task.CompletedTask;
            var Profile = GuildHelper.GetProfile(User.GuildId, User.Id);
            int Old = Profile.ChatXP;
            Profile.ChatXP += Random.Next(Message.Content.Length);
            var New = Profile.ChatXP;
            GuildHelper.SaveProfile(Convert.ToUInt64(Config.Id), User.Id, Profile);
            return LevelUpHandlerAsync(Message, Config, Old, New);
        }

        async Task AFKHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Message.MentionedUsers.Any(x => Config.AFK.ContainsKey(x.Id))) return;
            string Reason = null;
            var User = Message.MentionedUsers.FirstOrDefault(u => Config.AFK.TryGetValue(u.Id, out Reason));
            if (User != null) await Message.Channel.SendMessageAsync($"**{User.Username} has left an AFK Message:**  {Reason}");
        }

        async Task CleverbotHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            var Channel = (Message.Channel as SocketGuildChannel).Guild.GetTextChannel(Config.ChatterChannel);
            if (Channel == null || Message.Channel != Channel || !Message.Content.ToLower().StartsWith("valerie")) return;
            var Clever = await ConfigHandler.Cookie.Cleverbot.TalkAsync(Message.Content.ToLower().Replace("valerie", string.Empty));
            await Channel.SendMessageAsync(Clever.CleverOutput).ConfigureAwait(false);
        }

        async Task LevelUpHandlerAsync(SocketMessage Message, GuildModel Config, int OldXp, int NewXp)
        {
            var User = Message.Author as SocketGuildUser;
            int OldLevel = IntHelper.GetLevel(OldXp);
            int NewLevel = IntHelper.GetLevel(NewXp);
            if (!(NewLevel > OldLevel)) return;
            int Crystals = (int)Math.Sqrt(Math.PI * NewXp);
            var Profile = GuildHelper.GetProfile(User.Guild.Id, User.Id);
            Profile.Crystals += Crystals;
            GuildHelper.SaveProfile(User.Guild.Id, User.Id, Profile);
            if (!string.IsNullOrWhiteSpace(Config.ChatXP.LevelMessage))
                await Message.Channel.SendMessageAsync(StringHelper.Replace(Config.ChatXP.LevelMessage, User: $"{User}", Level: NewLevel, Crystals: Crystals));
            if (!Config.ChatXP.LevelRoles.Any()) return;
            var Role = User.Guild.GetRole(Config.ChatXP.LevelRoles.Where(x => x.Value == NewLevel).FirstOrDefault().Key);
            if (User.Roles.Contains(Role) || !User.Guild.Roles.Contains(Role)) return;
            await User.AddRoleAsync(Role);
            foreach (var lvlrole in Config.ChatXP.LevelRoles)
                if (lvlrole.Value < NewLevel) if (!User.Roles.Contains(User.Guild.GetRole(lvlrole.Key))) await User.AddRoleAsync(User.Guild.GetRole(lvlrole.Key));
        }

        Task AutoTagAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Config.Tags.Any(x => x.AutoRespond == true)) return Task.CompletedTask;
            var Tags = Config.Tags.Where(x => x.AutoRespond == true);
            var Content = Tags.FirstOrDefault(x => Message.Content.StartsWith(x.Name));
            if (Content != null) return Message.Channel.SendMessageAsync(Content.Content);
            return Task.CompletedTask;
        }
    }
}