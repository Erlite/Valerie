using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Handlers.ModuleHandler.Interactive
{
    public class Interactive<T> : IInteractive<T>
    {
        List<IInteractive<T>> InteractiveList = new List<IInteractive<T>>();

        public Interactive<T> AddInteractive(IInteractive<T> interactive)
        {
            InteractiveList.Add(interactive);
            return this;
        }

        public async Task<bool> JudgeAsync(IContext Context, T TypeParameter)
        {
            foreach (var interactive in InteractiveList)
            {
                var Result = await interactive.JudgeAsync(Context, TypeParameter);
                if (!Result) return false;
            }
            return true;
        }
    }

    public class InteractiveUser : IInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message)
            => Task.FromResult(Context.User.Id == Message.Author.Id);
    }

    public class InteractiveChannel : IInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message)
            => Task.FromResult(Context.Channel.Id == Message.Channel.Id);
    }
}