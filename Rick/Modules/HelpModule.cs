using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Discord.Commands;
using Discord;
using Rick.Extensions;

namespace Rick.Modules
{
    public class HelpModule : ModuleBase
    {
        private CommandService _service;
        HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Help"), Summary("Shows a list of all commands."), Alias("Cmds")]
        public async Task CommandsAsync()
        {
            string Description =
                "**Admin Commands:** Prefix, AddRole, RemoveRole, WelcomeAdd, WelcomeRemove, LeaveAdd, LeaveRemove, Toggle, Channel, Kick, Ban, Delete, PurgeUser, PurgeChanel, AddRole, RemoveRole" +
                "**Audio Commands:** Join, Leave, Play, Skip, Queue, Qadd, QClear\n" +
                "**Bot Commands:** [Group: Bot] Prefix, Avatar, Game, Username, Nickname\n" +
                "**General Commands:** Rank, Top, AFK, Iam, IamNot, Slotmachine, Flip, GuildInfo, RoleInfo, Rate, Translate, Trump, Avatar, Yomama, Probe, Discrim\n" +
                "**Giphy Commands:** [Group: Giphy] Tag, Stickers\n**Example:** Giphy Wat is love, Giphy Tag Love\n" +
                "**Google Commands:** Google, GImage, Youtube, Shorten, Revav\n" +
                "**Nsfw Commands:** Boobs, Ass, E621, Porn\n" +
                "**Owner Commands:** Serverlist, LeaveGuild, Boardcast, GetInvite, Archive, Blacklist, Whitelist, Eval, EvalList, EvalRemove, EvalAdd, SendMsg\n" +
                "**Search Commands:** Urban, Lmgtfy, Imgur, Robohash, Wiki, AdorableAvatar, DuckDuckGo, Docs, BImage, Bing, SNews, Suser\n" +
                "**Tag Commands:** [Group: Tag] Create, Remove, Info, Modify, List, Find\n**Example:** Tag How-To, Tag Remove TagName\n" +
                "**Twitter Commands:** Tweet, TweetMedia, Reply, DeleteTweet";
            var embed = Vmbed.Embed(VmbedColors.Gold, Context.Client.CurrentUser.GetAvatarUrl(), $"Commands List.", Description: Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Help"), Summary("Displays information about a specific command.")]
        public async Task HelpAsync(string CommandName)
        {
            var result = _service.Search(Context, CommandName);
            var SB = new StringBuilder();

            if (!result.IsSuccess)
            {
                await ReplyAsync($"**Command Name:** {CommandName}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                return;
            }

            var builder = new EmbedBuilder()
            {
                Color = new Color(179, 56, 216)
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                string Aliases = null;
                if (cmd.Aliases == null || cmd.Aliases.Count <= 0)
                    Aliases = "This command has no Aliases.";
                else
                    Aliases = string.Join(", ", cmd.Aliases);

                builder.Title = cmd.Name.ToUpper();
                builder.Description = $"**Aliases:** {Aliases}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                    $"**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", false, builder.Build());
        }
    }
}
