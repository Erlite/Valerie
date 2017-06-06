using System.Threading.Tasks;
using Discord.Commands;
using Octokit;
using Rick.Services;
using Rick.Attributes;
using Rick.Enums;

namespace Rick.Modules
{
    [Group("Github"), CheckBlacklist]
    public class GithubModule : ModuleBase
    {
        [Command("Userinfo")]
        public async Task UserInfoAsync(string user)
        {
            var github = new GitHubClient(new ProductHeaderValue("Rick"));
            var usr= await github.User.Get(user);
            string Description = $"**Bio:** {usr.Bio}\n**Public Repositories:** {usr.PublicRepos}\n**Private Repositories:** {usr.TotalPrivateRepos}\n**Followers:** {usr.Followers}\n**Company:** {usr.Company}";
            var embed = EmbedService.Embed(EmbedColors.Pastle, usr.Name, usr.AvatarUrl, Description: Description);
            await ReplyAsync("", embed: embed);
        }
    }
}