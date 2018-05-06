using Discord.WebSocket;
using System.Threading.Tasks;

namespace Valerie.Addons.Interactive
{
    public class FromUser : ICriteria<SocketMessage>
    {
        public Task<bool> JudgeAsync(IContext Context, SocketMessage Message) => Task.FromResult(Message.Author.Id == Context.User.Id);
    }
}