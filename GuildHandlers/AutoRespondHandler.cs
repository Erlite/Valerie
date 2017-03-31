﻿using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using DiscordBot.Interfaces;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Discord.Commands;
using System.Linq;

namespace DiscordBot.GuildHandlers
{
    public class AutoRespondHandler : IHandler
    {
        private GuildHandler GuildHandler;
        private CommandContext context;

        public AutoRespondHandler(GuildHandler GuildHandler)
        {
            this.GuildHandler = GuildHandler;
        }

        public async Task InitializeAsync()
        {
            GuildHandler.MainHandler.Client.MessageReceived += HandleAutoRespondAsync;
            await Task.CompletedTask;
        }

        public Task Close()
        {
            GuildHandler.MainHandler.Client.MessageReceived -= HandleAutoRespondAsync;
            return Task.CompletedTask;
        }

        public async Task SaveResponsesAsync()
        {
            await Task.Run(() =>
            {
                Dictionary<string, string> temp = new Dictionary<string, string>();
                File.WriteAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json", JsonConvert.SerializeObject(temp, Formatting.Indented));
            });
        }

        public Dictionary<string, string> LoadResponsesAsync()
        {
            //await Task.Run(() =>
            //{
            //Dictionary<string, string> temp;
                //temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json"));
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"Configs{Path.DirectorySeparatorChar}Guilds{Path.DirectorySeparatorChar}{GuildHandler.Guild.Id}{Path.DirectorySeparatorChar}Responses.json"));
            //});
        }


    public async Task HandleAutoRespondAsync(SocketMessage message)
        {
            var msg = message as SocketUserMessage;
            if (msg == null) return;
            if (msg.Author.IsBot) return;
            
            //if (msg.Channel != message.Channel) return;
            //if (msg == null || msg.Author.IsBot || msg.Author.Id == client.CurrentUser.Id || !(msg.Channel is ITextChannel)) return;
            var channel = msg.Channel as ITextChannel;            
            var autorespond = GuildHandler.MainHandler.GuildConfigHandler(channel.Guild).GetAutoRespond();
            if (autorespond.IsEnabled)
            {
                var load = LoadResponsesAsync();
                {
                    foreach (KeyValuePair<string, string> item in load.Where(x => x.Key.Contains(msg.ToString())))
                    {
                        if (msg.Content.Contains(item.Key))
                        {
                            await msg.Channel.SendMessageAsync(item.Value);
                        }
                    }
                }
            }
        }
    }
}
