using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Services;
using Rick.Classes;
using System.Linq;
using Rick.Attributes;
using Rick.Handlers;
using Google.Apis.Customsearch.v1;
using Google.Apis.Services;

namespace Rick.Modules
{
    [Group("Google"), CheckBlacklist]
    public class GoogleModule : ModuleBase
    {
        [Command("Search"), Summary("Google Seach Very wow"), Remarks("Seaches google for your terms")]
        public async Task SearchAsync([Remainder] string search)
        {
            var Str = new StringBuilder();
            var Service = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = BotHandler.BotConfig.GoogleAPIKey
            });
            var RequestList = Service.Cse.List(search);
            RequestList.Cx = BotHandler.BotConfig.SearchEngineID;

            var items = RequestList.Execute().Items.Take(5);
            foreach (var result in items)
            {
                Str.AppendLine($"**=> {result.Title}**\n{result.Link}");
            }
            string URL = "http://diylogodesigns.com/blog/wp-content/uploads/2016/04/google-logo-icon-PNG-Transparent-Background.png";
            var embed = EmbedService.Embed(EmbedColors.Pastle, $"Search for: {search}", Context.Client.CurrentUser.GetAvatarUrl(), null, Str.ToString(), null, null, null, URL);
            await ReplyAsync("", embed: embed);
        }

        [Command("Image"), Summary("Google Image doges"), Remarks("Searches google for your image")]
        public async Task ImageAsync([Remainder] string search)
        {

        }
    }
}
