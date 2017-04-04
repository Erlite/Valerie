using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using DiscordBot.Interfaces;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;
using System.Linq;
using DiscordBot.Services;
using DiscordBot.Handlers;
using System;

namespace DiscordBot.GuildHandlers
{
    public class LogHandler : IHandler
    {
        private GuildHandler GuildHandler;
        private CommandContext context;

        public LogHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task InitializeAsync()
        {
            GuildHandler.MainHandler.Client.MessageReceived += AutoRespondAsync;
            GuildHandler.MainHandler.Client.UserJoined += UserJoinAsync;
            GuildHandler.MainHandler.Client.UserLeft += UserLeftAsync;
            GuildHandler.MainHandler.Client.UserBanned += UserBannedAsync;
            await Task.CompletedTask;
        }

        public Task Close()
        {
            GuildHandler.MainHandler.Client.MessageReceived -= AutoRespondAsync;
            GuildHandler.MainHandler.Client.UserJoined -= UserJoinAsync;
            GuildHandler.MainHandler.Client.UserLeft -= UserLeftAsync;
            GuildHandler.MainHandler.Client.UserBanned -= UserBannedAsync;
            return Task.CompletedTask;
        }

        public async Task SaveResponsesAsync()
        {
            Dictionary<string, string> temp = new Dictionary<string, string>();
            File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json", JsonConvert.SerializeObject(temp, Formatting.Indented));
        }

        public Dictionary<string, string> LoadResponsesAsync()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json"));
        }

        public async Task AutoRespondAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            var guild = context.Guild as SocketGuild;
            var channel = msg.Channel as ITextChannel;
            if (msg == null) return;
            if (msg.Author.IsBot) return;
            var autorespond = GuildHandler.MainHandler.GuildConfigHandler(channel.Guild).GetAutoRespond();
            if (autorespond.IsEnabled)
            {
                var load = LoadResponsesAsync();
                    foreach (KeyValuePair<string, string> item in load.Where(x => x.Key.Contains(msg.ToString())))
                    {
                        if (msg.Content.Contains(item.Key))
                        {
                            await msg.Channel.SendMessageAsync(item.Value);
                        }
                    }
                
            }
        }

        private async Task UserBannedAsync(SocketUser user, SocketGuild gld)
        {
            var usr = user as SocketGuildUser;
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = user.Username;
                    x.IconUrl = user.GetAvatarUrl();
                })
                .WithDescription($"{user.Username} has been banned from {gld.Name} by {context.User}")
                .WithColor(new Color())
                .WithFooter(x =>
                {
                    x.Text = $"Banned on {DateTime.Now.ToString()}";
                });
            var Log = GuildHandler.MainHandler.GuildConfigHandler(usr.Guild).EventsLogging();
            if (Log.BanLog)
            {
                var channel = usr.Guild.GetChannel(Log.TextChannel) as ITextChannel;
                await channel.SendMessageAsync("", embed: embed);
            }

        }

        private async Task UserLeftAsync(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{user.Username}#{user.Discriminator} has left the server :wave:";
            embed.Color = new Color(83, 219, 207);
            var Log = GuildHandler.MainHandler.GuildConfigHandler(user.Guild).EventsLogging();
            if (Log.LeaveLog)
            {
                var channel = user.Guild.GetChannel(Log.TextChannel) as ITextChannel;
                await channel.SendMessageAsync("", embed: embed);
            }
        }

        private async Task UserJoinAsync(SocketGuildUser user)
        {
            var embed = new EmbedBuilder();
            embed.Title = "=== User Joined ===";
            embed.Description = $"**Username: **{user.Username}#{user.Discriminator}\n{GuildHandler.ConfigHandler.GetWelcomeMessage()}";
            embed.Color = new Color(83, 219, 207);
            var Log = GuildHandler.MainHandler.GuildConfigHandler(user.Guild).EventsLogging();
            if (Log.JoinLog)
            {
                var channel = user.Guild.GetChannel(Log.TextChannel) as ITextChannel;
                await channel.SendMessageAsync("", embed: embed);
            }
        }
    }
}
