using System.Threading.Tasks;
using Discord.Commands;
using Octokit;
using Rick.Services;
using Rick.Classes;

namespace Rick.Modules
{
    [Group("Github")]
    public class GithubModule : ModuleBase
    {
        [Command("Userinfo")]
        public async Task UserInfoAsync(string user)
        {
            var github = new GitHubClient(new ProductHeaderValue("Rick"));
            var usr= await github.User.Get(user);
            string Description = $"**Bio:** {usr.Bio}\n**Public Repositories:** {usr.PublicRepos}\n**Private Repositories:** {usr.TotalPrivateRepos}\n**Followers:** {usr.Followers}\n**Company:** {usr.Company}";
            var embed = EmbedService.Embed(EmbedColors.Pastle, usr.Name, usr.AvatarUrl, null, Description);
            await ReplyAsync("", embed: embed);
        }
    }
}