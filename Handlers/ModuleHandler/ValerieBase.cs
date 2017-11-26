using System;
using Discord;
using Discord.Commands;
using Valerie.JsonModels;
using Valerie.Modules.Addons;
using System.Threading.Tasks;

namespace Valerie.Handlers.ModuleHandler
{
    public class ValerieBase : ModuleBase<IContext>
    {
        public async Task<IUserMessage> ReplyAsync(string Message)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, null, null);
        }

        public async Task<IUserMessage> SaveAsync(ModuleEnums Action, string Message = null)
        {
            bool Check = false;
            switch (Action)
            {
                case ModuleEnums.Server:
                    Context.ServerHandler.Save(Context.Server, Context.Guild.Id);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
                case ModuleEnums.Config:
                    Context.ConfigHandler.Save(Context.Config);
                    Check = !Context.Session.Advanced.HasChanges;
                    break;
            }
            if (Check == true) return await ReplyAsync(Message ?? "✅ - Done.");
            return await ReplyAsync(Message ?? "❌ - There was an error.");
        }

        public async Task LogAsync(IGuildUser User, CaseType CaseType, string Reason)
        {
            var ModChannel = await Context.Guild.GetTextChannelAsync(Convert.ToUInt64(Context.Server.ModLog.TextChannel));
            if (ModChannel == null) return;
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.ModLog.ModCases.Count + 1} <Reason>`*";
            var Message = await ModChannel.SendMessageAsync($"**{CaseType}** | Case {Context.Server.ModLog.ModCases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
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
            Context.ServerHandler.Save(Context.Server, Context.Guild.Id);
        }
    }
}