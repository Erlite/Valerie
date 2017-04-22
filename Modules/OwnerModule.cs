﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;
using Rick.Classes;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Rick.Services;
using Rick.Handlers;

namespace Rick.Modules
{
    [RequireOwner]
    public class OwnerModule : ModuleBase
    {
        private BotConfigHandler Config;
        private EventService Events;
        public OwnerModule(BotConfigHandler config, EventService events)
        {
            Config = config;
            Events = events;
        }
        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }
        public static IEnumerable<Assembly> Assemblies => MethodService.GetAssemblies();
        public static IEnumerable<string> Imports => MethodService.EvalImports;

        [Command("ServerList"), Summary("Normal Command"), Remarks("Get's a list of all guilds the bot is in."), Alias("Sl")]
        public async Task ServerListAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var embed = new EmbedBuilder();
            foreach (SocketGuild guild in client.Guilds)
            {
                embed.AddField(x =>
                {
                    x.Name = $"{guild.Name} || {guild.Id}";
                    x.Value = $"Guild Owner: { guild.Owner} || { guild.OwnerId}\nGuild Members: {guild.MemberCount}";
                    x.IsInline = true;
                });
            }
            embed.Title = "=== Server List ===";
            embed.Color = new Color(244, 66, 113);
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = $"Total Guilds: {client.Guilds.Count.ToString()}",
                IconUrl = "http://tabard.gnomeregan.info/result/faction_Alliance_icon_emblem_00_border_border_00_iconcolor_ffffff_bgcolor_000000_bordercolor_ffffff.png"
            };

            await ReplyAsync("", embed: embed);
        }

        [Command("Leave"), Summary("Leave 123897481723 This is a message"), Remarks("Tells the bot to leave a certain guild")]
        public async Task LeaveAsync(ulong ID, [Remainder] string msg = "No reason provided by the owner.")
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new NullReferenceException("You must provide a reason!");
            if (ID <= 0)
                throw new ArgumentException("Please enter a valid Guild ID");
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
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync("Message has been sent and I've left the guild!");
        }

        [Command("Broadcast"),Summary("Broadcast This is a msg"), Remarks("Broadcasts a message to the default channel of all servers the bot is connected to.")]
        public async Task Broadcast([Remainder] string broadcast = null)
        {
            if (string.IsNullOrWhiteSpace(broadcast))
                throw new NullReferenceException("Broadcast message cannot be empty!");
            var guilds = (Context.Client as DiscordSocketClient).Guilds;
            var defaultChannels = guilds.Select(g => g.GetChannel(g.Id)).Cast<ITextChannel>();
            await Task.WhenAll(defaultChannels.Select(c => c.SendMessageAsync(broadcast)));
        }

        [Command("GetInvite"), Summary("Makes an invite to the specified guild"), Remarks("GI 123456")]
        public async Task GetInviteAsync([Summary("Target guild")]ulong guild)
        {
            var channel = await Context.Client.GetChannelAsync((await Context.Client.GetGuildAsync(guild)).DefaultChannelId);
            var invite = await (channel as SocketGuildChannel).CreateInviteAsync();
            await ReplyAsync(invite.Url);
        }

        [Command("Archive"), Summary("Archive GuildName ChannelName 10"), Remarks("archives a channel and uploads a JSON")]
        public async Task ArchiveCommand(string guildName, string channelName, int amount = 9000)
        {
            var channelToArchive = (await
                (await Context.Client.GetGuildsAsync()).FirstOrDefault(x => x.Name == guildName).GetTextChannelsAsync()).FirstOrDefault(x => x.Name == channelName);
            if (channelToArchive != null)
            {
                var listOfMessages = new List<IMessage>(await channelToArchive.GetMessagesAsync(amount).Flatten());
                List<Message> list = new List<Message>(listOfMessages.Capacity);
                foreach (var message in listOfMessages)
                    list.Add(new Message { Author = message.Author.Username, Content = message.Content, Timestamp = message.Timestamp });
                var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
                await Context.Channel.SendFileAsync(GenerateStreamFromString(json), $"{channelName}.json");
            }
        }

        [Command("Eval"), Summary("Eval 2+2"), Remarks("Evaluates some sort of expression for you.")]
        public async Task EvalAsync([Remainder] string value)
        {
            var client = Context.Client as DiscordSocketClient;
            var options = ScriptOptions.Default.AddReferences(Assemblies).AddImports(Imports);
            var working = await Context.Channel.SendMessageAsync("**Evaluating**, please wait...");
            var _globals = new ScriptGlobals { client = Context.Client as DiscordSocketClient };
            value = value.Trim('`');
            try
            {
                var eval = await CSharpScript.EvaluateAsync(value, options, _globals, typeof(ScriptGlobals));
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = "Evaluated Successfully!";
                        x.IconUrl = client.CurrentUser.GetAvatarUrl();
                    })
                    .WithDescription($"**Input:**```{value}```\n**Output:**```{eval.ToString()}```")
                    .WithColor(new Color(20, 255, 5))
                    .WithFooter(x =>
                    {
                        x.IconUrl = "https://blog.mariusschulz.com/content/images/dotnet_foundation_logo.png";
                        x.Text = "Using Microsoft Code Analysis Csharp Scripting";
                    });

                await ReplyAsync("", embed: embed);
            }
            catch (Exception e)
            {
                var embed = new EmbedBuilder()
                    .WithAuthor(x =>
                    {
                        x.Name = "An Error Occurred";
                        x.IconUrl = client.CurrentUser.GetAvatarUrl();
                    })
                    .WithDescription($"**Input:**```{value}```\n**Output:**```{e.Message.ToString()}```")
                    .WithColor(new Color(255, 6, 14))
                    .WithFooter(x =>
                    {
                        x.IconUrl = "https://blog.mariusschulz.com/content/images/dotnet_foundation_logo.png";
                        x.Text = "Using Microsoft Code Analysis Csharp Scripting";
                    });

                await ReplyAsync("", embed: embed);
            }
            finally
            {
                await working.DeleteAsync();
            }
        }

        [Command("EvalList"), Summary("Evallist"), Remarks("Shows all of the current namespaces in eval imports")]
        public async Task ListImportsAsync()
        {
            await ReplyAsync(string.Join(", ", MethodService.EvalImports.Select(x => x))).ConfigureAwait(false);
        }

        [Command("EvalRemove"), Summary("EvalRemove Discord"), Remarks("Removes a namespace from the current eval namespace list")]
        public async Task RemoveImportAsync([Summary("Namespace name")]string import)
        {
            MethodService.EvalImports.Remove(import);
            await ReplyAsync($"Removed {import}").ConfigureAwait(false);
        }

        [Command("EvalAdd"), Summary("EvalAdd Discord.Net"), Remarks("Adds a namespace to the current eval namespace list")]
        public async Task AddImportAsync([Summary("Namespace name")]string import)
        {
            MethodService.EvalImports.Add(import);
            await ReplyAsync($"Added {import}").ConfigureAwait(false);
        }

        [Command("Latency"), Summary("Normal Command"), Remarks("Enables/Disables monitoring your ping")]
        public async Task LatencyAsync()
        {
            if (!Config.ClientLatency)
            {
                Events.EnableLatencyMonitor();
                await ReplyAsync(":gear: Will AutoUpdate my status based on Ping!");
            }
            else
            {
                Events.DisableLatencyMonitor();
                await ReplyAsync(":skull_crossbones: Latency monitor disabled");
            }
        }
    }
}