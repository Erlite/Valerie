using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Rick.Services;
using Discord.Addons.InteractiveCommands;
using System.IO;
using System;
using Rick.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IServiceProvider Provider;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotHandler BotHandler;
        private GuildHandler GuildHandler;
        private EventService EventHandler;
        private InteractiveService Interactive;

        public CommandHandler(IServiceProvider prod)
        {
            Provider = prod;
            client = Provider.GetService<DiscordSocketClient>();
            BotHandler = Provider.GetService<BotHandler>();
            GuildHandler = Provider.GetService<GuildHandler>();
            EventHandler = Provider.GetService<EventService>();
            Interactive = Provider.GetService<InteractiveService>();

            client.MessageReceived += HandleCommandsAsync;
            cmds = Provider.GetService<CommandService>();
        }

        public async Task ConfigureAsync()
        {
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandsAsync(SocketMessage msg)
        {
            var gld = (msg.Channel as SocketGuildChannel).Guild;
            var message = msg as SocketUserMessage;

            await LogMessageAsync(message, gld);
            await AfkAsync(message, gld);
            await ChatKarma(message, gld);

            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot || GuildHandler.GuildConfigs[gld.Id].GuildPrefix == null) return;
            int argPos = 0;
            var context = new SocketCommandContext(client, message);

            string GuildPrefix = GuildHandler.GuildConfigs[gld.Id].GuildPrefix;
            if (!(message.HasStringPrefix(BotHandler.BotConfig.DefaultPrefix, ref argPos) || BotHandler.BotConfig.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildPrefix, ref argPos))) return;

            var result = await cmds.ExecuteAsync(context, argPos, Provider, MultiMatchHandling.Best);
            var service = cmds.Search(context, argPos);
            CommandInfo Command = null;

            if (service.IsSuccess)
                Command = service.Commands.FirstOrDefault().Command;
            if (result.IsSuccess)
                return;

            string ErrorMsg = null;
            Embed embed = null;
            switch(result)
            {
                case SearchResult search:
                    break;
                case ParseResult parse:
                    ErrorMsg = $"User failed to provide paramaters!\n**Command Usage:** {BotHandler.BotConfig.DefaultPrefix}{Command.Name} {string.Join(", ", Command.Parameters.Select(x => x.Name))}\n"+ 
                        $"You can get more info on how to use {Command.Name} by using:\n{BotHandler.BotConfig.DefaultPrefix}Help {Command.Name}";
                    embed = EmbedService.Embed(EmbedColors.Red, "Unmet Precondition Error", client.CurrentUser.GetAvatarUrl(), null, ErrorMsg);
                    break;
                case PreconditionResult pre:
                    ErrorMsg = pre.ErrorReason;
                    break;
                case ExecuteResult exe:
                    var exeresult = (ExecuteResult)result;
                    DefaultCommandError(exeresult, service, context);
                    break;
            }

            if (ErrorMsg != null)
                await context.Channel.SendMessageAsync("", embed: embed);
        }

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, SocketCommandContext context)
        {
            if (BotHandler.BotConfig.DebugMode)
            {
                string Name = $"Error Executing Command || Command Name: {res.Commands.FirstOrDefault().Command.Name}";
                string Description = $"**Error Reason:**\n{result.ErrorReason}\n\n**Target Site:**\n{result.Exception.TargetSite}\n\n**Stack Trace:**";
                string StackTrace = result.Exception.StackTrace;
                var embed = EmbedService.Embed(EmbedColors.Red, Name, client.CurrentUser.GetAvatarUrl(), null, Description, StackTrace);
                await context.Channel.SendMessageAsync("", embed: embed);
            }
            else
                await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        }        

        private async Task LogMessageAsync(SocketUserMessage msg, SocketGuild gld)
        {
            string log = $"[{DateTime.Now.ToString("hh:mm")}] [{gld.Name} || {gld.Id}] [{msg.Channel.Name} || {msg.Channel.Id}] [{msg.Author.Username} || {msg.Author.Id}] [{msg.Id}] {msg.Content}";
            using (StreamWriter file = new StreamWriter("Logs.txt", true))
            {
                await file.WriteLineAsync(log);
            }
        }

        private async Task AfkAsync(SocketUserMessage message, SocketGuild gld)
        {
            var AfkList = GuildHandler.GuildConfigs[gld.Id].AfkList;
            string afkReason = null;
            SocketUser gldUser = message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await message.Channel.SendMessageAsync(afkReason);
        }

        private async Task ChatKarma(SocketUserMessage message, SocketGuild gld)
        {
            var Guilds = GuildHandler.GuildConfigs[gld.Id];
            if (message.Author.IsBot) return;
            Random rand = new Random();
            double RandomKarma = rand.Next(1, 5);
            RandomKarma = MethodService.GiveKarma(RandomKarma);
            if (Guilds.ChatKarma)
            {
                var karmalist = Guilds.Karma;
                if (!karmalist.ContainsKey(message.Author.Id))
                    karmalist.Add(message.Author.Id, 1);
                else
                {
                    int getKarma = karmalist[message.Author.Id];
                    getKarma += Convert.ToInt32(RandomKarma);
                    karmalist[message.Author.Id] = getKarma;
                }
                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            }
        }
    }
}