using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Server;
using Valerie.Handlers.Config;

namespace Valerie.Handlers
{
    public class ValerieBase<T> : ModuleBase<ValerieContext> where T : ValerieContext
    {
        public static IServiceProvider Provider { get; set; }

        protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            _ = Provider.GetRequiredService<ServerConfig>().SaveAsync(Context.Config, Context.Guild.Id);
            _ = Provider.GetRequiredService<BotConfig>().SaveAsync(Context.ValerieConfig);
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            if (message != null || embed != null)
                return await Context.Channel.SendMessageAsync(message, isTTS, embed, options);
            else
                return await Context.Channel.SendMessageAsync("Whoops, something went wrong.");
        }
    }
}