using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using GPB.Handlers;
using System.Linq;

namespace GPB.Modules
{
    public class HelpModule : ModuleBase
    {
        private CommandService _service;
        private ConfigHandler Config;

        public HelpModule(CommandService service, ConfigHandler config)
        {
            _service = service;
            Config = config;
        }

        [Command("Help")]
        public async Task HelpAsync()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(179, 56, 216),
                Description = $"**Admin Module:** Kick, Ban, Serverlist, Response, Leave, Delete, Gift" +
                "\n**Bot Module:** [Group = Set] Username, Nickname, Avatar, Game, Status" +
                "\n**General Module:** Guildinfo, Gif, Urban, Ping, Gift, Top, Roleinfo" + 
                "\n**Help Module:** Help, Help" + 
                "\n**Log Module:** [Group = Log] ModChannel, ServerChannel, Actions, Joins, Leaves, NameChange, NickChange, Banlog, Latency, AutoRespond",
                Author = new EmbedAuthorBuilder()
                {
                    Name = "Command List",
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
                },
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = "https://cdn4.iconfinder.com/data/icons/ionicons/512/icon-help-circled-128.png",
                    Text = "What is this, 90's Conan?"
                }
            };

            await ReplyAsync("", false, builder.Build());
        }

        [Command("Help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"**Command Name:** {command}\n**Error:** Not Found!\n**Reason:** Wubbalubbadubdub!");
                return;
            }
            var builder = new EmbedBuilder()
            {
                Color = new Color(179, 56, 216)
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;
                builder.Title = cmd.Name.ToUpper();
                builder.Description = $"**Aliases:** {string.Join(", ", cmd.Aliases)}\n**Parameters:** {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n"+
                    $"**Remarks:** {cmd.Remarks}\n**Summary:** {cmd.Summary}";
            }
            await ReplyAsync("", false, builder.Build());
        }
    }
}