using System;
using Discord;
using Discord.Commands;
using Valerie.JsonModels;
using Discord.WebSocket;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler.Interactive;

namespace Valerie.Handlers.ModuleHandler
{
    public class ValerieBase : ModuleBase<IContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, null, null);
        }

        public async Task<IUserMessage> SendEmbedAsync(Embed Embed)
        {
            await Context.Channel.TriggerTypingAsync();
            Context.ServerHandler.Save(Context.Server, Context.Guild.Id);
            return await base.ReplyAsync(string.Empty, false, Embed, null);
        }

        public async Task<IUserMessage> SaveAsync(ModuleEnums Action, string Message = null)
        {
            bool Check = false;
            switch (Action)
            {
                case ModuleEnums.Server:
                    Context.ServerHandler.Save(Context.Server, Context.Guild.Id);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
                case ModuleEnums.Config:
                    Context.ConfigHandler.Save(Context.Config);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
            }
            if (Check == true) return await ReplyAsync(Message ?? "✅ - Done.");
            return await ReplyAsync("❌ - BEEP BOOP! SOMETHING IS WRONG! USE FEEDBACK COMMAND TO REPORT THIS ERROR!");
        }

        public async Task LogAsync(IGuildUser User, CaseType CaseType, string Reason)
        {
            var ModChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            if (ModChannel == null) return;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.Cases.Count + 1} <Reason>`*";
            var Message = await ModChannel.SendMessageAsync($"**{CaseType}** | Case {Context.Server.ModLog.Cases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");
            Context.Server.ModLog.Cases.Add(new CaseWrapper()
            {
                Reason = Reason,
                CaseType = CaseType,
                MessageId = $"{Message.Id}",
                ResponsibleMod = $"{Context.User}",
                UserInfo = $"{User.Username} ({User.Id})",
                CaseNumber = Context.Server.ModLog.Cases.Count + 1
            });
            Context.ServerHandler.Save(Context.Server, Context.Guild.Id);
        }

        public async Task<IUserMessage> ReplyAndDeleteAsync(string Message, TimeSpan? Timeout = null)
        {
            Timeout = Timeout ?? TimeSpan.FromSeconds(5);
            var Msg = await ReplyAsync(Message).ConfigureAwait(false);
            _ = Task.Delay(Timeout.Value).ContinueWith(_ => Msg.DeleteAsync().ConfigureAwait(false)).ConfigureAwait(false);
            return Msg;
        }

        public Task<SocketMessage> ResponseWaitAsync(bool User = true, bool Channel = true, TimeSpan? Timeout = null)
        {
            var Interactive = new Interactive<SocketMessage>();
            if (User) Interactive.AddInteractive(new InteractiveUser());
            if (Channel) Interactive.AddInteractive(new InteractiveChannel());
            return ResponseWaitAync(Interactive, Timeout);
        }

        async Task<SocketMessage> ResponseWaitAync(IInteractive<SocketMessage> Interactive, TimeSpan? Timeout = null)
        {
            Timeout = Timeout ?? TimeSpan.FromSeconds(15);
            var Trigger = new TaskCompletionSource<SocketMessage>();
            async Task InteractiveHandlerAsync(SocketMessage Message)
            {
                var Result = await Interactive.JudgeAsync(Context, Message).ConfigureAwait(false);
                if (Result) Trigger.SetResult(Message);
            }
            (Context.Client as DiscordSocketClient).MessageReceived += InteractiveHandlerAsync;
            var PersonalTask = await Task.WhenAny(Trigger.Task, Task.Delay(Timeout.Value)).ConfigureAwait(false);
            (Context.Client as DiscordSocketClient).MessageReceived -= InteractiveHandlerAsync;
            if (PersonalTask == Trigger.Task) return await Trigger.Task.ConfigureAwait(false); else return null;
        }
    }
}