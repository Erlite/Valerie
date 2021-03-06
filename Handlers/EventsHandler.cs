﻿using System;
using Discord;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using Valerie.Services;
using System.Reflection;
using System.Threading;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using CC = System.Drawing.Color;
using static Valerie.Addons.Embeds;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        Random Random { get; }
        GuildHelper GuildHelper { get; }
        EventHelper EventHelper { get; }
        DiscordSocketClient Client { get; }
        GuildHandler GuildHandler { get; }
        IServiceProvider Provider { get; set; }
        ConfigHandler ConfigHandler { get; }
        WebhookService WebhookService { get; }
        CommandService CommandService { get; }
        CancellationTokenSource CancellationToken { get; set; }

        public EventsHandler(GuildHandler guild, ConfigHandler config, DiscordSocketClient client, CommandService command,
            Random random, GuildHelper guildH, WebhookService webhookS, EventHelper eventHelper)
        {
            Client = client;
            Random = random;
            GuildHandler = guild;
            GuildHelper = guildH;
            ConfigHandler = config;
            EventHelper = eventHelper;
            CommandService = command;
            WebhookService = webhookS;
            CancellationToken = new CancellationTokenSource();
        }

        public async Task InitializeAsync(IServiceProvider ServiceProvider)
        {
            Provider = ServiceProvider;
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
        }

        internal Task Ready() => Task.Run(() =>
        {
            LogService.Write(LogSource.RDY, "Drum Roll, Please?", CC.GreenYellow);
            Client.SetActivityAsync(new Game(!ConfigHandler.Config.Games.Any() ?
                            $"{ConfigHandler.Config.Prefix}Help" : $"{ConfigHandler.Config.Games[Random.Next(ConfigHandler.Config.Games.Count)]}", ActivityType.Playing));
        });

        internal Task LeftGuild(SocketGuild Guild) => Task.Run(() => GuildHandler.RemoveGuild(Guild.Id, Guild.Name));

        internal Task GuildAvailable(SocketGuild Guild) => Task.Run(() => GuildHandler.AddGuild(Guild.Id, Guild.Name));

        internal Task Connected()
        {
            CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            LogService.Write(LogSource.CNN, "Connected to Discord.", CC.BlueViolet);
            return Task.CompletedTask;
        }

        internal Task Log(LogMessage log) => Task.Run(() => LogService.Write(LogSource.EXC, log.Message ?? log.Exception.Message, CC.Crimson));

        internal Task Disconnected(Exception Error)
        {
            _ = Task.Delay(EventHelper.GlobalTimeout, CancellationToken.Token).ContinueWith(async _ =>
            {
                LogService.Write(LogSource.DSN, $"Checking connection state...", CC.LightYellow);
                await EventHelper.CheckStateAsync();
            });
            return Task.CompletedTask;
        }

        internal Task LatencyUpdated(int Old, int Newer) => Client.SetStatusAsync((Client.ConnectionState == ConnectionState.Disconnected || Newer > 500) ? UserStatus.DoNotDisturb
                : (Client.ConnectionState == ConnectionState.Connecting || Newer > 250) ? UserStatus.Idle
                : (Client.ConnectionState == ConnectionState.Connected || Newer < 100) ? UserStatus.Online : UserStatus.AFK);

        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            GuildHandler.AddGuild(Guild.Id, Guild.Name);
            await Guild.DefaultChannel.SendMessageAsync(ConfigHandler.Config.JoinMessage ?? "Thank you for inviting me to your server!");
        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GetGuild(User.Guild.Id);
            await WebhookService.SendMessageAsync(new WebhookOptions
            {
                Name = Client.CurrentUser.Username,
                Webhook = Config.LeaveWebhook,
                Message = !Config.LeaveMessages.Any() ? $"**{User.Username}** abandoned us! {Emotes.Squint}"
                : StringHelper.Replace(Config.LeaveMessages[Random.Next(0, Config.LeaveMessages.Count)], User.Guild.Name, User.Username)
            });
        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {
            var Config = GuildHandler.GetGuild(User.Guild.Id);
            await WebhookService.SendMessageAsync(new WebhookOptions
            {
                Name = Client.CurrentUser.Username,
                Webhook = Config.JoinWebhook,
                Message = !Config.JoinMessages.Any() ? $"**{User.Username}** is here to rock our world! Yeah, baby!"
                : StringHelper.Replace(Config.JoinMessages[Random.Next(0, Config.JoinMessages.Count)], User.Guild.Name, User.Mention)
            });
            var Role = User.Guild.GetRole(Config.Mod.JoinRole);
            if (Role != null) await User.AddRoleAsync(Role).ConfigureAwait(false);
        }

        internal async Task CommandHandlerAsync(SocketMessage Message)
        {
            if (!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new IContext(Client, Msg, Provider);
            if (Context.Config.Blacklist.Contains(Msg.Author.Id) || GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id).IsBlacklisted
                || Msg.Author.IsBot) return;
            _ = Task.Run(() => EventHelper.RunTasks(Message, Context.Server));
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos) ||
                Msg.HasMentionPrefix(Client.CurrentUser, ref argPos)) || Msg.Source != MessageSource.User) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, Provider, MultiMatchHandling.Best);
            var Search = CommandService.Search(Context, argPos);
            var Command = Search.IsSuccess ? Search.Commands.FirstOrDefault().Command : null;
            switch (Result.Error)
            {
                case CommandError.Exception: LogService.Write(LogSource.EXC, Result.ErrorReason, CC.Crimson); break;
                case CommandError.UnmetPrecondition:
                    if (!Result.ErrorReason.Contains("SendMessages")) await Context.Channel.SendMessageAsync(Result?.ErrorReason); break;
                case CommandError.BadArgCount:
                    string Name = Command.Module.Group != null && Command.Name.Contains("Async")
                        ? Command.Module.Group : $"{Command.Module.Group ?? null} {Command.Name}";
                    await Context.Channel.SendMessageAsync($"**Usage:** {Context.Config.Prefix}{Name} {StringHelper.ParametersInfo(Command.Parameters)}");
                    break;
            }
            _ = Task.Run(() => EventHelper.RecordCommand(Command, Context));
        }

        internal async Task MessageDeletedAsync(Cacheable<IMessage, ulong> Cache, ISocketMessageChannel Channel)
        {
            var Config = GuildHandler.GetGuild((Channel as SocketGuildChannel).Guild.Id);
            var Message = await Cache.GetOrDownloadAsync();
            if (Message == null || Config == null || !Config.Mod.LogDeletedMessages || Message.Author.IsBot) return;
            Config.DeletedMessages.Add(new MessageWrapper
            {
                ChannelId = Channel.Id,
                MessageId = Message.Id,
                AuthorId = Message.Author.Id,
                DateTime = Message.Timestamp.DateTime,
                Content = Message.Content ?? Message.Attachments.FirstOrDefault()?.Url
            });
            GuildHandler.Save(Config);
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
                .WithAuthor(Message.Author.Username, Message.Author.GetAvatarUrl())
                .WithFooter(Message.Timestamp.ToString("F"));
            var ReactionCount = Message.Reactions.Count(x => x.Key.Name == "⭐");
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
                    $"{StringHelper.Star(ReactionCount)}{ReactionCount} {(Reaction.Channel as ITextChannel).Mention} ID: {Reaction.MessageId}", embed: Embed.Build());
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
                .WithAuthor(Message.Author.Username, Message.Author.GetAvatarUrl())
                .WithFooter(Message.Timestamp.ToString("F"));
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
    }
}