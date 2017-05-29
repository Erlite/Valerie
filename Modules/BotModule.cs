using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.IO;
using Rick.Services;
using Rick.Handlers;

namespace Rick.Modules
{
    [Group("Bot"), RequireOwner, RequireContext(ContextType.Guild)]
    public class BotModule : ModuleBase
    {
        private EventService Events;
        public BotModule(EventService events)
        {
            Events = events;
        }

        [Command("Username"), Summary("Username OwO"), Remarks("Changes Bot's username")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var botConfig = BotHandler.BotConfig;
            botConfig.BotName = value;
            var client = Context.Client as DiscordSocketClient;
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value);
            await ReplyAsync(":eyes: Done :eyes:");
            await BotHandler.SaveAsync(botConfig);
        }

        [Command("Nickname"), Summary("Nickname XD"), Remarks("Changes Bot's nickname")]
        public async Task NicknameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = value);
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Avatar"), Summary("Avatar {Some-Link.Com}"), Remarks("Changes Bot's avatar")]
        public async Task AvatarAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var url = value;
            if (value == "reset")
            {
                var app = await Context.Client.GetApplicationInfoAsync();
                url = app.IconUrl;
            }
            var q = new Uri(url);
            using (var client = new HttpClient())
            {
                await client.DownloadAsync(q, q.LocalPath.Replace("/", ""));
                using (var imagestream = new FileStream(q.LocalPath.Replace("/", ""), FileMode.Open))
                {
                    await
                        Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imagestream));
                }
                File.Delete(q.LocalPath.Replace("/", ""));
            }
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Game"), Summary("Game DarkSouls 3"), Remarks("Changes Bot's game")]
        public async Task GameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            var botConfig = BotHandler.BotConfig;
            botConfig.BotGame = value;
            await client.SetGameAsync(value);
            await ReplyAsync(":eyes: Done :eyes:");
            await BotHandler.SaveAsync(botConfig);
        }

        [Command("Status"), Summary("Status 3"), Remarks("Changes Bot's status such as setting status to DND")]
        public async Task StatusAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var newStatus = Enum.Parse(typeof(UserStatus), value);
            await (Context.Client as DiscordSocketClient).SetStatusAsync((UserStatus)newStatus);
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Latency"), Summary("Normal Command"), Remarks("Enables/Disables monitoring your ping")]
        public async Task LatencyAsync()
        {
            var Config = BotHandler.BotConfig;
            if (!Config.ClientLatency)
            {
                Config.ClientLatency = true;
                Events.EnableLatencyMonitor();
                await ReplyAsync(":gear: Will AutoUpdate my status based on Ping!");
            }
            else
            {
                Config.ClientLatency = false;
                Events.DisableLatencyMonitor();
                await ReplyAsync(":skull_crossbones: Latency monitor disabled");
            }
            await BotHandler.SaveAsync(Config);
        }

        [Command("Prefix"), Summary("Prefix ?"), Remarks("Sets Bot's default prefix")]
        public async Task DefaultPrefixAsync([Remainder]string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new NullReferenceException("Prefix can't be left empty!");
            var botConfig = BotHandler.BotConfig;
            botConfig.DefaultPrefix = prefix;
            await ReplyAsync($":gear: Bot's default prefix has been set to: **{prefix}**");
            await BotHandler.SaveAsync(botConfig);
        }

        [Command("Debug"), Summary("Normal Command"), Remarks("Enables/Disables debug mode")]
        public async Task DebugAsync()
        {
            var Config = BotHandler.BotConfig;
            if (!Config.DebugMode)
            {
                Config.DebugMode = true;
                await ReplyAsync(":gear: Debug mode has been enabled!");
            }
            else
            {
                Config.DebugMode = false;
                await ReplyAsync(":skull_crossbones: Debug mode has been disbaled!");
            }
            await BotHandler.SaveAsync(Config);
        }

        [Command("Mention"), Summary("Normal Command"), Remarks("Enables/Disables mention prefix")]
        public async Task MentionAsync()
        {
            var Config = BotHandler.BotConfig;
            if (!Config.MentionDefaultPrefix)
            {
                await ReplyAsync(":gear: Mention Prefix has been enabled!");
            }
            else
            {
                await ReplyAsync(":skull_crossbones: Mention Prefix has been disbaled!");
            }
            await BotHandler.SaveAsync(Config);
        }

    }
}