﻿using Discord;
using System.Collections.Generic;
using System.Linq;
using System;

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

        public static bool IsNSFW(IChannel Channel) => Channel.Name.Contains("nsfw");

        public static bool IsMessageUrl(string Message)
        {
            List<string> ImageMatch = new List<string>
            {
                ".png",
                ".gif",
                ".jpg",
                ".webp"
            };
            return ImageMatch.Any(x => Message.Contains(x)) && Uri.IsWellFormedUriString(Message, UriKind.RelativeOrAbsolute);
        }
    }
}
