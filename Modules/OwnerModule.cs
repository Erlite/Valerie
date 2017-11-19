using System;
using Discord;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace Valerie.Modules
{
    [Name("Valerie's Owner Commands"), RequireOwner]
    public class OwnerModule : ValerieBase
    {
        MemoryStream GenerateStreamFromString(string Value) => new MemoryStream(Encoding.Unicode.GetBytes(Value ?? string.Empty));

        [Command("Blacklist"), Summary("Adds or removes a user from the blacklist.")]
        public async Task BlaclistAsync(ModuleEnums Action, IUser User, [Remainder]string Reason)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Config.UsersBlacklist.ContainsKey(User.Id))
                    {
                        await ReplyAsync($"{User.Username} already exists in blacklist.");
                        return;
                    }
                    Context.Config.UsersBlacklist.Add(User.Id, Reason);
                    await SaveAsync(ModuleEnums.Config);
                    await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync($"You have been blacklisted from using my commands.");
                    break;
                case ModuleEnums.Remove:
                    if (!Context.Config.UsersBlacklist.ContainsKey(User.Id))
                    {
                        await ReplyAsync($"{User.Username} isn't blacklist.");
                        return;
                    }
                    Context.Config.UsersBlacklist.Remove(User.Id);
                    await SaveAsync(ModuleEnums.Config);
                    await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync($"You have been removed from blacklist.");
                    break;
            }
        }

        [Command("Debug"), Summary("Evaluates some sort of expression for you.")]
        public async Task EvalAsync([Remainder] string Code)
        {
            var Message = await ReplyAsync("Debugging ...");
            var Options = ScriptOptions.Default.AddReferences(GetAssemblies()).AddImports(Context.Config.Imports);
            var Globals = new Globals
            {
                Context = Context,
                Guild = Context.Guild as SocketGuild,
                User = Context.User as SocketGuildUser,
                Client = Context.Client as DiscordSocketClient,
                Channel = Context.Channel as SocketGuildChannel
            };
            try
            {
                var Eval = await CSharpScript.EvaluateAsync(Code, Options, Globals, typeof(Globals));
                await Message.ModifyAsync(x => x.Content = $"{Eval ?? "No Result Produced."}");
            }
            catch (Exception Ex)
            {
                await Message.ModifyAsync(x => x.Content = Ex.Message ?? Ex.StackTrace);
            }
        }

        [Command("Namespace"), Alias("Nm"), Summary("Shows a list of all namespaces.")]
        public Task NamespaceAsync()
            => ReplyAsync($"{string.Join(", ", Context.Config.Imports) ?? "No namespaces have been imported."}");

        [Command("Namespace"), Alias("Nm"), Summary("Shows a list of all namespaces in Eval's list.")]
        public Task NamespaceAsync(ModuleEnums Action, string Namespace)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Config.Imports.Contains(Namespace)) return ReplyAsync($"{Namespace} already exists.");
                    Context.Config.Imports.Add(Namespace);
                    return SaveAsync(ModuleEnums.Config);
                case ModuleEnums.Remove:
                    if (!Context.Config.Imports.Contains(Namespace)) return ReplyAsync($"{Namespace} doesn't exist.");
                    Context.Config.Imports.Remove(Namespace);
                    return SaveAsync(ModuleEnums.Config);
            }
            return Task.CompletedTask;
        }

        [Command("GetInvite"), Summary("Makes an invite to the specified guild.")]
        public async Task GetInviteAsync(ulong GuildId)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(GuildId)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync(null);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Here is your invite link: <{invite.Url}>");
        }

        [Command("Archive"), Summary("Archives a channel and uploads a JSON.")]
        public async Task ArchiveCommand(IMessageChannel Channel, int Amount = 5000)
        {
            var listOfMessages = new List<IMessage>(await Channel.GetMessagesAsync(Amount).Flatten());

            List<Archive> list = new List<Archive>(listOfMessages.Capacity);
            foreach (var message in listOfMessages)
                list.Add(new Archive
                {
                    Author = message.Author.Username,
                    Message = message.Content,
                    Attachments = message.Attachments.FirstOrDefault().Url,
                    Timestamp = message.Timestamp,
                });
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
            await Context.Channel.SendFileAsync(GenerateStreamFromString(json), $"{Channel.Name}.json");
        }

        [Command("SendMsg"), Summary("Sends messages to a guild.")]
        public async Task SendMsgAsync(ulong ID, [Remainder] string Message)
            => await (await (await Context.Client.GetGuildAsync(ID)).GetDefaultChannelAsync()).SendMessageAsync($"{Format.Bold("From Bot Owner: ")} {Message}");

        [Command("Prefix"), Summary("Changes bot's prefix.")]
        public Task PrefixAsync(string NewPrefix)
        {
            Context.Config.Prefix = NewPrefix;
            return SaveAsync(ModuleEnums.Config);
        }

        [Command("Avatar"), Summary("Changes Bot's avatar.")]
        public async Task AvatarAsync([Remainder] string Path)
        {
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                await Context.Client.CurrentUser.ModifyAsync(x
                    => x.Avatar = new Image(stream));
                stream.Dispose();
            }
            await ReplyAsync("Avatar has been updated.");
        }

        [Command("Game"), Summary("Adds a game to bot's game list and sets it as current bot's game.")]
        public Task GameAsync(ModuleEnums Action, [Remainder] string GameName)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Config.Games.Contains(GameName))
                        return ReplyAsync($"{GameName} already exists.");
                    Context.Config.Games.Add(GameName);
                    return SaveAsync(ModuleEnums.Config);
                case ModuleEnums.Remove:
                    if (!Context.Config.Games.Contains(GameName))
                        return ReplyAsync($"{GameName} doesn't exist.");
                    Context.Config.Games.Remove(GameName);
                    return SaveAsync(ModuleEnums.Config);
            }
            return Task.CompletedTask;
        }

        [Command("Username"), Summary("Changes Bot's username.")]
        public async Task UsernameAsync([Remainder] string Username)
            => await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Username);

        [Command("Nickname"), Summary("Changes Bot's nickname")]
        public async Task NicknameAsync([Remainder] string Nickname)
            => await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = Nickname);

        [Command("ServerMessage"), Summary("Custom Guild Join Message")]
        public Task GuildJoinMessageAsync([Remainder] string ServerMessage)
        {
            Context.Config.ServerMessage = ServerMessage;
            return ReplyAsync("Server message updated.");
        }

        [Command("ReportChannel"), Summary("Sets report channel.")]
        public Task ReportChannelAsync(ITextChannel Channel)
        {
            Context.Config.ReportChannel = $"{Channel.Id}";
            return ReplyAsync("Report channel updated.");
        }

        IEnumerable<Assembly> GetAssemblies()
        {
            var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
            foreach (var Ass in Assemblies)
                yield return Assembly.Load(Ass);

            yield return Assembly.GetEntryAssembly();
            yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
        }
    }
}