using Discord;

namespace Valerie.Extensions
{
    public class ValerieEmbed
    {
        public static EmbedBuilder Embed(EmbedColor Color,
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

        static EmbedBuilder Embed(EmbedColor Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case EmbedColor.Black: embed.Color = new Color(0x000000); break;
                case EmbedColor.Cyan: embed.Color = new Color(0x00FFFF); break;
                case EmbedColor.Gold: embed.Color = new Color(0xFFDF00); break;
                case EmbedColor.Green: embed.Color = new Color(0x00FA9A); break;
                case EmbedColor.Pastel: embed.Color = new Color(0xFF86E3); break;
                case EmbedColor.Red: embed.Color = new Color(0xDC143C); break;
                case EmbedColor.Snow: embed.Color = new Color(0xFFFAFA); break;
            }
            return embed;
        }
    }

    public enum EmbedColor
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
