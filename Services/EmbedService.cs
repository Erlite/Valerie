using Discord;
using Rick.Models;

namespace Rick.Services
{
    public class EmbedService
    {
        public static  EmbedBuilder Embed(EmbedModel Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case EmbedModel.Blurple:
                    embed.Color = new Color(0x7289DA);
                    break;

                case EmbedModel.Cyan:
                    embed.Color = new Color(0x8cfff6);
                    break;

                case EmbedModel.Dark:
                    embed.Color = new Color(0x2C2F33);
                    break;

                case EmbedModel.Gold:
                    embed.Color = new Color(0xfdff28);
                    break;

                case EmbedModel.Green:
                    embed.Color = new Color(0x93ff89);
                    break;

                case EmbedModel.Maroon:
                    embed.Color = new Color(0x800000);
                    break;

                case EmbedModel.NotQuiteBlack:
                    embed.Color = new Color(0x23272A);
                    break;

                case EmbedModel.Orange:
                    embed.Color = new Color(0xffba63);
                    break;

                case EmbedModel.Pastle:
                    embed.Color = new Color(0xa91edf);
                    break;

                case EmbedModel.Red:
                    embed.Color = new Color(0xff0000);
                    break;

                case EmbedModel.Teal:
                    embed.Color = new Color(0x008080);
                    break;

                case EmbedModel.White:
                    embed.Color = new Color(0xFFFFFF);
                    break;

                case EmbedModel.Yellow:
                    embed.Color = new Color(0xfff863);
                    break;
            }
            return embed;
        }

        public static  EmbedBuilder Embed(EmbedModel Color, string AuthorName = null, string AuthorPic = null,  string Title = null, string Description = null, string FooterText = null, string FooterIcon = null, string ImageUrl = null, string ThumbUrl = null)
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
