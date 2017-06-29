using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Rick.Functions;

namespace Rick.Modules
{
    public class TestModule : ModuleBase
    {
        [Command("Test")]
        public async Task TestAsync(string Msg)
        {
            string Filter = Function.Censor(Msg);
            await ReplyAsync(Filter);
        }
    }
}
