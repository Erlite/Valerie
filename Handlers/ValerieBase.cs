using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Valerie.Handlers.Server;

namespace Valerie.Handlers
{
    public class ValerieBase<T> : ModuleBase<ValerieContext> where T: ValerieContext
    {
        public IServiceProvider Provider { get; set; }

        protected override Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            Provider.GetService<ServerConfig>().Save(Context.Config, Context.Guild.Id);
            Context.Channel.TriggerTypingAsync();
            return Context.Channel.SendMessageAsync(message, isTTS, embed, options);
        }
    }
}
