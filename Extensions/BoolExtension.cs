using System.Linq;
using System.Collections.Generic;
using Discord;

namespace Valerie.Extensions
{
    public class BoolExtension
    {
        public static bool Advertisement(string Message)
        {
            List<string> URLS = new List<string>
            {
                "https://discordapp.com/invite/",
                "https://discord.com/invite/",
                "https://discord.me/invite/",
                "https://discordapp.gg/invite/",
                "https://discord.gg/invite/",
                "https://discord.me/",
                "https://discord.gg/",
                "https://www.discordapp.com/invite/",
                "https://www.discord.com/invite/",
                "https://www.discord.me/invite/",
                "https://www.discordapp.gg/invite/",
                "https://www.discord.gg/invite/",
                "https://www.discord.me/",
                "discordapp.com/invite/",
                "discord.com/invite/",
                "discord.me/invite/",
                "discordapp.gg/invite/",
                "discord.gg/invite/",
                "discord.me/",
                "discord.gg/"
            };
            return (URLS.Any(x => Message.Contains(x) | Message.StartsWith(x)));
        }

        public static bool HasLeveledUp(int Previous, int New) => New > Previous;

        public static bool IsNSFW(ITextChannel Channel) => Channel.Name.Contains("nsfw") || Channel.IsNsfw;
    }
}
