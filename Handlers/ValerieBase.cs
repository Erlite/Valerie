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
            if (!string.IsNullOrWhiteSpace(message) || embed != null)
                return await Context.Channel.SendMessageAsync(message, isTTS, embed, options);
            else
                return await Context.Channel.SendMessageAsync("Whoops, something went wrong.");
        }

        public async Task ReactAsync(string GetEmoji)
        {
            await Provider.GetRequiredService<ServerConfig>().SaveAsync(Context.Config, Context.Guild.Id).ConfigureAwait(false);
            await Provider.GetRequiredService<BotConfig>().SaveAsync(Context.ValerieConfig).ConfigureAwait(false);
            await Context.Message.AddReactionAsync(new Emoji(GetEmoji)).ConfigureAwait(false);
        }

        public async Task<IUserMessage> SendMessageAsync(ITextChannel Channel, string Message)
        {
            _ = Provider.GetRequiredService<ServerConfig>().SaveAsync(Context.Config, Context.Guild.Id);
            _ = Provider.GetRequiredService<BotConfig>().SaveAsync(Context.ValerieConfig);
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await Channel.SendMessageAsync(Message).ConfigureAwait(false);
        }
    }
}