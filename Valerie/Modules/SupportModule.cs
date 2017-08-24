using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.Handlers.ConfigHandler;

namespace Valerie.Modules
{
    public class SupportModule : InteractiveBase
    {
        [Command("Report"), Summary("Reports an issue to Bot owner / Used to give feedback.")]
        public async Task ReportAsync()
        {
            EmbedBuilder Embed = null;
            await ReplyAndDeleteAsync("Are you submitting a feedback or report? (F/R)", timeout: TimeSpan.FromSeconds(10));
            var GetType = await NextMessageAsync();
            string ReportType = null;
            if (GetType.Content.ToLower() == "f")
            {
                ReportType = "Feedback";
                Embed = Vmbed.Embed(VmbedColors.Green, Title: $"New {ReportType}");
            }
            else if (GetType.Content.ToLower() == "r")
            {
                ReportType = "Report";
                Embed = Vmbed.Embed(VmbedColors.Red, Title: $"New {ReportType}");
            }
            else
            {
                await ReplyAndDeleteAsync("Invalid report type.", timeout: TimeSpan.FromSeconds(5));
                return;
            }
            await ReplyAndDeleteAsync($"Please enter your {ReportType}:", timeout: TimeSpan.FromSeconds(30));
            var GetReport = await NextMessageAsync();
            if (GetReport != null)
                Embed.Description = GetReport.Content;
            else
            {
                await ReplyAndDeleteAsync("No response was provided.", timeout: TimeSpan.FromSeconds(5));
                return;
            }
            Embed.AddInlineField("Server", $"{Context.Guild.Name}\n{Context.Guild.Id}");
            Embed.AddInlineField("User", $"{Context.User}\n{Context.User.Id}");
            Embed.AddInlineField("Additional Information", $"User Count: {Context.Guild.Users.Count}\nChannel Count: {Context.Guild.Channels.Count}");
            var Channel = Context.Client.GetChannel(Convert.ToUInt64(BotDB.Config.ReportChannel)) as ITextChannel;
            await Channel.SendMessageAsync("", embed: Embed);
            await ReplyAndDeleteAsync($"Your {ReportType} has been submitted.", timeout: TimeSpan.FromSeconds(5));
        }
    }
}