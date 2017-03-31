using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.IO;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    [Group("Set"), RequireOwner]
    public class BotModule : ModuleBase
    {
        [Command("Username"), RequireContext(ContextType.Guild)]
        public async Task UsernameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = value).ConfigureAwait(false);
            await ReplyAsync(":eyes: Done :eyes:").ConfigureAwait(false);
        }

        [Command("Nickname"), RequireContext(ContextType.Guild)]
        public async Task NicknameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await (await Context.Guild.GetCurrentUserAsync().ConfigureAwait(false)).ModifyAsync(x => x.Nickname = value).ConfigureAwait(false);
            await ReplyAsync(":eyes: Done :eyes:").ConfigureAwait(false);
        }

        [Command("Avatar"), RequireContext(ContextType.Guild)]
        public async Task AvatarAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var url = value;
            if (value == "reset")
            {
                var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
                url = app.IconUrl;
            }
            var q = new Uri(url);
            using (var client = new HttpClient())
            {
                await client.DownloadAsync(q, q.LocalPath.Replace("/", "")).ConfigureAwait(false);
                using (var imagestream = new FileStream(q.LocalPath.Replace("/", ""), FileMode.Open))
                {
                    await
                        Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imagestream)).ConfigureAwait(false);
                }
                File.Delete(q.LocalPath.Replace("/", ""));
            }
            await ReplyAsync(":eyes: Done :eyes:").ConfigureAwait(false);
        }

        [Command("Game"), RequireContext(ContextType.Guild)]
        public async Task GameAsync([Remainder] string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value cannot be empty");
            var client = Context.Client as DiscordSocketClient;
            await client.SetGameAsync(value).ConfigureAwait(false);
            await ReplyAsync(":eyes: Done :eyes:").ConfigureAwait(false);
        }

        [Command("Status"), RequireContext(ContextType.Guild)]
        public async Task StatusAsync([Remainder] string value)
        {
            var newStatus = Enum.Parse(typeof(UserStatus), value);
            await (Context.Client as DiscordSocketClient).SetStatusAsync((UserStatus)newStatus).ConfigureAwait(false);
            await ReplyAsync(":eyes: Done :eyes:").ConfigureAwait(false);
        }
    }
}