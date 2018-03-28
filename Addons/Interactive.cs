using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Addons
{
    public class Interactive<T> : VInteractive<T>
    {
        List<VInteractive<T>> InteractiveList = new List<VInteractive<T>>();

        public Interactive<T> AddInteractive(VInteractive<T> interactive)
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

    public class InteractiveUser : VInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message) => Task.FromResult(Context.User.Id == Message.Author.Id);
    }

    public class InteractiveChannel : VInteractive<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message) => Task.FromResult(Context.Channel.Id == Message.Channel.Id);
    }

    public interface VInteractive<T>
    {
        Task<bool> JudgeAsync(IContext Context, T TypeParameter);
    }
}