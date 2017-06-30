using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Controllers;
using Rick.Handlers;

namespace Rick.Modules
{
    public class TestModule : ModuleBase
    {
        [Command("Test")]
        public async Task TestAsync()
        {
            await ReplyAsync("Test");
        }
    }
}
