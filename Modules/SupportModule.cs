using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.Handlers.Config;

namespace Valerie.Modules
{
    public class SupportModule : InteractiveBase
    {
        [Command("Report"), Alias("Feedback"), Summary("Reports an issue to Bot owner or give feedback.")]
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
            var Channel = Context.Client.GetChannel(Convert.ToUInt64(BotConfig.Config.ReportChannel)) as ITextChannel;
            await Channel.SendMessageAsync("", embed: Embed.Build());
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
                $":two: Toggle Eridium(Chat XP), Etc\n" +
                $":three: Help With Tags!\n" +
                $":four: I think I found a bug! OR I need to give feedback!\n" +
                $":five: What are Schmeckles? What do they do?\n" +
                $":six: Updates\n" +
                $":seven: Valerie's Status Changing", timeout: TimeSpan.FromSeconds(20));
            var Choice = (await NextMessageAsync()).Content;
            if (!int.TryParse(Choice, out int Num))
            {
                await ReplyAndDeleteAsync("Your input wasn't valid! Exiting guide ...");
                return;
            }
            if (Num > 7 || Num < 1)
            {
                await ReplyAndDeleteAsync("Not a valid choice. Exiting guide ...");
                return;
            }
            switch (Num)
            {
                case 1:
                    Title = "GUIDE | NSFW Commands";
                    GuideMessage = $"NSFW Commands can only be ran in a channel that has NSFW enabled or if the channel name starts with or contains \"NSFW\" in it." +
                        $"This is to make sure that users can't user the command outside of NSFW channels.";

                    break;
                case 2:
                    Title = "GUIDE | Toggle Eridium(Chat XP), Etc";
                    GuideMessage = $"Here are the following things you can toggle: Eridium, NoAds\n\n" +
                        $"**Eridium:** Gives Eridium based on your message. The more you talk, the higher you will rank up in Eridium Leaderboards.\n" +
                        $"**NoAds:** AntiAdvertisement system. No posting discord links.\n" +
                        "You need to set custom Level up message! To show user level or mention user in your custom message, " +
                        "you need to type `{user}` `{level}`. This will replace `{user}` and Mention user and replace `{level}` with user's level!";
                    break;
                case 3:
                    Title = "GUIDE | Help With Tags";
                    GuideMessage = $"Can't create a tag? Or tag doesn't exist? Tag command(s) works like this:\n" +
                        $"`{BotConfig.Config.Prefix}Tag <TagNameToRun>` `{BotConfig.Config.Prefix}Tag Create <NameOfTheTag> <TagResponse>`";
                    break;
                case 4:
                    Title = "GUIDE | I think I found a bug! OR I need to give feedback!";
                    GuideMessage = $"If you find a bug or want to give feedback, please run the `{BotConfig.Config.Prefix}Report` command. The command will guide you thoroughly.";
                    break;
                case 5:
                    Title = "GUIDE | What are Schmeckles and what do they do?";
                    GuideMessage = $"Schmeckles are a new form of currency. In order to get Schmeckles, you need to enable Eridium on your server (Check Guide #2). " +
                        $"If you have enough Eridium for the command that requires Schmeckles, the bot will automatically subtract that much Eridium and execute the command succesfully.\n\n" +
                        $"**Why would you add something so stupid?** It's not stupid if it works. The main reason I added this was to make sure users don't abuse a certain command and on the other hand " +
                        $"you get competition in your server for Eridium leaderboards. Who is stupid now?";
                    break;
                case 6:
                    Title = "GUIDE | Updates";
                    GuideMessage = "Valerie is regularly updating to make sure it is using the latest feature of the library and the API's. How do you get the update logs? You don't. " +
                        "Why? They are useless. If you can want, you can check Valerie's commit messages by using the Stats command or visiting Valerie's github repo.";
                    break;
                case 7:
                    Title = "GUIDE | Valerie's Status Changing";
                    GuideMessage = $"Valerie will automatically change it's status (Which is the green/yellow/red dot you see next to someone's avatar) based on it's ping. If the ping is " +
                        $"really bad, the status will be set to DND (Do Not Disturb) and it's advised not to use commands during that periods or keep usage to minimum because it will give somewhat delayed " +
                        $"response.";
                    break;
            }
            var embed = Vmbed.Embed(VmbedColors.Pastel, Title: Title, Description: GuideMessage, ThumbUrl: "https://png.icons8.com/open-book/dusk/250");
            await ReplyAndDeleteAsync("", embed: embed.Build(), timeout: TimeSpan.FromSeconds(60));
        }
    }
}