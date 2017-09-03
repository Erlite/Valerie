using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Server;

namespace Valerie.Extensions
{
    public class CommandBase : ModuleBase<ICommandContext>
    {
        protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            await ServerConfig.SaveAsync(Context.Guild.Id).ConfigureAwait(false);
            await Context.Channel.TriggerTypingAsync();
            return await Context.Channel.SendMessageAsync(message, isTTS, embed, options);
        }
    }
}