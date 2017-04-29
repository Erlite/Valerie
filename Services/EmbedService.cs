using Discord;
using Discord.WebSocket;
using Rick.Handlers;
using System;

namespace Rick.Services
{
    public class EmbedService
    {
        public static EmbedBuilder AdminEmbed(IUser user, int Color1, int Color2, int Color3, string Mod, string ModUrl, string Reason, string ThumbUrl)
        {
            var gld = user as SocketGuildUser;
            var gldConfig = GuildHandler.GuildConfigs[gld.Guild.Id];

            return new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = $"{Mod}";
                    x.IconUrl = ModUrl;
                })
                .WithDescription($"**Username: **{user.Username}#{user.Discriminator}\n**Responsilble Mod: **{Mod}\n**Reason: **{Reason}\n**Case Number:** {gldConfig.CaseNumber}")
                .WithImageUrl(ThumbUrl)
                .WithColor(new Color(Color1, Color2, Color3))
                .WithFooter(x =>
                {
                    x.Text = $"Incident Date: {DateTime.Now.ToString()}";
                });
        }

        public static EmbedBuilder BasicEmbed(string AuthorName, string AuthorIcon, string Description, int Color1, int Color2, int Color3)
        {
            return new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = AuthorName;
                    x.IconUrl = AuthorIcon;
                })
                .WithDescription(Description)
                .WithColor(new Color(Color1, Color2, Color3));
        }
    }
}
