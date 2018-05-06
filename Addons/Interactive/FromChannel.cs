using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Addons.Interactive
{
    public class FromChannel : ICriteria<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message) => Task.FromResult(Message.Channel.Id == Context.Channel.Id);
    }
}