using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rick.Enums;
using Rick.Extensions;
using Rick.Handlers;
using Rick.Roslyn;
using Rick.Functions;
using Rick.Models;

namespace Rick.Modules
{
    [RequireOwner, RequireBotPermission(GuildPermission.SendMessages)]
    public class OwnerModule : ModuleBase
    {
        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(value ?? ""));
        }
        public static IEnumerable<Assembly> Assemblies => Function.GetAssemblies();
        public static IEnumerable<string> Imports => ConfigHandler.IConfig.EvalImports;

        [Command("ServerList"), Summary("Get's a list of all guilds the bot is in."), Alias("Sl")]
        public async Task ServerListAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            var Sb = new StringBuilder();
            foreach (SocketGuild guild in client.Guilds)
            {
                Sb.AppendLine($"{guild.Name} ({guild.Id})\n");
            }
            await (await Context.User.GetOrCreateDMChannelAsync()).SendMessageAsync(Sb.ToString());
            await ReplyAsync("Serverlist has been sent :point_up: ");
        }

        [Command("LeaveGuild"), Summary("Tells the bot to leave a certain guild"), Remarks("Leave 123897481723 This is a message")]
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

        [Command("Broadcast"), Summary("Broadcasts a message to the default channel of all servers the bot is connected to."), Remarks("Broadcast This is a msg")]
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

        [Command("Archive"), Summary("Archives a channel and uploads a JSON"), Remarks("Archive #ChannelName 50")]
        public async Task ArchiveCommand(IMessageChannel Channel, int Amount = 9000)
        {
            if (Amount >= 10000)
            {
                await ReplyAsync("Amount must by less than 9000!");
                return;
            }

            var listOfMessages = new List<IMessage>(await Channel.GetMessagesAsync(Amount).Flatten());

            List<ArchiveModel> list = new List<ArchiveModel>(listOfMessages.Capacity);
            foreach (var message in listOfMessages) list.Add(new ArchiveModel
            {
                Author = message.Author.Username,
                Message = message.Content,
                Timestamp = message.Timestamp,
            });
            var jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(list, Formatting.Indented, jsonSettings);
            await (await Context.User.GetOrCreateDMChannelAsync()).SendFileAsync(GenerateStreamFromString(json), $"{Channel.Name}.json");
            await ReplyAsync($"{Channel.Name}]s Archive has been sent to your DM.");
        }

        [Command("Blacklist"), Summary("Forbids a user from using bot commands"), Remarks("Blacklist @Username Reason")]
        public async Task BlacklistAsync(SocketGuildUser user, [Remainder] string reason = "No reason provided by the owner!")
        {
            if (user.Id == Context.Client.CurrentUser.Id)
                await ReplyAsync("Wow, You think this is funny?");
            var botConfig = ConfigHandler.IConfig;
            var Bl = botConfig.Blacklist;
            if (Bl.ContainsKey(user.Id))
            {
                await ReplyAsync("This user already exist in the blacklist! :skull_crossbones:"); return;
            }
            Bl.Add(user.Id, reason);
            ConfigHandler.IConfig.Blacklist = Bl;
            await ConfigHandler.SaveAsync();
            await ReplyAsync($"{user.Username} has been added to blacklist!");
            var GetOwner = await (Context.Client as DiscordSocketClient).GetApplicationInfoAsync();
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync($"You have been added to my Blacklist for the following reason: {reason}\n\n" +
                $"If you would like to appeal please send a DM to: {GetOwner.Owner.Username}#{GetOwner.Owner.Discriminator}.");
        }

        [Command("Whitelist"), Summary("Removes users from blacklist"), Remarks("Whitelist @username")]
        public async Task WhitelistAsync(SocketGuildUser user)
        {
            var botConfig = ConfigHandler.IConfig;
            var Bl = botConfig.Blacklist;
            if (!Bl.ContainsKey(user.Id))
            {
                await ReplyAsync("This user is not listed in the Blacklist!");
                return;
            }
            Bl.Remove(user.Id);
            ConfigHandler.IConfig.Blacklist = Bl;
            await ConfigHandler.SaveAsync();
            await ReplyAsync($"{user.Username} has been removed from the blacklist!");
            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("You have been removed from my Blacklist! You may use my commands again.");
        }

        [Command("Eval"), Summary("Evaluates some sort of expression for you."), Remarks("Eval Client.Guilds.Count")]
        public async Task EvalAsync([Remainder] string Code)
        {
            var Client = Context.Client as DiscordSocketClient;
            var options = ScriptOptions.Default.AddReferences(Assemblies).AddImports(Imports);
            var working = await Context.Channel.SendMessageAsync("**Evaluating Expression ...**");
            var Globals = new Globals
            {
                Client = Client,
                Context = Context,
                SocketGuild = Context.Guild as SocketGuild,
                SocketGuildChannel = Context.Channel as SocketGuildChannel,
                SocketGuildUser = Context.User as SocketGuildUser
            };

            Code = Code.Trim('`');
            try
            {
                var eval = await CSharpScript.EvaluateAsync(Code, options, Globals, typeof(Globals));
                var embed = EmbedExtension.Embed(EmbedColors.Green,
                    "Expression Evaluated Successfully.", Client.CurrentUser.GetAvatarUrl());
                embed.AddField(x =>
                {
                    x.Name = "Input";
                    x.Value = $"```{Code}```";
                })
                .AddField(x =>
                {
                    x.Name = "Output";
                    x.Value = $"```{eval.ToString()}```";
                });

                await ReplyAsync("", embed: embed);
            }
            catch (Exception e)
            {
                var embed = EmbedExtension.Embed(EmbedColors.Maroon,
                    "Failed To Evaluate Expression.", Client.CurrentUser.GetAvatarUrl(), FooterText: $"From: {e.Source}");
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
                await ReplyAsync("", embed: embed);
            }
            finally
            {
                await working.DeleteAsync();
            }
        }

        [Command("EvalList"), Summary("Shows all of the current namespaces in eval imports")]
        public async Task ListImportsAsync()
        {
            if (ConfigHandler.IConfig.EvalImports.Count == 0)
            {
                await ReplyAsync("Eval Imports list is empty!");
                return;
            }
            await ReplyAsync(string.Join(", ", ConfigHandler.IConfig.EvalImports.Select(x => x)));
        }

        [Command("EvalRemove"), Summary("Removes a namespace from the current eval namespace list"), Remarks("EvalRemove Discord")]
        public async Task RemoveImportAsync(string Namespace)
        {
            if (!ConfigHandler.IConfig.EvalImports.Contains(Namespace))
            {
                await ReplyAsync($"**{Namespace}** doesn't exists in Evan Imports!");
                return;
            }
            ConfigHandler.IConfig.EvalImports.Remove(Namespace);
            await ReplyAsync($"Removed **{Namespace}** from Eval Imports.");
            await ConfigHandler.SaveAsync();
        }

        [Command("EvalAdd"), Summary("Adds a namespace to the current eval namespace list"), Remarks("EvalAdd Discord.Net"),]
        public async Task AddImportAsync(string Namespace)
        {
            if (ConfigHandler.IConfig.EvalImports.Contains(Namespace))
            {
                await ReplyAsync($"**{Namespace}** already exists in Evan Imports!");
                return;
            }
            ConfigHandler.IConfig.EvalImports.Add(Namespace);
            await ReplyAsync($"Added **{Namespace}** to Eval Imports.");
            await ConfigHandler.SaveAsync();
        }

        [Command("Reconnect"), Summary("As Foxbot said: It doesn't get a chance to send a graceful close.")]
        public async Task ReconnectAsync()
        {
            var client = Context.Client as DiscordSocketClient;
            await client.StopAsync();
            await Task.Delay(1000);
            await client.StartAsync();
            await ReplyAsync("Restarted!");
        }

        [Command("Dump"), Summary("Dumps all diagonastic information about Rick's process.")]
        public async Task InfoAsync()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            var AppInfo = Process.GetCurrentProcess();
            var Is64BitStr = Environment.Is64BitProcess ? "Yes" : "No";
            var Is64Bit = IntPtr.Size == 8 ? "Yes" : "No";
            var IsOS64 = Environment.Is64BitOperatingSystem ? "Yes" : "No";
            var isMono = Environment.Is64BitOperatingSystem ? "Yes" : "No";

            long length = new FileInfo(ConfigHandler.ConfigFile).Length + new FileInfo(GuildHandler.configPath).Length;
            string Description = $"{Format.Bold("Info")}\n" +
                                $"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n" +
                                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                                $"- Uptime: {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n\n" +

                                $"{Format.Bold("Stats")}\n" +
                                $"- Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB\n" +
                                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}\n" +
                                $"- Databse Size: {length} Bytes\n\n" +

                                $"{Format.Bold("Full dump of all diagnostic information about this instance.")}\n" +
                                $"- PID: {AppInfo.Id}\n" +
                                $"- Is 64-bit: {Is64BitStr}\n" +
                                $"- Is 64-bit: {Is64Bit}\n" +
                                $"- Thread count: {AppInfo.Threads.Count}\n" +
                                $"- Total processor time: {AppInfo.TotalProcessorTime:c}\n" +
                                $"- User processor time: {AppInfo.UserProcessorTime:c}\n" +
                                $"- Privileged processor time: {AppInfo.PrivilegedProcessorTime:c}\n" +
                                $"- Handle count: {AppInfo.HandleCount:#,##0}\n" +
                                $"- Working set: {AppInfo.WorkingSet64.ToString()}\n" +
                                $"- Virtual memory size: {AppInfo.VirtualMemorySize64.ToString()}\n" +
                                $"- Paged memory size: {AppInfo.PagedMemorySize64.ToString()}\n\n" +

                                $"{Format.Bold("OS and .Net")}\n" +
                                $"- OS platform: {Environment.OSVersion.Platform.ToString()}\n" +
                                $"- OS version: {Environment.OSVersion.Version} ({Environment.OSVersion.VersionString})\n" +
                                $"- OS is 64-bit: {IsOS64}\n" +
                                $"- .NET is Mono: {isMono}\n";
            var embed = EmbedExtension.Embed(EmbedColors.Teal, "Full dump of all diagnostic information about this instance.",
                application.IconUrl, Description: Description);
            var Msg = await ReplyAsync("", embed: embed);
            await Task.Delay(5000);
            await Msg.DeleteAsync();
        }

        [Command("SendMsg"), Summary("Sends messages to a guild"), Remarks("SendMsg GuildID Msg")]
        public async Task SendMsgAsync(ulong ID, [Remainder] string Message)
        {
            var GetGuild = await (await (await Context.Client.GetGuildAsync(ID)).GetDefaultChannelAsync()).SendMessageAsync($"{Format.Bold("From Bot Owner: ")} {Message}");
        }
    }
}