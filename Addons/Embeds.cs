using Discord;

namespace Valerie.Addons
{
    public class Embeds
    {
        public static EmbedBuilder GetEmbed(Paint Paint)
        {
            var Embed = new EmbedBuilder();
            switch (Paint)
            {
                case Paint.Aqua: Embed.Color = new Color(39, 255, 255); break;
                case Paint.Rose: Embed.Color = new Color(255, 153, 153); break;
                case Paint.Lime: Embed.Color = new Color(217, 255, 207); break;
                case Paint.Crimson: Embed.Color = new Color(220, 20, 60); break;
                case Paint.Yellow: Embed.Color = new Color(255, 250, 131); break;
                case Paint.Magenta: Embed.Color = new Color(245, 207, 255); break;
                case Paint.PaleYellow: Embed.Color = new Color(255, 245, 207); break;
            }
            return Embed;
        }

        public enum Paint
        {
            Lime,
            Aqua,
            Rose,
            Yellow,
            Crimson,
            Magenta,
            PaleYellow
        }
    }
}