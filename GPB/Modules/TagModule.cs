using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.InteractiveCommands;

namespace GPB.Modules
{
    [Group("Tag")]
    public class TagModule : ModuleBase
    {
        private InteractiveService interactive;
        public TagModule(InteractiveService inter)
        {
            interactive = inter;
        }

        [Command("Create")]
        public async Task CreateAsync()
        {
            await ReplyAsync("**What is the name of your tag?** _'cancel' to cancel_");
            var Name = await interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(5));
            if (Name.Content == "cancel") return;
            string name = Name.Content;

            await ReplyAsync("**Enter the tag body:** _'cancel' to cancel_");
            var cont = await interactive.WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(5));
            if (cont.Content == "cancel") return;
            string response = cont.Content;


        }
    }
}