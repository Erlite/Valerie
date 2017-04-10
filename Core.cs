﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private MainHandler MainHandler;
        private CommandHandler handler;
        private InteractiveService Interactive;


        public async Task StartAsync()
        {            
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 9000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            client.Log += (l) => Task.Run(() => ConsoleService.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(handler);
            map.Add(Interactive);

            MainHandler = new MainHandler(client);
            client.GuildAvailable += MainHandler.GuildAvailableEvent;
            client.LeftGuild += MainHandler.LeftGuildEvent;

            await MainHandler.InitializeEarlyAsync(map);

            await client.LoginAsync(TokenType.Bot, MainHandler.ConfigHandler.GetBotToken());
            await client.StartAsync();
            await Task.Delay(-1);
        }
    }
}