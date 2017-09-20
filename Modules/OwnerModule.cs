﻿using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers;
using Valerie.Handlers.Config;
using Valerie.Models;
using Valerie.Services;
using Valerie.Extensions;

namespace Valerie.Modules
{
    [RequireOwner, RequireBotPermission(ChannelPermission.SendMessages)]
    public class OwnerModule : ValerieBase<ValerieContext>
    {
        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }
        public static IEnumerable<Assembly> Assemblies => Misc.GetAssemblies();
        public static IEnumerable<string> Imports => BotConfig.Config.EvalImports;

        [Command("Blacklist"), Summary("Adds a user to Blacklist.")]
        public async Task BlaclistAsync(IGuildUser User, [Remainder] string Reason = "No reason provided.")
        {
            if (BotConfig.Config.UsersBlacklist.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User.Username} already exists in blacklist.");
                return;
            }
            BotConfig.Config.UsersBlacklist.TryAdd(User.Id, Reason);
            await ReplyAsync($"{User.Username} has been added to blacklist.");
            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync($"You have been added to my Blacklist for the following reason: ```{Reason}```");
        }

        [Command("Whitelist"), Summary("Removes a user from Blacklist.")]
        public async Task WhitelistAsync(IGuildUser User)
        {
            if (!BotConfig.Config.UsersBlacklist.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User.Username} doesn't exist in blacklist."); return;
            }
            BotConfig.Config.UsersBlacklist.TryRemove(User.Id, out string Value);
            await ReplyAsync($"{User.Username} has been removed from Blacklist.");
            await (await User.GetOrCreateDMChannelAsync()).SendMessageAsync("You have been removed from my Blacklist! You may use my commands again.");
        }

        [Command("Eval"), Summary("Evaluates some sort of expression for you.")]
        public async Task EvalAsync([Remainder] string Code)
        {
            var Options = ScriptOptions.Default.AddReferences(Assemblies).AddImports(Imports);
            var Globals = new Globals
            {
                Client = Context.Client as DiscordSocketClient,
                Context = Context,
                Guild = Context.Guild as SocketGuild,
                Channel = Context.Channel as SocketGuildChannel,
                User = Context.User as SocketGuildUser
            };
            try
            {
                var eval = await CSharpScript.EvaluateAsync(Code, Options, Globals, typeof(Globals));
                var embed = ValerieEmbed.Embed(VmbedColors.Green, AuthorName: "Code evaluated successfully.");
                embed.AddField(x =>
                {
                    x.Name = "Input";
                    x.Value = $"```{Code}```";
                })
                .AddField(x =>
                {
                    x.Name = "Output";
                    x.Value = $"```{eval.ToString() ?? "No Result."}```";
                });
                await ReplyAsync("", embed: embed.Build());
            }
            catch (Exception e)
            {
                var embed = ValerieEmbed.Embed(VmbedColors.Red, AuthorName: "Failed to evaluate code.", FooterText: $"From: {e.Source}");
                embed.AddField(x =>
                {
                    x.Name = "Input";
                    x.Value = $"```{Code}```";
                })
                .AddField(x =>
                {
                    x.Name = "Output";
                    x.Value = $"```{e.GetType().ToString()} : {e.Message}```";
                });
                await ReplyAsync("", embed: embed.Build());
            }
        }

        [Command("EvalAdd"), Alias("EA"), Summary("Adds namespaces to Eval's list.")]
        public async Task EvalAddAsync(string Namespace)
        {
            if (BotConfig.Config.EvalImports.Contains(Namespace))
            {
                await ReplyAsync($"**{Namespace}** already exist in Eval Imports.");
                return;
            }
            BotConfig.Config.EvalImports.Add(Namespace);
            await ReplyAsync($"**{Namespace}** namespace has been added to Eval list.");
        }

        [Command("EvalRemove"), Alias("ER"), Summary("Removes a namespace from Eval's list.")]
        public async Task EvalRemoveAsync(string Namespace)
        {
            if (!BotConfig.Config.EvalImports.Contains(Namespace))
            {
                await ReplyAsync($"**{Namespace}** doesn't exist in Eval Imports.");
                return;
            }
            BotConfig.Config.EvalImports.Remove(Namespace);
            await ReplyAsync($"**{Namespace}** namespace has been removed from Eval's list.");
        }

        [Command("Evallist"), Alias("EL"), Summary("Shows a list of all namespaces in Eval's list.")]
        public async Task EvalListAsync()
        {
            if (BotConfig.Config.EvalImports.Count == 0)
            {
                await ReplyAsync("Eval Imports list is empty.");
                return;
            }
            await ReplyAsync(string.Join(", ", BotConfig.Config.EvalImports.Select(x => x)));
        }

        [Command("LeaveGuild"), Summary("Tells the bot to leave a certain guild")]
        public async Task LeaveAsync(ulong ID, [Remainder] string msg = "No reason provided by the owner.")
        {
            var client = Context.Client;
            var gld = await client.GetGuildAsync(ID);
            var ch = await gld.GetDefaultChannelAsync();
            var embed = new EmbedBuilder();
            embed.Description = $"Hello, I've been instructed by my owner to leave this guild!\n**Reason: **{msg}";
            embed.Color = new Color(186, 24, 66);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username,
                IconUrl = Context.User.GetAvatarUrl()
            };
            await ch.SendMessageAsync("", embed: embed.Build());
            await Task.Delay(5000).ConfigureAwait(false);
            await gld.LeaveAsync();
            await ReplyAsync("Message has been sent and I've left the guild!");
        }

        [Command("Broadcast"), Summary("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        public async Task Broadcast([Remainder] string Message)
        {
            var guilds = (Context.Client as DiscordSocketClient).Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync(Message)));
        }

        [Command("GetInvite"), Summary("Makes an invite to the specified guild")]
        public async Task GetInviteAsync(ulong GuildId)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(GuildId)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync(null);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync($"Here is your invite link: <{invite.Url}>");
        }

        [Command("Archive", RunMode = RunMode.Async), Summary("Archives a channel and uploads a JSON"), Remarks("Archive #ChannelName 50")]
        public async Task ArchiveCommand(IMessageChannel Channel, int Amount = 9000)
        {
            if (Amount >= 10000)
            {
                await ReplyAsync("Amount must by less than 9000!");
                return;
            }

            var listOfMessages = new List<IMessage>(await Channel.GetMessagesAsync(Amount).Flatten());

            List<Archive> list = new List<Archive>(listOfMessages.Capacity);
            foreach (var message in listOfMessages) list.Add(new Archive
            {
                Author = message.Author.Username,
                Message = message.Content,
                Attachments = message.Attachments.FirstOrDefault().Url,
                Timestamp = message.Timestamp,
            });
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendFileAsync(GenerateStreamFromString(json), $"{Channel.Name}.json");
            await ReplyAsync($"{Channel.Name}]s Archive has been sent to your DM.");
        }

        [Command("SendMsg"), Summary("Sends messages to a guild")]
        public async Task SendMsgAsync(ulong ID, [Remainder] string Message)
            => await (await (await Context.Client.GetGuildAsync(ID)).GetDefaultChannelAsync()).SendMessageAsync($"{Format.Bold("From Bot Owner: ")} {Message}");
    }

    public class Globals
    {
        public ICommandContext Context { get; internal set; }
        public DiscordSocketClient Client { get; internal set; }
        public SocketGuildUser User { get; internal set; }
        public SocketGuild Guild { get; internal set; }
        public SocketGuildChannel Channel { get; internal set; }
    }
}
