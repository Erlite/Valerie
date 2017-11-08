using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Models;
using Valerie.Handlers.ModuleHandler;
using Valerie.Services;
using Discord.Commands;
using System.Reflection;

namespace Valerie.Handlers
{
    public class EventsHandler
    {
        IServiceProvider Provider;
        DiscordSocketClient Client;
        ServerHandler ServerHandler;
        CommandService CommandService;
        public EventsHandler(ServerHandler ServerParam, DiscordSocketClient ClientParam, CommandService CmdParam)
        {
            Client = ClientParam;
            CommandService = CmdParam;
            ServerHandler = ServerParam;
        }

        public async Task InitializeAsync(IServiceProvider IServiceProvider)
        {
            Provider = IServiceProvider;
            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        internal Task LogAsync(LogMessage Log) => Task.Run(() => LogClient.Write(Source.DISCORD, Log.Message ?? Log.Exception.Message));

        internal async Task GuildAvailableAsync(SocketGuild Guild) => await ServerHandler.AddServerAsync(new ServerModel { Id = $"{Guild.Id}", Prefix = "." }).ConfigureAwait(false);

        internal async Task LeftGuildAsync(SocketGuild Guild) => await ServerHandler.DeleteServerAsync(Guild.Id).ConfigureAwait(false);

        internal async Task JoinedGuildAsync(SocketGuild Guild)
        {
            await ServerHandler.AddServerAsync(new ServerModel
            {
                Id = $"{Guild.Id}",
                Prefix = "."
            }).ConfigureAwait(false);
        }

        internal async Task HandleCommandAsync(SocketMessage Message)
        {
            if(!(Message is SocketUserMessage Msg)) return;
            int argPos = 0;
            var Context = new IContext(Client, Msg, Provider);
            if (!(Msg.HasStringPrefix(Context.Config.Prefix, ref argPos) || Msg.HasStringPrefix(Context.Server.Prefix, ref argPos)) ||
                Msg.Source != MessageSource.User || Msg.Author.IsBot || Context.Config.UsersBlacklist.ContainsKey(Msg.Author.Id)) return;
            var Result = await CommandService.ExecuteAsync(Context, argPos, null, MultiMatchHandling.Best);
            switch (Result.Error)
            {
                case CommandError.Exception: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.Unsuccessful: LogClient.Write(Source.DISCORD, Result.ErrorReason); break;
                case CommandError.UnmetPrecondition: await Context.Channel.SendMessageAsync(Result.ErrorReason); break;
            }
        }

        internal async Task HandleMessageAsync(SocketMessage Message)
        {

        }

        internal async Task UserJoinedAsync(SocketGuildUser User)
        {

        }

        internal async Task UserLeftAsync(SocketGuildUser User)
        {

        }

        internal async Task UserBannedAsync(SocketUser User, SocketGuild Guild)
        {

        }

        internal async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {

        }

        internal async Task ReactionRemovedAsync(Cacheable<IUserMessage, ulong> Cache, ISocketMessageChannel Channel, SocketReaction Reaction)
        {

        }

        internal async Task ReadyAsync()
        {

        }

        internal async Task LatencyUpdatedAsync(int Old, int New)
        {

        }
    }
}