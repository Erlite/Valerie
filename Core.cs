﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.Addons.InteractiveCommands;
using Discord.Net.Providers.WS4Net;
using System.IO;
using Rick.Models;

namespace Rick
{
    public class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();
        private DiscordSocketClient client;
        private BotConfigHandler config;
        private CommandHandler handler;

        public async Task StartAsync()
        {
            #region Config Check
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                ConsoleService.Log(LogSeverity.Info, "Config", "Config has been Loaded!");
                config = await BotConfigHandler.UseCurrentAsync();
            }
            else
            {
                ConsoleService.Log(LogSeverity.Warning, "Config", "Config Directory created! Time to setup config!");
                config = await BotConfigHandler.CreateNewAsync();
            }
            #endregion

            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                WebSocketProvider = WS4NetProvider.Instance,
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 9000,
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.AlwaysRetry
            });

            client.Log += (log) => Task.Run(() => ConsoleService.Log(log.Severity, log.Source, log.Exception?.ToString() ?? log.Message));

            var map = new DependencyMap();
            map.Add(client);
            map.Add(config);
            map.Add(new InteractiveService(client));
            map.Add(new GuildHandler());

            client.GuildAvailable += CreateGuildConfigAsync;
            client.LeftGuild += RemoveGuildConfigAsync;
           
            handler = new CommandHandler(map);
            await handler.InstallAsync();

            await client.LoginAsync(TokenType.Bot, config.BotToken);
            await client.StartAsync();
            await Task.Delay(-1);
        }

        public async Task CreateGuildConfigAsync(SocketGuild Guild)
        {
            var gldConfig = new GuildModel();
            GuildModel.GuildConfig.Add(Guild.Id, gldConfig);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json");
            await GuildModel.SaveAsync(path, GuildModel.GuildConfig);
        }

        public async Task RemoveGuildConfigAsync(SocketGuild Guild)
        {
            ConsoleService.Log(LogSeverity.Warning, Guild.Name, "Config Deleted!");
            if (GuildModel.GuildConfig.ContainsKey(Guild.Id))
            {
                GuildModel.GuildConfig.Remove(Guild.Id);
            }
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Config", "GuildConfig.json");
            await GuildModel.SaveAsync(path, GuildModel.GuildConfig);
        }

    }
}