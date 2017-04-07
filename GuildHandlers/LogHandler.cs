using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Meeseeks.Interfaces;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;
using System.Linq;
using System;
using Rick.Classes;

namespace Meeseeks.GuildHandlers
{
    public class LogHandler : IHandler
    {
        private GuildHandler GuildHandler;
        private CommandContext context;
        private Dictionary<string, Response> Resp = null;

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
            await Task.Run(() =>
            {
                Dictionary<string, Response> temp = new Dictionary<string, Response>();
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json", JsonConvert.SerializeObject(temp, Formatting.Indented));
            });
        }

        public async Task LoadResponsesAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<string, Response> Responses;
                Responses = JsonConvert.DeserializeObject<Dictionary<string, Response>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json"));
                Resp = Responses;
            });
        }

        public void CreateResponse(string trigger, string response, IUser user)
        {
            Resp[trigger.ToLower()] = new Response(trigger, response, user.Id);
        }

        public async Task AutoRespondAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            var guild = context.Guild as SocketGuild;
            var channel = msg.Channel as ITextChannel;
            if (msg == null || msg.Author.IsBot || msg.Channel.Id == 0) return;
            var autorespond = GuildHandler.MainHandler.GuildConfigHandler(channel.Guild).GetAutoRespond();
            if (autorespond.IsEnabled)
            {
                foreach (KeyValuePair<string, Response> item in Resp.Where(x=>x.Key.Contains(msg.ToString())))
                {
                    if (msg.Content.Contains(item.Key))
                    {
                        await msg.Channel.SendMessageAsync(item.Value.ToString());
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
