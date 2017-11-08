using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Valerie.Handlers.ModuleHandler
{
    public class ValerieBase : ModuleBase<IContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message, Embed GetEmbed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, GetEmbed, null);
        }

        public async Task<IUserMessage> SaveAsync(string Message = null)
        {
            var check = await Context.ServerHandler.UpdateServerAsync(Context.Guild.Id, Context.Server).ConfigureAwait(false);
            if (check == true) return await ReplyAsync(Message ?? "✔️ - Done.");
            return await ReplyAsync(Message ?? "✖️ - There was an error.");
        }
    }
}