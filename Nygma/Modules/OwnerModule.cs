using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Nygma.Services;
using Newtonsoft.Json;
using Nygma.Utils;
using System.IO;
using System.Diagnostics;

namespace Nygma.Modules
{
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        private readonly IDependencyMap _map;
        public OwnerModule(IDependencyMap map)
        {
            _map = map;
        }
        private DiscordSocketClient client;
        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }

        [Command("Broadcast")]
        [Remarks("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        public async Task Broadcast([Remainder] string broadcast)
        {
            var guilds = client.Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync(broadcast)));
        }

        [Command("Eval", RunMode = RunMode.Sync)]
        [Summary("evaluates C# script")]
        [RequireOwner]
        public async Task EvalCommand([Remainder]string script)
        {
            var evalService = new EvalService(_map);
            var scriptTask = evalService.Evaluate(Context, script);
            await Task.Delay(10000);
            if (!scriptTask.IsCompleted) evalService.PopToken();
        }

        [Command("archive")]
        [Summary("archives a channel and uploads a JSON")]
        [RequireOwner]
        public async Task ArchiveCommand(string guildName, string channelName, int amount = 10000)
        {
            var channelToArchive = (await 
                (await Context.Client.GetGuildsAsync()).FirstOrDefault(x => x.Name == guildName).GetTextChannelsAsync()).FirstOrDefault(x => x.Name == channelName);
            if (channelToArchive != null)
            {
                var listOfMessages = new List<IMessage>(await channelToArchive.GetMessagesAsync(amount).Flatten());
                List<Message> list = new List<Message>(listOfMessages.Capacity);
                foreach (var message in listOfMessages)
                    list.Add(new Message { Author = message.Author.Username, Content = message.Content, Timestamp = message.Timestamp });
                var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
                await Context.Channel.SendFileAsync(GenerateStreamFromString(json), $"{channelName}.json");
            }
        }

        [Command("Exit"), Summary("Exit"), Remarks("Kills the poor soul")]
        public async Task ExitAsync()
        {
            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            embed.Title = "Exiting Application";
            embed.Description = "Closing in 5 seconds";
            await Context.Channel.SendMessageAsync("", false, embed);
            await Task.Delay(5000);
            Environment.Exit(0);
        }

        [Command("Restart"), Summary("Restart 5"), Remarks("TIME IS IN FUCKING SECONDS!")]
        public async Task RestartAsync(int time)
        {
            var embed = new EmbedBuilder();
            embed.Title = "Restarting Application";
            embed.Description = $"Restarting in {time} seconds";
            embed.Color =Misc.RandColor();
            var z = await Context.Channel.SendMessageAsync("", false, embed);
            await Task.Delay(3000);
            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }

    }
}