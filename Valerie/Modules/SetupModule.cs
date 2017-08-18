using System;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Valerie.Modules
{
    public class SetupModule : InteractiveBase
    {
        [Command("next")]
        public async Task SetupGuildConfigAsync()
        {
            await ReplyAsync($"Welcome to **{Context.Guild}'s** setup.");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }
    }
}
