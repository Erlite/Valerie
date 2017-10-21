using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Raven.Client.Documents.Session;

namespace Valerie.Handlers
{
    public class ValerieBase<T> : ModuleBase<ValerieContext> where T : ValerieContext
    {
        public static IServiceProvider Provider { get; set; }
        IDocumentSession Session { get; set; }

        protected override void BeforeExecute(CommandInfo command)
        {
            Session = MainHandler.Store.OpenSession();
            base.BeforeExecute(command);
        }

        protected override void AfterExecute(CommandInfo command)
        {
            if (Session != null)
            {
                Session.Store(Context.Config, id: $"{Context.Guild.Id}");
                Session.Store(Context.ValerieConfig, id: "Config");
                Session.SaveChanges();
            }
            Session.Dispose();
            base.AfterExecute(command);
        }

        protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);
        }

        public async Task ReactAsync(string GetEmoji)
            => await Context.Message.AddReactionAsync(new Emoji(GetEmoji)).ConfigureAwait(false);

        public async Task<IUserMessage> SendMessageAsync(ITextChannel Channel, string Message)
        {
            await Context.Channel.TriggerTypingAsync().ConfigureAwait(false);
            return await Channel.SendMessageAsync(Message).ConfigureAwait(false);
        }
    }
}