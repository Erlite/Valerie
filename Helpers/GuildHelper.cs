﻿using System;
using Discord;
using System.Linq;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Handlers;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Valerie.Helpers
{
    public class GuildHelper
    {
        GuildHandler GuildHandler { get; }
        DiscordSocketClient Client { get; }
        public GuildHelper(GuildHandler guildHandler, DiscordSocketClient client)
        {
            Client = client;
            GuildHandler = guildHandler;
        }

        string ProfanityRegex { get => @"\b(f+u+c+k+|b+i+t+c+h+|w+h+o+r+e+|c+u+n+t+|a+s+s+|n+i+g+g+|f+a+g+|g+a+y+|p+u+s+s+y+)(w+i+t+|e+r+|i+n+g+|h+o+l+e+)?\b"; }
        string InviteRegex { get => @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?(d+i+s+c+o+r+d+|a+p+p)+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$"; }

        public IMessageChannel DefaultChannel(ulong GuildId)
        {
            var Guild = Client.GetGuild(GuildId);
            return Guild.TextChannels.FirstOrDefault(x => x.Name.Contains("general") || x.Name.Contains("lobby") || x.Id == Guild.Id) ?? Guild.DefaultChannel;
        }

        public UserProfile GetProfile(ulong GuildId, ulong UserId)
        {
            var Guild = GuildHandler.GetGuild(Client.GetGuild(GuildId).Id);
            if (!Guild.Profiles.ContainsKey(UserId))
            {
                Guild.Profiles.Add(UserId, new UserProfile());
                GuildHandler.Save(Guild);
                return Guild.Profiles[UserId];
            }
            return Guild.Profiles[UserId];
        }

        public void SaveProfile(ulong GuildId, ulong UserId, UserProfile Profile)
        {
            var Config = GuildHandler.GetGuild(GuildId);
            Config.Profiles[UserId] = Profile;
            GuildHandler.Save(Config);
        }

        public async Task LogAsync(IContext Context, IUser User, CaseType CaseType, string Reason)
        {
            Reason = Reason ?? $"*Responsible moderator, please type `{Context.Config.Prefix}Reason {Context.Server.Mod.Cases.Count + 1} <Reason>`*";
            var ModChannel = await Context.Guild.GetTextChannelAsync(Context.Server.Mod.TextChannel);
            if (ModChannel == null) return;
            var Message = await ModChannel.SendMessageAsync($"**{CaseType}** | Case {Context.Server.Mod.Cases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Context.User}");
            Context.Server.Mod.Cases.Add(new CaseWrapper
            {
                User = $"{User}",
                UserId = User.Id,
                Reason = Reason,
                CaseType = CaseType,
                MessageId = Message.Id,
                ModId = Context.User.Id,
                CaseNumber = Context.Server.Mod.Cases.Count + 1
            });
        }

        public async Task LogAsync(SocketGuild Guild, IUser User, IUser Mod, CaseType CaseType, string Reason)
        {
            var Server = GuildHandler.GetGuild(Guild.Id);
            Reason = Reason ?? $"*Responsible moderator, please type `{Server.Prefix}Reason {Server.Mod.Cases.Count + 1} <Reason>`*";
            var ModChannel = Guild.GetTextChannel(Server.Mod.TextChannel);
            if (ModChannel == null) return;
            var Message = await ModChannel.SendMessageAsync($"**{CaseType}** | Case {Server.Mod.Cases.Count + 1}\n**User:** {User} ({User.Id})\n**Reason:** {Reason}\n" +
                    $"**Responsible Moderator:** {Mod}");
            Server.Mod.Cases.Add(new CaseWrapper
            {
                User = $"{User}",
                UserId = User.Id,
                ModId = Mod.Id,
                Reason = Reason,
                CaseType = CaseType,
                MessageId = Message.Id,
                CaseNumber = Server.Mod.Cases.Count + 1
            });
            GuildHandler.Save(Server);
        }

        public Task PurgeAync(IEnumerable<IUserMessage> Messages, ITextChannel Channel, int Amount)
            => Amount <= 100 ? Channel.DeleteMessagesAsync(Messages) :
               Task.Run(() =>
               {
                   for (int i = 0; i < Messages.ToArray().Length; i = i + 100) Channel.DeleteMessagesAsync(Messages.Skip(i).Take(100));
               });

        public (bool, ulong) GetChannelId(SocketGuild Guild, string Channel)
        {
            if (string.IsNullOrWhiteSpace(Channel)) return (true, 0);
            UInt64.TryParse(Channel.Replace('<', ' ').Replace('>', ' ').Replace('#', ' ').Replace(" ", ""), out ulong Id);
            var GetChannel = Guild.GetTextChannel(Id);
            if (GetChannel != null) return (true, Id);
            var FindChannel = Guild.TextChannels.FirstOrDefault(x => x.Name == Channel.ToLower());
            if (FindChannel != null) return (true, FindChannel.Id);
            return (false, 0);
        }

        public (bool, ulong) GetRoleId(SocketGuild Guild, string Role)
        {
            if (string.IsNullOrWhiteSpace(Role)) return (true, 0);
            UInt64.TryParse(Role.Replace('<', ' ').Replace('>', ' ').Replace('@', ' ').Replace('&', ' ').Replace(" ", ""), out ulong Id);
            var GetRole = Guild.GetRole(Id);
            if (GetRole != null) return (true, Id);
            var FindRole = Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == Role.ToLower());
            if (FindRole != null) return (true, FindRole.Id);
            return (false, 0);
        }

        public bool InviteMatch(string Message) => CheckMatch(InviteRegex).Match(Message).Success;

        public bool ProfanityMatch(string Message) => CheckMatch(ProfanityRegex).Match(Message).Success;

        public Regex CheckMatch(string Pattern) => new Regex(Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public (bool, string) ListCheck<T>(List<T> Collection, object Value, string ObjectName, string CollectionName)
        {
            var check = Collection.Contains((T)Value);
            if (Collection.Contains((T)Value)) return (false, $"`{ObjectName}` already exists in {CollectionName}.");
            if (Collection.Count == Collection.Capacity) return (false, $"Reached max number of entries {Emotes.Shout}");
            return (true, $"`{ObjectName}` has been added to {CollectionName}");
        }

        public bool HierarchyCheck(IGuild IGuild, IGuildUser User)
        {
            var Guild = IGuild as SocketGuild;
            var HighestRole = Guild.CurrentUser.Roles.OrderByDescending(x => x.Position).FirstOrDefault().Position;
            return (User as SocketGuildUser).Roles.Any(x => x.Position > HighestRole) ? true : false;
        }
    }
}