using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.IO;
using Rick.Handlers;
using Rick.Extensions;

namespace Rick.Modules
{
    [Group("Bot"), RequireOwner]
    public class BotModule : ModuleBase
    {
        [Command("Username"), Summary("Changes Bot's username"), Remarks("Username OwO")]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var botConfig = ConfigHandler.IConfig;
            var client = Context.Client as DiscordSocketClient;
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value);
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Nickname"), Summary("Changes Bot's nickname"), Remarks("Nickname XD")]
        public async Task NicknameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = value);
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Avatar"), Summary("Changes Bot's avatar"), Remarks("Avatar {Some-Link.Com}")]
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

        [Command("Game"), Summary("Changes Bot's game"), Remarks("Game DarkSouls 3")]
        public async Task GameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            var botConfig = ConfigHandler.IConfig;
            botConfig.Games.Add(value);
            await client.SetGameAsync(value);
            await ReplyAsync(":eyes: Done :eyes:");
            await ConfigHandler.SaveAsync();
        }

        [Command("Status"), Summary("Changes Bot's status such as setting status to DND"), Remarks("Status 3")]
        public async Task StatusAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new NullReferenceException("Value cannot be empty");
            var newStatus = Enum.Parse(typeof(UserStatus), value);
            await (Context.Client as DiscordSocketClient).SetStatusAsync((UserStatus)newStatus);
            await ReplyAsync(":eyes: Done :eyes:");
        }

        [Command("Prefix"), Summary("Sets Bot's default prefix"), Remarks("Prefix ?")]
        public async Task DefaultPrefixAsync([Remainder]string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                throw new NullReferenceException("Prefix can't be left empty!");
            var botConfig = ConfigHandler.IConfig;
            botConfig.Prefix = prefix;
            await ReplyAsync($":gear: Bot's default prefix has been set to: **{prefix}**");
            await ConfigHandler.SaveAsync();
        }

        [Command("Debug"), Summary("Enables/Disables debug mode")]
        public async Task DebugAsync()
        {
            var Config = ConfigHandler.IConfig;
            if (!Config.IsDebugEnabled)
            {
                Config.IsDebugEnabled = true;
                await ReplyAsync(":gear: Debug mode has been enabled!");
            }
            else
            {
                Config.IsDebugEnabled = false;
                await ReplyAsync(":skull_crossbones: Debug mode has been disbaled!");
            }
            await ConfigHandler.SaveAsync();
        }

        [Command("Mention"), Summary("Enables/Disables mention prefix")]
        public async Task MentionAsync()
        {
            var Config = ConfigHandler.IConfig;
            if (!Config.IsMentionEnabled)
            {
                await ReplyAsync(":gear: Mention Prefix has been enabled!");
            }
            else
            {
                await ReplyAsync(":skull_crossbones: Mention Prefix has been disbaled!");
            }
            await ConfigHandler.SaveAsync();
        }
    }
}