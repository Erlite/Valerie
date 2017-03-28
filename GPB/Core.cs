﻿using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.IO;
using GPB.Services;
using GPB.Handlers;
using Discord.Addons.InteractiveCommands;
using Newtonsoft.Json;
using System.Text;
using GPB.Services.TagServices;
using Discord.Net.WebSockets;
using System;
using Discord.Net.Providers.WS4Net;

namespace GPB
{
    class Core
    {
        static void Main(string[] args) => new Core().StartAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private ConfigHandler config;
        private LogService log;
        private CommandHandler handler;
        private DependencyMap map;
        private TagService tag;
        string dict = Path.Combine(Directory.GetCurrentDirectory(), "Guilds");
        private const string EmptyJson = "{\r\n}";


        public async Task StartAsync()
        {
            ConsoleService.Log(LogSeverity.Info, "Connection", "Type 7 for Windows 7 & below else Type 8: ");
            var get = Convert.ToInt32(Console.ReadLine());
            if (get == 7)
            {
                client = new DiscordSocketClient(new DiscordSocketConfig()
                {
                    WebSocketProvider = WS4NetProvider.Instance,
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 10000,
                    AlwaysDownloadUsers = true
                });
            }
            else if (get == 8)
            {
                client = new DiscordSocketClient(new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 10000,
                    AlwaysDownloadUsers = true
                });
            }
            else
                throw new FormatException("Not a valid value!");


            ConsoleService.TitleCard("Oreos", DiscordConfig.Version);
            client.Log += (l) => Task.Run(() => ConsoleService.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            #region Config
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
            {
                ConsoleService.Log(LogSeverity.Info, "Config", "Config has been Loaded!");
                config = await ConfigHandler.UseCurrentAsync();
            }
            else
            {
                ConsoleService.Log(LogSeverity.Warning, "Config", "Config Directory created! Time to setup config!");
                config = await ConfigHandler.CreateNewAsync();
            }

            #endregion

            log = new LogService(client, config);
            await log.LoadConfigurationAsync();

            client.GuildAvailable += GuildAvailable;
            map = new DependencyMap();
            map.Add(client);
            map.Add(config);
            map.Add(log);
            map.Add(new InteractiveService(client));
            map.Add(tag);

            handler = new CommandHandler(map);
            await handler.InstallAsync();

            await client.LoginAsync(TokenType.Bot, config.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task GuildAvailable(SocketGuild gld)
        {
            LogHandler result;
            result = new LogHandler();
            if (!Directory.Exists(dict))
                Directory.CreateDirectory(dict);
            foreach (var guild in client.Guilds)
            {
                string guildDirectory = Path.Combine(dict, guild.Id.ToString());
                if (!Directory.Exists(guildDirectory))
                {
                    Directory.CreateDirectory(guildDirectory);
                }

                using (var configStream = File.Create(Path.Combine(guildDirectory, "logs.json")))
                {
                    using (var configWriter = new StreamWriter(configStream))
                    {
                        var save = JsonConvert.SerializeObject(result, Formatting.Indented);
                        await configWriter.WriteAsync(save);
                    }
                }
                using (var file = File.Create(Path.Combine(guildDirectory, "response.json")))
                {
                    var content = Encoding.UTF8.GetBytes(EmptyJson);
                    file.Write(content, 0, content.Length);
                    file.Flush();
                }
            }
        }
    }
}