using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using GPB.Services;

namespace GPB.Modules
{
    public class GeneralModule : ModuleBase
    {
        [Command("Test")]
        public async Task TestAsync()
        {
            var embed = new EmbedBuilder()
        .WithTitle("===== Kicked User =====")
        .WithDescription($"**kjhajkshdjkahsjd**")
        .WithAuthor(x =>
        {
            x.Name = Context.User.Username;
            x.IconUrl = Context.User.GetAvatarUrl();
        })
        .WithColor(new Color(232, 226, 53))
        .WithFooter(x =>
        {
            x.Text = $"Kicked by {Context.User}";
            x.IconUrl = Context.User.GetAvatarUrl();
        });
            await ReplyAsync("", embed: embed);
        }
    }
}