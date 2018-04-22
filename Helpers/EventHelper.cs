﻿using System;
using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Handlers;
using Valerie.Services;
using Discord.Commands;
using Discord.WebSocket;
using Cookie.Cleverbot.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Valerie.Helpers
{
    public class EventHelper
    {
        Random Random { get; }
        GuildHelper GuildHelper { get; }
        DiscordSocketClient Client { get; }
        ConfigHandler ConfigHandler { get; }
        MethodHelper MethodHelper { get; }
        WebhookService WebhookService { get; }
        Dictionary<ulong, Response> CleverbotTracker { get; set; }

        public EventHelper(Random random, GuildHelper GH, DiscordSocketClient client, ConfigHandler CH,
            MethodHelper MH, WebhookService WS)
        {
            Client = client;
            Random = random;
            GuildHelper = GH;
            ConfigHandler = CH;
            MethodHelper = MH;
            WebhookService = WS;
            CleverbotTracker = new Dictionary<ulong, Response>();
        }

        internal Task XPHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            var User = Message.Author as IGuildUser;
            var BlacklistedRoles = new List<ulong>(Config.ChatXP.ForbiddenRoles.Select(x => x));
            var HasRole = (User as IGuildUser).RoleIds.Intersect(BlacklistedRoles).Any();
            if (HasRole || !Config.ChatXP.IsEnabled) return Task.CompletedTask;
            var Profile = GuildHelper.GetProfile(User.GuildId, User.Id);
            int Old = Profile.ChatXP;
            Profile.ChatXP += Random.Next(Message.Content.Length);
            var New = Profile.ChatXP;
            GuildHelper.SaveProfile(Convert.ToUInt64(Config.Id), User.Id, Profile);
            return LevelUpHandlerAsync(Message, Config, Old, New);
        }

        internal Task ExecuteTagAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Config.Tags.Any(x => x.AutoRespond == true)) return Task.CompletedTask;
            var Tags = Config.Tags.Where(x => x.AutoRespond == true);
            var Content = Tags.FirstOrDefault(x => Message.Content.StartsWith(x.Name));
            if (Content != null) return Message.Channel.SendMessageAsync(Content.Content);
            return Task.CompletedTask;
        }

        internal async Task AFKHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            if (!Message.MentionedUsers.Any(x => Config.AFK.ContainsKey(x.Id))) return;
            string Reason = null;
            var User = Message.MentionedUsers.FirstOrDefault(u => Config.AFK.TryGetValue(u.Id, out Reason));
            if (User != null) await Message.Channel.SendMessageAsync($"**{User.Username} has left an AFK Message:**  {Reason}");
        }

        internal void RecordCommand(CommandService CommandService, IContext Context)
        {
            var Search = CommandService.Search(Context, 0);
            if (!Search.IsSuccess) return;
            var Command = Search.Commands.FirstOrDefault().Command;
            var Profile = GuildHelper.GetProfile(Context.Guild.Id, Context.User.Id);
            if (!Profile.Commands.ContainsKey(Command.Name)) Profile.Commands.Add(Command.Name, 0);
            Profile.Commands[Command.Name]++;
            GuildHelper.SaveProfile(Context.Guild.Id, Context.User.Id, Profile);
        }

        internal async Task ModeratorAsync(SocketUserMessage Message, GuildModel Config)
        {
            if (GuildHelper.ProfanityMatch(Message.Content))
                await WarnUserAsync(Message, Config, $"{Message.Author.Mention}, Refrain from using profanity. You've been warned.");
            if (GuildHelper.InviteMatch(Message.Content))
                await WarnUserAsync(Message, Config, $"{Message.Author.Mention}, No invite links allowed. You've been warned.");
        }

        internal async Task CleverbotHandlerAsync(SocketMessage Message, GuildModel Config)
        {
            string UserMessage = Message.Content.ToLower().Replace("valerie", string.Empty);
            if (!Message.Content.ToLower().StartsWith("valerie") || string.IsNullOrWhiteSpace(UserMessage)) return;
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
            }
            else
            {
                Profile.Warnings++;
                GuildHelper.SaveProfile(Guild.Id, Message.Author.Id, Profile);
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