using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Nygma.Utils;

namespace Nygma.Modules
{
    public class InfoModule : ModuleBase
    {
        [Command("Info")]
        public async Task InfoAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var Gld = Context.Guild as SocketGuild;

            var Servers = Gld.Discord.Guilds.Count();
            var Users = client.Guilds.Sum(g => g.Users.Count);
            var Lib = DiscordConfig.Version;
            var Heap = $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)}MB";
            var Lat = $"{client.Latency} MS";
            var Run = $"{RuntimeInformation.FrameworkDescription}{ RuntimeInformation.OSArchitecture}";
            var Up = $"{(DateTime.Now - Process.GetCurrentProcess().StartTime)}";
            var Ap = DiscordConfig.APIVersion.ToString();
            var Con = client.ConnectionState;

            await Context.Message.DeleteAsync();

            var embed = new EmbedBuilder()
                .WithTitle(client.CurrentUser.Username)
                .WithThumbnailUrl(client.CurrentUser.AvatarUrl)
                .WithColor(Misc.RandColor())
                .WithDescription($"**Servers: **{Servers}\n**Users: **{Users}\n**Library: **{Lib}\n**API Version: **{Ap}\n" +
                    $"**Latency: **{Lat}\n**Heap Size: **{Heap}\n**Runtime: **{Run}\n**Uptime: **{Up}\n**Connection State: **{Con}");

            await ReplyAsync("", false, embed);
        }
    }
}