using System.Threading.Tasks;
using Discord;

namespace Nygma.Utils
{
    public static class EmbedAsync
    {
        public static EmbedBuilder Error(string Title, string Reason, string source, string target, string StackTrace)
        {
            var embed = new EmbedBuilder();

            embed.Title = $"Error Type => {Title}";
            embed.Description = $"**Reason: **{Reason}\n**Source: **{source}\n**Target: **{target}\n**Stacktrace: **{StackTrace}";
            embed.Color = Misc.RandColor();
            return embed;
        }
        public static EmbedBuilder Reminder(string title, IUser user, string reason, string DT, string url)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription($"**Username: **{user}\n**Reason: **{reason}\n**Date Time: **{DT}")
                .WithThumbnailUrl(url)
                .WithColor(Misc.RandColor())
                .WithCurrentTimestamp();
            return embed;
        }

        public static async Task<IUserMessage> SendEmbedAsync(this IMessageChannel Channel, EmbedBuilder Embed)
            => await Channel.SendMessageAsync("", false, Embed);
    }
}