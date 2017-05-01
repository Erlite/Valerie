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

namespace Rick.Handlers
{
    public class CommandHandler
    {
        private IDependencyMap map;
        private DiscordSocketClient client;
        private CommandService cmds;
        private BotHandler config;
        private GuildHandler model;
        private EventService Logger;
        private InteractiveService Interactive;

        public CommandHandler(IDependencyMap _map)
        {
            client = _map.Get<DiscordSocketClient>();
            config = _map.Get<BotHandler>();
            model = _map.Get<GuildHandler>();
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
            var message = msg as SocketUserMessage;

            LogMessageAsync(message, gld);
            AfkAsync(message, gld);
            LevelUpAsync(message, gld);
            //ProfileKarma(message, gld);

            if (message == null || !(message.Channel is IGuildChannel) || message.Author.IsBot) return;
            int argPos = 0;
            var context = new SocketCommandContext(client, message);

            if (!(message.HasStringPrefix(BotHandler.BotConfig.DefaultPrefix, ref argPos) || BotHandler.BotConfig.MentionDefaultPrefixEnabled(message, client, ref argPos) || message.HasStringPrefix(GuildHandler.GuildConfigs[gld.Id].GuildPrefix, ref argPos))) return;

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
                    ErrorMsg = $":x: Failed to provide required parameters!\n**Usage:** {BotHandler.BotConfig.DefaultPrefix}{Command.Name} {string.Join(", ", Command.Parameters.Select(x => x.Name))}";
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

        private async void LogMessageAsync(SocketUserMessage msg, SocketGuild gld)
        {
            string log = $"[{DateTime.Now.ToString("hh:mm")}] [{gld.Name} || {gld.Id}] [{msg.Channel.Name} || {msg.Channel.Id}] [{msg.Author.Username} || {msg.Author.Id}] [{msg.Id}] {msg.Content}";
            using (StreamWriter file = new StreamWriter("Logs.txt", true))
            {
                await file.WriteLineAsync(log);
            }
        }

        private async void AfkAsync(SocketUserMessage message, SocketGuild gld)
        {
            var AfkList = GuildHandler.GuildConfigs[gld.Id].AfkList;
            string afkReason = null;
            SocketUser gldUser = message.MentionedUsers.FirstOrDefault(u => AfkList.TryGetValue(u.Id, out afkReason) || BotHandler.BotConfig.OwnerAfk.TryGetValue(u.Id, out afkReason));
            if (gldUser != null)
                await message.Channel.SendMessageAsync(afkReason);
        }

        private async void LevelUpAsync(SocketUserMessage message, SocketGuild gld)
        {
            if (message.Author.IsBot) return;

            var Guilds = GuildHandler.GuildConfigs[gld.Id];
            var karmalist = Guilds.Karma;
            if (!karmalist.ContainsKey(message.Author.Id))
            {
                karmalist.Add(message.Author.Id, 1);
                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                ConsoleService.Log(message.Author.Username, $"Added to {gld.Name} Karma's list");
            }
            else
            {
                int getKarma = karmalist[message.Author.Id];
                getKarma++;
                karmalist[message.Author.Id] = getKarma;

                GuildHandler.GuildConfigs[gld.Id] = Guilds;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                ConsoleService.Log(message.Author.Username, $"[{gld.Name}] Got 1 Karma");
            }
        }

        //private async void ProfileKarma(SocketUserMessage message, SocketGuild gld)
        //{
        //    if (message.Author.IsBot) return;

        //    var Guilds = GuildHandler.GuildConfigs[gld.Id];
        //    var KarmaDB = Guilds.KarmaDB;
        //    Random rand = new Random();
        //    double XP = rand.Next(1, 5);

        //    var Profile = new UsersProfile
        //    {
        //        UserID = message.Author.Id,
        //        Karma = GetLevelXP(XP),
        //        ProfileMsg = "",
        //        Level = GetLevelFromXP(XP)
        //    };
        //    KarmaDB.Add(Profile);
        //    GuildHandler.GuildConfigs[gld.Id] = Guilds;
        //    await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);

        //}

        //private double GetLevelXP(double XP)
        //{
        //    return 5 * (Math.Pow(2.0, XP)) + 50 * XP + 100;
        //}

        //private double GetLevelFromXP(double XP)
        //{
        //    int Level = 0;
        //    while(XP >= GetLevelXP(Level))
        //    {
        //        XP -= GetLevelXP(Level);
        //        Level += 1;
        //    }
        //    return Level;

        //}
    }
}