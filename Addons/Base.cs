using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Addons
{
    public class Base : ModuleBase<IContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message, Embed Embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, Embed, null);
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

        async Task<SocketMessage> ResponseWaitAync(VInteractive<SocketMessage> Interactive, TimeSpan? Timeout = null)
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