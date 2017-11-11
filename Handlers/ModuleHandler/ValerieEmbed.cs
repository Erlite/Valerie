using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace Valerie.Handlers.ModuleHandler
{
    public class ValerieEmbed
    {
        public static EmbedBuilder Embed(EmbedColor Color, string AuthorIcon = null, string AuthorName = null, string AuthorUrl = null,
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
                case EmbedColor.Yellow: embed.Color = new Color(247, 255, 25); break;
                case EmbedColor.Pastel: embed.Color = new Color(247, 131, 227); break;
                case EmbedColor.Red: embed.Color = new Color(232, 27, 78); break;
            }
            return embed;
        }
    }

    public enum EmbedColor
    {
        Red,
        Pastel,
        Yellow,
    }
}