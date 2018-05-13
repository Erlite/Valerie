using System;
using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Services;
using Valerie.Handlers;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using Cookie.Cleverbot.Models;
using System.Collections.Generic;

namespace Valerie.Helpers
{
    public class EventHelper
    {
        Random Random { get; }
        GuildHelper GuildHelper { get; }
        DiscordSocketClient Client { get; }
        MethodHelper MethodHelper { get; }
        ConfigHandler ConfigHandler { get; }
        WebhookService WebhookService { get; }
        Dictionary<ulong, DateTime> XPUserList { get; set; }
        Dictionary<ulong, Response> CleverbotTracker { get; set; }
        public static readonly TimeSpan GlobalTimeout = TimeSpan.FromSeconds(30);

        public EventHelper(Random random, GuildHelper GH, DiscordSocketClient client, ConfigHandler CH,
            MethodHelper MH, WebhookService WS)
        {
            Client = client;
            Random = random;
            GuildHelper = GH;
            ConfigHandler = CH;
            MethodHelper = MH;
            WebhookService = WS;
            XPUserList = new Dictionary<ulong, DateTime>();
            CleverbotTracker = new Dictionary<ulong, Response>();
        }

        internal Task RunTasks(SocketMessage Message, GuildModel Config)
        {
            XPHandlerAsync(Message, Config);
            ExecuteTagAsync(Message, Config);
            AFKHandlerAsync(Message, Config);
            CleverbotHandlerAsync(Message, Config).ConfigureAwait(false);
            ModeratorAsync(Message as SocketUserMessage, Config);
            return Task.CompletedTask;
        }

        internal async Task CheckStateAsync()
        {
            if (Client.ConnectionState == ConnectionState.Connected) return;

            var Timeout = Task.Delay(GlobalTimeout);
            var Connect = Client.StartAsync();
            var LocalTask = await Task.WhenAny(Timeout, Connect);

            if (LocalTask == Timeout || Connect.IsFaulted) Environment.Exit(1);
            else if (Connect.IsCompletedSuccessfully)
            {
                LogService.Write(Enums.LogSource.DSD, "Client Reset Completed.", System.Drawing.Color.ForestGreen);
                return;
            }
            else Environment.Exit(1);
        }

        Task XPHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            var User = Message.Author as IGuildUser;
            var GetTime = XPUserList.ContainsKey(User.Id) ? XPUserList[User.Id] : DateTime.UtcNow;
            var BlacklistedRoles = new List<ulong>(Config.ChatXP.ForbiddenRoles.Select(x => x));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (HasRole || !Config.ChatXP.IsEnabled || !(GetTime.AddSeconds(60) > DateTime.UtcNow)) return Task.CompletedTask;
            var Profile = GuildHelper.GetProfile(User.GuildId, User.Id);
            int Old = Profile.ChatXP;
            Profile.ChatXP += Random.Next(Message.Content.Length);
            var New = Profile.ChatXP;
            GuildHelper.SaveProfile(Convert.ToUInt64(Config.Id), User.Id, Profile);
            if (XPUserList.ContainsKey(User.Id)) XPUserList.Remove(User.Id);
            XPUserList.Add(User.Id, DateTime.UtcNow);
            return LevelUpHandlerAsync(Message, Config, Old, New);
        }

