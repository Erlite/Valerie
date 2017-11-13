﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Models;

namespace Valerie.Handlers.ModuleHandler
{
    public class ValerieBase : ModuleBase<IContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, null, null);
        }

        public async Task<IUserMessage> SaveAsync(string Message = null)
        {
            var check = await Context.ServerHandler.UpdateServerAsync(Context.Guild.Id, Context.Server).ConfigureAwait(false);
            if (check == true) return await ReplyAsync(Message ?? "✅ - Done.");
            return await ReplyAsync(Message ?? "✖️ - There was an error.");
        }

        public async Task LogAsync(IGuildUser User, CaseType CaseType, string Reason)
        {
            var ModChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            if (ModChannel == null) return;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";
            var Message = await ModChannel.SendMessageAsync($"**{typeof(CaseType).Name}** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");
            Context.Server.ModLog.ModCases.Add(new CaseWrapper()
            {
                Reason = Reason,
                CaseType = CaseType,
                MessageId = $"{Message.Id}",
                ResponsibleMod = $"{Context.User}",
                UserInfo = $"{User.Username} ({User.Id})",
                CaseNumber = Context.Server.ModLog.ModCases.Count + 1
            });
            await Context.ServerHandler.UpdateServerAsync(Context.Guild.Id, Context.Server).ConfigureAwait(false);
        }

        public async Task<IUserMessage> SendEmbedAsync(Embed Embed)
        {
            await Context.ServerHandler.UpdateServerAsync(Context.Guild.Id, Context.Server).ConfigureAwait(false);
            return await base.ReplyAsync(null, false, Embed);
        }
    }
}