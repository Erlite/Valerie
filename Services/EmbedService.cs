using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using System;
using Rick.Classes;

namespace Rick.Services
{
    public class EmbedService
    {
        public static  EmbedBuilder Embed(EmbedColors Color)
        {
            var embed = new EmbedBuilder();
            switch (Color)
            {
                case EmbedColors.Blurple:
                    embed.Color = new Color(0x7289DA);
                    break;

                case EmbedColors.Cyan:
                    embed.Color = new Color(0x8cfff6);
                    break;

                case EmbedColors.Dark:
                    embed.Color = new Color(0x2C2F33);
                    break;

                case EmbedColors.Gold:
                    embed.Color = new Color(0xfdff28);
                    break;

                case EmbedColors.Green:
                    embed.Color = new Color(0x93ff89);
                    break;

                case EmbedColors.Maroon:
                    embed.Color = new Color(0x800000);
                    break;

                case EmbedColors.NotQuiteBlack:
                    embed.Color = new Color(0x23272A);
                    break;

                case EmbedColors.Orange:
                    embed.Color = new Color(0xffba63);
                    break;

                case EmbedColors.Pastle:
                    embed.Color = new Color(0xa91edf);
                    break;

                case EmbedColors.Red:
                    embed.Color = new Color(0xff0000);
                    break;

                case EmbedColors.Teal:
                    embed.Color = new Color(0x008080);
                    break;

                case EmbedColors.White:
                    embed.Color = new Color(0xFFFFFF);
                    break;

                case EmbedColors.Yellow:
                    embed.Color = new Color(0xfff863);
                    break;
            }
            return embed;
        }

        public static  EmbedBuilder Embed(EmbedColors Color, string AuthorName = null, string AuthorPic = null, string Title = null, string Description = null, string FooterText = null, string FooterIcon = null, string ImageUrl = null)
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
                .WithFooter(x =>
                {
                    x.Text = FooterText;
                    x.IconUrl = FooterIcon;
                });
        }
    }
}
