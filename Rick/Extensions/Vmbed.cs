using System;
using Discord;

namespace Rick.Extensions
{
    public class Vmbed
    {
        static readonly Random Random = new Random();

        public static EmbedBuilder Embed(VmbedColors Color,
            string AuthorIcon = null, string AuthorName = null, string AuthorUrl = null,
            string Description = null, string FooterIcon = null, string FooterText = null,
            string ImageUrl = null, string ThumbUrl = null, string Title = null, string Url = null)
        {
            return Embed(Color)
                .WithAuthor(new EmbedAuthorBuilder
                {
                    IconUrl = AuthorIcon,
                    Name = AuthorName,
                    Url = AuthorUrl
                })
                .WithDescription(Description)
                .WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = FooterIcon,
                    Text = FooterText
                })
                .WithImageUrl(ImageUrl)
                .WithThumbnailUrl(ThumbUrl)
                .WithTitle(Title)
                .WithUrl(Url);
        }

        static EmbedBuilder Embed(VmbedColors Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case VmbedColors.Black: embed.Color = new Color(0x000000); break;
                case VmbedColors.Cyan: embed.Color = new Color(0x00FFFF); break;
                case VmbedColors.Gold: embed.Color = new Color(0xFFDF00); break;
                case VmbedColors.Green: embed.Color = new Color(0x00FA9A); break;
                case VmbedColors.Pastel: embed.Color = new Color(0xFF86E3); break;
                case VmbedColors.Red: embed.Color = new Color(0xDC143C); break;
                case VmbedColors.Snow: embed.Color = new Color(0xFFFAFA); break;
            }
            return embed;
        }
    }

    public enum VmbedColors
    {
        Red,
        Gold,
        Green,
        Cyan,
        Snow,
        Pastel,
        Black
    }
}