        Task ExecuteTagAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Config.Tags.Any(x => x.AutoRespond == true)) return Task.CompletedTask;
            var Tags = Config.Tags.Where(x => x.AutoRespond == true);
            var Content = Tags.FirstOrDefault(x => Message.Content.StartsWith(x.Name));
            if (Content != null) return Message.Channel.SendMessageAsync(Content.Content);
            return Task.CompletedTask;
        }

        Task AFKHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Message.MentionedUsers.Any(x => Config.AFK.ContainsKey(x.Id))) return Task.CompletedTask;
            string Reason = null;
            var User = Message.MentionedUsers.FirstOrDefault(u => Config.AFK.TryGetValue(u.Id, out Reason));
            if (User != null) return Message.Channel.SendMessageAsync($"**{User.Username} has left an AFK Message:**  {Reason}");
            return Task.CompletedTask;
        }

        internal void RecordCommand(CommandInfo Command, IContext Context)
        {
            if (Command == null) return;
            var Profile = GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (!Profile.Commands.ContainsKey(Command.Name)) Profile.Commands.Add(Command.Name, 0);
            Profile.Commands[Command.Name]++;
            GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
        }

        Task ModeratorAsync(SocketUserMessage Message, GuildModel Config)
        {
            if (GuildHelper.ProfanityMatch(Message.Content) && Config.Mod.AntiProfanity)
                return WarnUserAsync(Message, Config, $"{Message.Author.Mention}, Refrain from using profanity. You've been warned.");
            if (GuildHelper.InviteMatch(Message.Content) && Config.Mod.AntiInvite)
                return WarnUserAsync(Message, Config, $"{Message.Author.Mention}, No invite links allowed. You've been warned.");
            return Task.CompletedTask;
        }

        async Task CleverbotHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            string UserMessage = Message.Content.ToLower().Replace("valerie", string.Empty);
            if (!Message.Content.ToLower().StartsWith("valerie") || string.IsNullOrWhiteSpace(UserMessage)
                || Message.Channel.Id != Config.CleverbotWebhook.TextChannel) return;
            Response CleverResponse;
            if (!CleverbotTracker.ContainsKey(Config.CleverbotWebhook.TextChannel))
            {
                CleverResponse = await ConfigHandler.Cookie.Cleverbot.TalkAsync(UserMessage);
                CleverbotTracker.Add(Config.CleverbotWebhook.TextChannel, CleverResponse);
            }
            else
            {
                CleverbotTracker.TryGetValue(Config.CleverbotWebhook.TextChannel, out CleverResponse);
                CleverResponse = await ConfigHandler.Cookie.Cleverbot.TalkAsync(UserMessage);
                CleverbotTracker[Config.CleverbotWebhook.TextChannel] = CleverResponse;
            }
            await WebhookService.SendMessageAsync(new WebhookOptions
            {
                Message = CleverResponse.CleverOutput,
                Name = "Cleverbot",
                Webhook = Config.CleverbotWebhook
            });
        }

        async Task WarnUserAsync(SocketUserMessage Message, GuildModel Config, string Warning)
        {
            var Guild = (Message.Author as SocketGuildUser).Guild;
            if (Config.Mod.MaxWarnings == 0 || Message.Author.Id == Guild.OwnerId) return;
            await Message.DeleteAsync();
            var Profile = GuildHelper.GetProfile(Guild.Id, Message.Author.Id);
            if (Profile.Warnings >= Config.Mod.MaxWarnings)
            {
                await (Message.Author as SocketGuildUser).KickAsync("Kicked By AutoMod.");
                await Guild.GetTextChannel(Config.Mod.TextChannel).SendMessageAsync(
                    $"**Kick** | Case {Config.Mod.Cases.Count + 1}\n**User:** {Message.Author} ({Message.Author.Id})\n**Reason:** Reached Max Warnings.\n" +
                    $"**Responsible Moderator:** {Client.CurrentUser}");
                await GuildHelper.LogAsync(Guild, Message.Author, Client.CurrentUser, CaseType.Kick, Warning);
            }
            else
            {
                Profile.Warnings++;
                GuildHelper.SaveProfile(Guild.Id, Message.Author.Id, Profile);
                await GuildHelper.LogAsync(Guild, Message.Author, Client.CurrentUser, CaseType.Warning, Warning);
            }
            await Message.Channel.SendMessageAsync(Warning);
        }

        async Task LevelUpHandlerAsync(SocketMessage Message, GuildModel Config, int OldXp, int NewXp)
        {
            var User = Message.Author as SocketGuildUser;
            int OldLevel = IntHelper.GetLevel(OldXp);
            int NewLevel = IntHelper.GetLevel(NewXp);
            if (!(NewLevel > OldLevel)) return;
            int Crystals = (int)Math.Sqrt(Math.PI * NewXp);
            var Profile = GuildHelper.GetProfile(User.Guild.Id, User.Id);
            Profile.Crystals += Crystals;
            GuildHelper.SaveProfile(User.Guild.Id, User.Id, Profile);
            if (!string.IsNullOrWhiteSpace(Config.ChatXP.LevelMessage))
                await Message.Channel.SendMessageAsync(StringHelper.Replace(Config.ChatXP.LevelMessage, User: $"{User}", Level: NewLevel, Crystals: Crystals));
            if (!Config.ChatXP.LevelRoles.Any()) return;
            var Role = User.Guild.GetRole(Config.ChatXP.LevelRoles.Where(x => x.Value == NewLevel).FirstOrDefault().Key);
            if (User.Roles.Contains(Role) || !User.Guild.Roles.Contains(Role)) return;
            await User.AddRoleAsync(Role);
            foreach (var lvlrole in Config.ChatXP.LevelRoles)
                if (lvlrole.Value < NewLevel) if (!User.Roles.Contains(User.Guild.GetRole(lvlrole.Key))) await User.AddRoleAsync(User.Guild.GetRole(lvlrole.Key));
        }
    }
}