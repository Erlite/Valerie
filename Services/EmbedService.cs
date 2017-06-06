using Discord;
using Rick.Models;

namespace Rick.Services
{
    public class EmbedService
    {
        public static  EmbedBuilder Embed(EmbedColor Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case EmbedColor.Blurple:
                    embed.Color = new Color(0x7289DA);
                    break;

                case EmbedColor.Cyan:
                    embed.Color = new Color(0x8cfff6);
                    break;

                case EmbedColor.Dark:
                    embed.Color = new Color(0x2C2F33);
                    break;

                case EmbedColor.Gold:
                    embed.Color = new Color(0xfdff28);
                    break;

                case EmbedColor.Green:
                    embed.Color = new Color(0x93ff89);
                    break;

                case EmbedColor.Maroon:
                    embed.Color = new Color(0x800000);
                    break;

                case EmbedColor.NotQuiteBlack:
                    embed.Color = new Color(0x23272A);
                    break;

                case EmbedColor.Orange:
                    embed.Color = new Color(0xffba63);
                    break;

                case EmbedColor.Pastle:
                    embed.Color = new Color(0xa91edf);
                    break;

                case EmbedColor.Red:
                    embed.Color = new Color(0xff0000);
                    break;

                case EmbedColor.Teal:
                    embed.Color = new Color(0x008080);
                    break;

                case EmbedColor.White:
                    embed.Color = new Color(0xFFFFFF);
                    break;

                case EmbedColor.Yellow:
                    embed.Color = new Color(0xfff863);
                    break;
            }
            return embed;
        }

        public static  EmbedBuilder Embed(EmbedColor Color, string AuthorName = null, string AuthorPic = null,  string Title = null, string Description = null, string FooterText = null, string FooterIcon = null, string ImageUrl = null, string ThumbUrl = null)
        {
            return Embed(Color)
                .WithAuthor(x =>
                {
                    x.Name = AuthorName;
                    x.IconUrl = AuthorPic;
                })
                .WithTitle(Title)
                .WithDescription(Description)
                .WithImageUrl(ImageUrl)
                .WithThumbnailUrl(ThumbUrl)
                .WithFooter(x =>
                {
                    x.Text = FooterText;
                    x.IconUrl = FooterIcon;
                });
        }
    }
}
