using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Nygma.Handlers;
using Discord.Addons.Paginator;
using Nygma.Utils;
using System.IO;
using System;

namespace Nygma
{
    public class Core
    {
        private DiscordSocketClient _client;
        private CommandHandler _handler;
        private DependencyMap _map;
        private ConfigHandler config;

        public Core(ConfigHandler con)
        {
            config = con;
        }
        public Core()
        {
        }

        public async Task RunAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });
            _client.Log += (l)
                => Task.Run(()
                => IConsole.Log(l.Severity, l.Source, l.Exception?.ToString() ?? l.Message));

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config")))
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Config"));

            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Config", "Config.json")))
                config = await ConfigHandler.UseCurrentAsync();
            else
                config = await ConfigHandler.CreateNewAsync();

            //Figure it out later
            //_client.UserJoined += EventsHandler.UserJoinedAsync;
            //_client.UserLeft += EventsHandler.UserLeftAsync;
            _client.UserJoined += async (user) =>
            {
                if (_client.GetGuild(config.LogGuild).Id == config.LogGuild)
                {
                    if (config.Welcome == true)
                    {
                        var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
                        var embed = new EmbedBuilder();
                        embed.Title = "User Joined Event";
                        embed.Description = $"**Username: **{user.Mention}\n**Guild Name: **{user.Guild.Name}\n{config.WelcomeMsg}";
                        embed.Color = Misc.RandColor();
                        await ch.SendMessageAsync("", embed: embed);
                    }
                }
                else
                {
                    var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
                    var embed = new EmbedBuilder();
                    embed.Title = "User Joined Event";
                    embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}";
                    embed.Color = Misc.RandColor();
                    await ch.SendMessageAsync("", embed: embed);
                }
            };

            _client.UserLeft += async (user) =>
            {
                var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
                var embed = new EmbedBuilder();
                embed.Title = "User Left Event";
                embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}";
                embed.Color = Misc.RandColor();
                await ch.SendMessageAsync("", embed: embed);
            };

            _client.MessageReceived += async (message) =>
            {
                IConsole.Log(LogSeverity.Info, "MESSAGE", "[" + message.Timestamp.UtcDateTime.ToString("dd/MM/yyyy HH:mm:ss") + "]" + message.Author.Username + ": " + message.Content);
                var chx = message.Channel as SocketGuildChannel as ITextChannel;
                if (message.Author.Id != _client.CurrentUser.Id)
                {
                    var ch = _client.GetGuild(config.LogGuild).GetChannel(config.LogChannel) as ITextChannel;
                    var embed = new EmbedBuilder();
                    embed.Title = "Message Received Event";
                    embed.Description = $"**Guild Name: **{chx.Guild.Name}\n**Channel Name: **{message.Channel.Name}\n**Username: **{message.Author.Username} || **IsBot?** {message.Author.IsBot}\n**Message ID: **{message.Id}\n**Message Content: **{message.Content}\n**Attachments **{message.Attachments.Count}";
                    embed.Color = Misc.RandColor();
                    await ch.SendMessageAsync("", embed: embed);
                }
                else
                    IConsole.Log(LogSeverity.Info, "Application Message", "Ignoring this message");
            };

            _map = new DependencyMap();
            _map.Add(_client);
            _map.Add(config);
            ConfigureServices(_map);

            _handler = new CommandHandler();
            await _handler.InstallAsync(_map);

            IConsole.Log(LogSeverity.Info, "Login", "Selfbot => Type S | Normal Bot => Type B:  ");

            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                await _client.LoginAsync(TokenType.User, config.UserToken);
                IConsole.TitleCard($"v{DiscordConfig.Version} || SelfBot");
            }
            else if (Console.ReadKey().Key == ConsoleKey.B)
            {
                await _client.LoginAsync(TokenType.Bot, config.BotToken);
                IConsole.TitleCard($"{config.BotName} || v{DiscordConfig.Version} || Bot");
            }
            else
            {
                IConsole.Log(LogSeverity.Error, "ERROR", "Invalid Key! Closing... ");
                return;
            }



            await _client.ConnectAsync();

            await _client.SetGameAsync(config.Game);

            await Task.Delay(-1);
        }

        public void ConfigureServices(IDependencyMap map)
        {
            _client.UsePaginator(map);
        }
    }
}