using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using Discord.WebSocket;
using Discord.Commands;
using Discord;
using Rick.Services;
using Rick.Models;
using Discord.Addons.InteractiveCommands;
using System.IO;
using System;

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotModel config;
        private GuildModel model;
        private EventService Logger;
        private InteractiveService Interactive;

        public CommandHandler(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            config = _map.Get<BotModel>();
            model = _map.Get<GuildModel>();
            Logger = _map.Get<EventService>();
            Interactive = _map.Get<InteractiveService>();
            client.MessageReceived += HandleCommandsAsync;
            cmds = new CommandService();
            map = _map;
        }

        public async Task ConfigureAsync()
        {
            await cmds.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandsAsync(SocketMessage msg)
        {
            var gld = (msg.Channel as SocketGuildChannel).Guild;
            var AfkList = GuildModel.GuildConfigs[gld.Id].AfkList;
            var OwnerAfk = BotModel.BotConfig.OwnerAfk;

            string log = $"[{DateTime.Now.ToString("hh:mm")}] [{gld.Name} || {gld.Id}] [{msg.Channel.Name} || {msg.Channel.Id}] [{msg.Author.Username} || {msg.Author.Id}] [{msg.Id}] {msg.Content} {msg.Embeds}";
            using (StreamWriter file = new StreamWriter("Logs.txt", true))
            {
                await file.WriteLineAsync(log);
            }

            var message = msg as SocketUserMessage;

            string afkReason = null;
            SocketUser gldUser = message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason) || OwnerAfk.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await message.Channel.SendMessageAsync(afkReason);

            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot) return;
            int argPos = 0;
            var context = new SocketCommandContext(client, message);

            if (!(message.HasStringPrefix(BotModel.BotConfig.DefaultPrefix, ref argPos) || config.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildModel.GuildConfigs[gld.Id].GuildPrefix, ref argPos))) return;

            var result = await cmds.ExecuteAsync(context, argPos, map, MultiMatchHandling.Best);

            var service = cmds.Search(context, argPos);

            CommandInfo Command = null;

            if (service.IsSuccess)
                Command = service.Commands.FirstOrDefault().Command;

            if (result.IsSuccess)
                return;

            string ErrorMsg = null;

            switch(result)
            {
                case SearchResult search:
                    break;
                case ParseResult parse:
                    ErrorMsg = $":x: Failed to provide required parameters!\n**Usage:** {BotModel.BotConfig.DefaultPrefix}{Command.Name} {string.Join("", Command.Parameters.Select(x => x.Name))}";
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
                await context.Channel.SendMessageAsync(ErrorMsg);
        }

        private async void DefaultCommandError(ExecuteResult result, SearchResult res, SocketCommandContext context)
        {
            if (BotModel.BotConfig.DebugMode)
            {
                var embed = new EmbedBuilder();
                embed.Color = new Color(150, 16, 25);
                embed.WithAuthor(x =>
                {
                    x.Name = $"Error Executing Command || Command Name: {res.Commands.FirstOrDefault().Command.Name}";
                    x.IconUrl = client.CurrentUser.GetAvatarUrl();
                });
                embed.Description = $"**Error Reason:**\n{result.ErrorReason}\n\n**Target Site:**\n{result.Exception.TargetSite}\n\n**Stack Trace:**";
                embed.WithFooter(x =>
                {
                    x.Text = result.Exception.StackTrace;
                });
                await context.Channel.SendMessageAsync("", embed: embed);
            }
            else
                await context.Channel.SendMessageAsync($"{string.Concat(Format.Bold("ERROR: "), result.ErrorReason)}");
        }
        
    }
}