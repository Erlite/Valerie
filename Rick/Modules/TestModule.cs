using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Controllers;
using Rick.Handlers;
using Rick.Functions;
namespace Rick.Modules
{
    public class TestModule : ModuleBase
    {
        [Command("Test")]
        public async Task TestAsync([Remainder]string Message)
        {
            if (Function.Advertisement(Message))
            {
                await ReplyAsync("OK");
            }
            else
                await ReplyAsync("None");
        }
    }
}
