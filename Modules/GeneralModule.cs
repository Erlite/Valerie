using Discord;
using System.IO;
using Valerie.Addons;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;

namespace Valerie.Modules
{
    [Name("General Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : Base
    {
        [Command("Ping"), Summary("Replies back with a pong?")]
        public Task PingAsync() => ReplyAsync(string.Empty, BuildEmbed(Paint.Lime)
            .AddField("Gateway Latency", $"{(Context.Client as DiscordSocketClient).Latency}ms").Build());
    }
}