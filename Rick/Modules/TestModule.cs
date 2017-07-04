using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Controllers;
using Rick.Handlers;
using Rick.Functions;
using Discord.WebSocket;
using System.Linq;

namespace Rick.Modules
{
    public class TestModule : ModuleBase
    {
        [Command("Test")]
        public async Task TestAsync(string Discrim)
        {
            var Guilds = (Context.Client as DiscordSocketClient).Guilds;
            var sb = new StringBuilder();
            foreach (var gld in Guilds)
            {
                var dis = gld.Users.Where(x => x.Discriminator == Discrim && x.Username != Context.User.Username);
                foreach (var d in dis)
                {
                    sb.AppendLine(d.Username);
                }
            }
            await ReplyAsync(sb.ToString());
        }
    }
}
