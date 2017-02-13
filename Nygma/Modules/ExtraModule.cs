using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.IO;
using Discord.Addons.InteractiveCommands;
using Nygma.Utils;
using Nygma.Handlers;
using System.Linq;
using System;

namespace Nygma.Modules
{
    public class ExtraModule : InteractiveModuleBase
    {
        private ConfigHandler Config;
        public ExtraModule(ConfigHandler Con)
        {
            Config = Con;
        }

        [Command("hello", RunMode = RunMode.Async)]
        public async Task SayHello()
        {
            await ReplyAsync("What is your name?");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel);
            await ReplyAsync($"Hello, {response.Content}");
        }

        [Command("favoriteanimal", RunMode = RunMode.Async)]
        public async Task FavoriteAnimal()
        {
            await ReplyAsync("What is your favorite animal?");
            var response = await WaitForMessage(Context.Message.Author, Context.Channel, null, new MessageContainsResponsePrecondition("dog", "cat", "giraffe"));
            await ReplyAsync($"Your favorite animal is a {response.Content}!");
        }

        [Command("destroy")]
        public async Task DeleteAfter()
        {
            await ReplyAsync("This message will destroy itself in 5 seconds", deleteAfter: 5);
        }

    }
}