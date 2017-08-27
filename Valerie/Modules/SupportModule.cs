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
            await ReplyAndDeleteAsync($"Please enter your {ReportType}:", timeout: TimeSpan.FromSeconds(60));
            var GetReport = await NextMessageAsync();
            if (GetReport.Content.Length < 30)
            {
                await ReplyAndDeleteAsync("The report must be longer than 30 characters.");
                return;
            }

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

        [Command("Guide"), Summary("Shows guide to various things related to the bot.")]
        public async Task GuideAsync()
        {
            string GuideMessage = null;
            string Title = null;
            await ReplyAndDeleteAsync(
                $"Hello {Context.User.Mention}! Please pick a topic (Enter Number):\n" +
                $":one: NSFW Commands\n" +
                $":two: Toggle Join/Eridium(Chat XP)/Starboard Etc\n" +
                $":three: Help With Tags!\n" +
                $":four: I think I found a bug! OR I need to give feedback!", timeout: TimeSpan.FromSeconds(20));
            var Choice = (await NextMessageAsync()).Content;
            if (!int.TryParse(Choice, out int Num))
            {
                await ReplyAndDeleteAsync("Your input wasn't valid! Exiting guide ...");
                return;
            }
            if (Num > 4 || Num < 1)
            {
                await ReplyAndDeleteAsync("Not a valid choice. Exiting guide ...");
                return;
            }
            switch (Num)
            {
                case 1:
                    Title = "GUIDE | NSFW Commands";
                    GuideMessage = $"NSFW Commands can only be ran in a channel named NSFW or your channel name contains the word \"NSFW\" in it. " +
                        $"This is to make sure that users can't user the command outside of NSFW channels. For more info: https://github.com/Yucked/Valerie/wiki/Command-Help";

                    break;
                case 2:
                    Title = $"GUIDE | Toggle Join/Eridium(Chat XP)/Starboard Etc";
                    GuideMessage = $"Here are the following things you can toggle: CB, Join, Eridium, Leave, Starboard, Mod, NoAds.\n\n" +
                        $"**CB:** Chatterbot\n**Join/Leave:** When a user joins/leaves your server.\n**Eridium:** Gives Eridium based on your message. " +
                        $"The more you talk, the higher you will rank up in Eridium Leaderboards.\n" +
                        $"**Starboard:** When you star a message it will post that message on starboard.\n**NoAds:** AntiAdvertisement system. No posting discord links.\n" +
                        $"**Mod:** Sets channel for ban/kick logging.\n\n" +
                        $"If a channel isn't set for a toggle action, it will show a small tip when you enable an action saying that the channel isn't set for that action and it's advised you set the channel " +
                        $"to get the feature to work to it's fullest.";
                    break;
                case 3:
                    Title = $"Help With Tags";
                    GuideMessage = $"Can't create a tag? Or tag doesn't exist? Tag command(s) works like this:\n" +
                        $"`{BotDB.Config.Prefix}Tag <TagNameToRun>` `{BotDB.Config.Prefix}Tag Create <NameOfTheTag> <TagResponse>`";
                    break;
                case 4:
                    Title = $"I think I found a bug! OR I need to give feedback!";
                    GuideMessage = $"If you find a bug or want to give feedback, please run the `{BotDB.Config.Prefix}Report` command. The command will guide you thoroughly.";
                    break;
            }
            var embed = Vmbed.Embed(VmbedColors.Pastel, Title: Title, Description: GuideMessage, ThumbUrl: "https://png.icons8.com/open-book/dusk/250");
            await ReplyAndDeleteAsync("", embed: embed, timeout: TimeSpan.FromSeconds(60));
        }
    }
}