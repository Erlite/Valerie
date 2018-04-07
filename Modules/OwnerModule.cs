using System;
using Discord;
using System.IO;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Valerie.Modules
{
    [Name("Valerie's Owner Commands"), RequireOwner, RequireBotPermission(ChannelPermission.SendMessages)]
    public class OwnerModule : Base
    {
        [Command("Update"), Summary("Updates Valerie's Information.")]
        public async Task UpdateAsync(UpdateType UpdateType, [Remainder] string Value)
        {
            switch (UpdateType)
            {
                case UpdateType.Avatar:
                    using (var Picture = new FileStream(Value, FileMode.Open))
                        await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(Picture));
                    break;
                case UpdateType.Username:
                    await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Value);
                    break;
                case UpdateType.Status:
                    var Split = Value.Split(':');
                    await (Context.Client as DiscordSocketClient).SetActivityAsync(
                        new Game(Split[1], (ActivityType)Enum.Parse(typeof(ActivityType), Split[0]))).ConfigureAwait(false);
                    break;
                case UpdateType.Prefix: Context.Config.Prefix = Value; break;
                case UpdateType.Nickname:
                    await (await Context.Guild.GetCurrentUserAsync(CacheMode.AllowDownload)).ModifyAsync(x => x.Nickname = Value);
                    break;
                case UpdateType.ReportChannel:
                    Context.Config.ReportChannel = Context.GuildHelper.GetChannelId(Context.Guild as SocketGuild, Value).Item2; break;
                case UpdateType.JoinMessage: Context.Config.JoinMessage = Value; break;
            }
            await ReplyAsync($"{UpdateType} has been updated {Emotes.DWink}", Document: DocumentType.Config);
        }

        [Command("GetInvite"), Summary("Makes an invite for the  specified guild.")]
        public async Task GetInviteAsync(ulong GuildId)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(GuildId)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync(null);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Here is your invite link: <{invite.Url}>");
        }

        [Command("GlobalBlacklist"), Summary("Adds or removes a user from the global blacklist.")]
        public Task BlaclistAsync(char Action, IUser User)
        {
            switch (Action)
            {
                case 'a':
                    if (Context.Config.Blacklist.Contains(User.Id)) return ReplyAsync($"{User} is already in global blacklist.");
                    Context.Config.Blacklist.Add(User.Id);
                    return ReplyAsync($"{User} has been added to global blacklist.", Document: DocumentType.Config);
                case 'r':
                    if (!Context.Config.Blacklist.Contains(User.Id)) return ReplyAsync($"{User} isn't globally blacklisted.");
                    Context.Config.Blacklist.Remove(User.Id);
                    return ReplyAsync($"{User} has been removed from global blacklist.", Document: DocumentType.Config);
            }
            return Task.CompletedTask;
        }

        [Command("Eval"), Summary("Evaluates C# code.")]
        public async Task EvalAsync([Remainder] string Code)
        {
            var Message = await ReplyAsync("Debugging ...");
            var Imports = Context.Config.Namespaces.Any() ? Context.Config.Namespaces :
                new[] { "System", "System.Linq", "System.Collections.Generic", "System.IO", "System.Threading.Tasks" }.ToList();
            var Options = ScriptOptions.Default.AddReferences(Context.MethodHelper.GetAssemblies()).AddImports(Imports);
            var Globals = new EvalModel
            {
                Context = Context,
                Guild = Context.Guild as SocketGuild,
                User = Context.User as SocketGuildUser,
                Client = Context.Client as DiscordSocketClient,
                Channel = Context.Channel as SocketGuildChannel
            };
            try
            {
                var Eval = await CSharpScript.EvaluateAsync(Code, Options, Globals, typeof(EvalModel));
                await Message.ModifyAsync(x => x.Content = $"{Eval ?? "No Result Produced."}");
            }
            catch (Exception Ex)
            {
                await Message.ModifyAsync(x => x.Content = Ex.Message ?? Ex.StackTrace);
            }
        }

        [Command("Namespace"), Summary("Shows a list of all namespaces in Valerie's config.")]
        public Task NamespaceAsync()
            => ReplyAsync(Context.Config.Namespaces.Any() ? $"**__Current Namespaces:__**\n{string.Join(", ", Context.Config.Namespaces)}" : "No namespaces.");

        [Command("Namespace"), Summary("Shows a list of all namespaces in Valerie's config.")]
        public Task NamespaceAsync(char Action, string Namespace)
        {
            switch (Action)
            {
                case 'a':
                    if (Context.Config.Namespaces.Contains(Namespace)) return ReplyAsync($"{Namespace} namespace already exists.");
                    Context.Config.Namespaces.Add(Namespace);
                    return ReplyAsync($"{Namespace} has been added.", Document: DocumentType.Config);
                case 'r':
                    if (!Context.Config.Namespaces.Contains(Namespace)) return ReplyAsync($"{Namespace} namespace doesn't exist.");
                    Context.Config.Namespaces.Remove(Namespace);
                    return ReplyAsync($"{Namespace} has been removed.", Document: DocumentType.Config);
            }
            return Task.CompletedTask;
        }

        [Command("Archive"), Summary("Archives a channel and uploads a JSON.")]
        public async Task ArchiveCommand(IMessageChannel Channel, int Amount = 5000)
        {
            var MessagesList = new List<IMessage>(await Channel.GetMessagesAsync(Amount).FlattenAsync()).OrderByDescending(x => x.CreatedAt);
            var SaveFile = Path.Combine(Directory.GetCurrentDirectory(), $"{Channel.Name}.txt");
            if (!File.Exists(SaveFile)) File.Create(SaveFile);
            foreach (var Message in MessagesList)
            {
                var Embed = Message.Embeds.FirstOrDefault();
                var Content = (Message.Content ?? Message.Attachments.FirstOrDefault().Url) ??
                    $"{Environment.NewLine}{Embed?.Description} {Embed?.Url} {Embed?.Video?.Url} {Embed?.Thumbnail} {Embed?.Image?.Url} {Embed?.Image?.Url}";
                await File.AppendAllTextAsync(SaveFile, $"[{Message.CreatedAt}] [{Message.Author}]({Message.Author.Id}) {Content}{Environment.NewLine}");
            }
            await ReplyAsync($"Finished archiving {Amount} messages for {Channel}.");
        }
    }
}