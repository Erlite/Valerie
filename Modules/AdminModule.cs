using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Valerie.Handlers.Server;
using Valerie.Extensions;
using Valerie.Attributes;
using Valerie.Modules.Enums;
using Valerie.Handlers.Server.Models;

namespace Valerie.Modules
{
    [RequireBotPermission(ChannelPermission.SendMessages | ChannelPermission.ManageMessages), CustomUserPermission]
    public class AdminModule : CommandBase
    {
        ServerModel GuildConfig => ServerConfig.ConfigAsync(Context.Guild.Id).GetAwaiter().GetResult();
        ServerModel Config => ServerConfig.Config;

        [Command("Prefix"), Summary("Changes guild's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            var GConfig = await ServerConfig.ConfigAsync(Context.Guild.Id);
            GConfig.Prefix = NewPrefix;
            await ReplyAsync("Done.");
        }

        [Command("RoleAdd"), Alias("RA"), Summary("Adds a role to assignable role list.")]
        public async Task RoleAddAsync(IRole Role)
        {
            if (Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} already exists in assignable roles list.");
                return;
            }
            Config.AssignableRoles.Add($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("RoleRemove"), Alias("RR"), Summary("Removes a role from assignable role list.")]
        public async Task RoleRemoveAsync(IRole Role)
        {
            if (!Config.AssignableRoles.Contains($"{Role.Id}"))
            {
                await ReplyAsync($"{Role.Name} doesn't exists in assignable roles list.");
                return;
            }
            Config.AssignableRoles.Remove($"{Role.Id}");
            await ReplyAsync("Done.");
        }

        [Command("WelcomeAdd"), Alias("WA"), Summary("Adds a welcome message to welcome messages list.")]
        public async Task WelcomeAddAsync([Remainder] string WelcomeMessage)
        {
            if (Config.WelcomeMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Welcome Messages.");
                return;
            }
            if (Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message already exists.");
                return;
            }
            Config.WelcomeMessages.Add(WelcomeMessage);
            await ReplyAsync("Welcome Message has been added.");
        }

        [Command("WelcomeRemove"), Alias("WR"), Summary("Removes a welcome message from welcome messages list.")]
        public async Task WelcomeRemoveAsync([Remainder] string WelcomeMessage)
        {
            if (!Config.WelcomeMessages.Contains(WelcomeMessage))
            {
                await ReplyAsync("Welcome message doesn't exist.");
                return;
            }
            Config.WelcomeMessages.Remove(WelcomeMessage);
            await ReplyAsync("Welcome Message has been removed");
        }

        [Command("LeaveAdd"), Alias("LA"), Summary("Adds a leave message to leave messages list.")]
        public async Task LeaveAddAsync([Remainder] string LeaveMessage)
        {
            if (Config.LeaveMessages.Count == 3)
            {
                await ReplyAsync("Can't have more than 3 Leave Messages.");
                return;
            }
            if (Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message already exists.");
                return;
            }
            Config.LeaveMessages.Add(LeaveMessage);
            await ReplyAsync("Leave Message has been added.");
        }

        [Command("LeaveRemove"), Alias("LR"), Summary("Removes a leave message from leaves message list.")]
        public async Task LeaveRemoveAsync([Remainder] string LeaveMessage)
        {
            if (!Config.LeaveMessages.Contains(LeaveMessage))
            {
                await ReplyAsync("Leave message doesn't exist.");
                return;
            }
            Config.LeaveMessages.Remove(LeaveMessage);
            await ReplyAsync("Leave Message has been removed.");
        }

        [Command("Toggle"), 
            Summary("Enables/Disables various guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod, NoAds, DM.")]
        public async Task ToggleAsync(CommandEnums ValueType)
        {
            switch (ValueType)
            {
                case CommandEnums.CB:
                    if (!Config.Chatterbot.IsEnabled)
                    {
                        Config.Chatterbot.IsEnabled = true;
                        await ReplyAsync($"Chatterbot has been enabled. {StringExtension.Suggestion("Chatterbot", Config.Chatterbot.TextChannel)}");
                    }
                    else
                    {
                        Config.Chatterbot.IsEnabled = false;
                        await ReplyAsync("Chatterbot has been disabled.");
                    }
                    break;
                case CommandEnums.Join:
                    if (!Config.JoinLog.IsEnabled)
                    {
                        Config.JoinLog.IsEnabled = true;
                        await ReplyAsync($"Join event has been enabled. {StringExtension.Suggestion("Join", Config.JoinLog.TextChannel)}");
                    }
                    else
                    {
                        Config.JoinLog.IsEnabled = false;
                        await ReplyAsync("Join event has been disabled.");
                    }
                    break;
                case CommandEnums.Eridium:
                    if (!Config.EridiumHandler.IsEnabled)
                    {
                        Config.EridiumHandler.IsEnabled = true;
                        await ReplyAsync("Eridium has been enabled.");
                    }
                    else
                    {
                        Config.EridiumHandler.IsEnabled = false;
                        await ReplyAsync("Eridium has been disabled.");
                    }
                    break;
                case CommandEnums.Leave:
                    if (!Config.LeaveLog.IsEnabled)
                    {
                        Config.LeaveLog.IsEnabled = true;
                        await ReplyAsync($"Leave logging has been enabled. {StringExtension.Suggestion("Leave", Config.LeaveLog.TextChannel)}");
                    }
                    else
                    {
                        Config.LeaveLog.IsEnabled = false;
                        await ReplyAsync("Leave logging has been disabled.");
                    }
                    break;
                case CommandEnums.Starboard:
                    if (!Config.Starboard.IsEnabled)
                    {
                        Config.Starboard.IsEnabled = true;
                        await ReplyAsync($"Starboard has been enabled. {StringExtension.Suggestion("Starboard", Config.Starboard.TextChannel)}");
                    }
                    else
                    {
                        Config.Starboard.IsEnabled = false;
                        await ReplyAsync("Starboard has been disabled.");
                    }
                    break;
                case CommandEnums.Mod:
                    if (!Config.ModLog.IsEnabled)
                    {
                        Config.ModLog.IsEnabled = true;
                        await ReplyAsync($"Mod log has been enabled. {StringExtension.Suggestion("Mod", Config.ModLog.TextChannel)}");
                    }
                    else
                    {
                        Config.ModLog.IsEnabled = false;
                        await ReplyAsync("Mod log has been disabled.");
                    }
                    break;
                case CommandEnums.NoAds:
                    if (!Config.ModLog.AntiAdvertisement)
                    {
                        Config.ModLog.AntiAdvertisement = true;
                        await ReplyAsync("AntiAdvertisement has been enabled.");
                    }
                    else
                    {
                        Config.ModLog.AntiAdvertisement = false;
                        await ReplyAsync("AntiAdvertisement has been disabled.");
                    }
                    break;
                case CommandEnums.DM:
                    if (!Config.EridiumHandler.IsDMEnabled)
                    {
                        Config.EridiumHandler.IsDMEnabled = true;
                        string Tip = string.IsNullOrWhiteSpace(Config.EridiumHandler.LevelUpMessage) ? 
                            "Level up message hasn't been set. Use Guide command and read topic #2." : "";
                        await ReplyAsync($"Level Up DM has been enabled.\n{Tip}");
                    }
                    else
                    {
                        Config.EridiumHandler.IsDMEnabled = false;
                        await ReplyAsync("Level Up DM has been enabled.");
                    }
                    break;
                case CommandEnums.AutoAssign:
                    if (!Config.ModLog.IsAutoRoleEnabled)
                    {
                        Config.ModLog.IsAutoRoleEnabled = true;
                        string Tip = string.IsNullOrWhiteSpace(Config.ModLog.AutoAssignRole) ? 
                            "Auto assign role hasn't been set. Use `--SetAutoRole` command to set role." : "";
                        await ReplyAsync($"Auto role assigning has been enabled.\n{Tip}");
                    }
                    else
                    {
                        Config.ModLog.IsAutoRoleEnabled = true;
                        await ReplyAsync("Auto role assigning has been disabled");
                    }
                    break;
            }
        }

        [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod.")]
        public async Task ChannelAsync(CommandEnums ValueType, ITextChannel Channel)
        {
            switch (ValueType)
            {
                case CommandEnums.CB:
                    Config.Chatterbot.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Chatterbot channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Join:
                    Config.JoinLog.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Join log channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Leave:
                    Config.LeaveLog.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Leave log channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Starboard:
                    Config.Starboard.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Starboard channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Mod:
                    Config.ModLog.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Mod log channel has been set to: {Channel.Mention}");
                    break;
            }
        }

        [Command("Settings"), Summary("Displays Guild's settings.")]
        public async Task SettingsAsync()
        {
            string AFKList = null;
            if (Config.AFKList.Count <= 0)
                AFKList = $"AFK List is empty.";
            else
                AFKList = $"AFK Users: {Config.AFKList.Count}";

            string TagList = null;
            if (Config.TagsList.Count <= 0)
                TagList = $"No Tag(s).";
            else
                TagList = $"Total Tags: {Config.TagsList.Count}";

            string WelcomeMessages = null;
            if (!Config.WelcomeMessages.Any())
                WelcomeMessages = "No Welcome Message(s).";
            else
                WelcomeMessages = string.Join("\n", Config.WelcomeMessages.Select(x => x));

            string LeaveMessages = null;
            if (!Config.LeaveMessages.Any())
                LeaveMessages = "No Leave Message(s).";
            else
                LeaveMessages = string.Join("\n", Config.LeaveMessages.Select(x => x));

            string AssignableRoles = null;
            if (Config.AssignableRoles.Count <= 0)
                AssignableRoles = $"No Assignable Role(s).";
            else
                AssignableRoles = $"{Config.AssignableRoles.Count} assignable AssignableRoles.";

            SocketGuildChannel JoinChannel;
            SocketGuildChannel LeaveChannel;
            SocketGuildChannel BanChannel;
            SocketGuildChannel ChatterBotChannel;
            SocketGuildChannel SBChannel;

            if (Config.JoinLog.TextChannel != null || Config.LeaveLog.TextChannel != null ||
                Config.ModLog.TextChannel != null || Config.Starboard.TextChannel != null)
            {
                JoinChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(Config.JoinLog.TextChannel)) as SocketGuildChannel;
                LeaveChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(Config.LeaveLog.TextChannel)) as SocketGuildChannel;
                BanChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(Config.ModLog.TextChannel)) as SocketGuildChannel;
                ChatterBotChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(Config.Chatterbot.TextChannel)) as SocketGuildChannel;
                SBChannel = await Context.Guild.GetChannelAsync(Convert.ToUInt64(Config.Starboard.TextChannel)) as SocketGuildChannel;
            }
            else
            {
                JoinChannel = null;
                LeaveChannel = null;
                BanChannel = null;
                ChatterBotChannel = null;
                SBChannel = null;
            }

            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"SETTINGS | {Context.Guild}");

            embed.AddInlineField("Prefix", Config.Prefix);
            embed.AddInlineField("Mute Role", Config.ModLog.MuteRole);
            embed.AddInlineField("Mod Cases", Config.ModLog.Cases);
            embed.AddInlineField("AntiAdvertisement", Config.ModLog.AntiAdvertisement ? "Enabled" : "Disabled");
            embed.AddInlineField("AFK List", AFKList);
            embed.AddInlineField("Tags List", TagList);

            var Eridium = Config.EridiumHandler;
            var EEnabled = Eridium.IsEnabled ? "Enabled" : "Disabled";
            string BRoles = null;
            if (!Eridium.BlacklistedRoles.Any())
                BRoles = "No Blacklisted Roles";
            else
                BRoles = string.Join(", ", Eridium.BlacklistedRoles.Select(x => Context.Guild.GetRole(UInt64.Parse(x)).Name));
            string LRoles = null;
            if (!Eridium.LevelUpRoles.Any())
                LRoles = "No LevelUp Roles";
            else
                LRoles = string.Join(", ", Eridium.LevelUpRoles.Select(x => Context.Guild.GetRole(x.Key).Name));
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Eridium Stats";
                x.Value = $"**Enabled?** {EEnabled}\n**Total Users:** {Eridium.UsersList.Count}\n**Max Level:** {Eridium.MaxRoleLevel}" +
                $"\n**Blacklisted Roles:** {BRoles}\n**LevelUp Roles:** {LRoles}";
            });

            var JEnabled = Config.JoinLog.IsEnabled ? "Enabled" : "Disabled";
            var LEnabled = Config.LeaveLog.IsEnabled ? "Enabled" : "Disabled";
            var CEnabled = Config.Chatterbot.IsEnabled ? "Enabled" : "Disabled";
            var SEnabled = Config.Starboard.IsEnabled ? "Enabled" : "Disabled";
            embed.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Events";
                x.Value = $"**Join:** {JEnabled} ({JoinChannel})\n**Leave:** {LEnabled} ({LeaveChannel})\n" +
                $"**Starboard:** {SEnabled} ({SBChannel})\n**Chatter Bot:** {CEnabled} ({ChatterBotChannel})";
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Welcome Messages";
                x.Value = WelcomeMessages;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Leave Messages";
                x.Value = LeaveMessages;
            });
            embed.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Assignable Roles";
                x.Value = AssignableRoles;
            });
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("EridiumBlacklist"), Summary("Adds/removes a role to/from blacklisted roles"), Alias("EB")]
        public async Task BlacklistRoleAsync(Actions Action, IRole Role)
        {
            switch (Action)
            {
                case Actions.Add:
                    if (Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} already exists in roles blacklist."); return;
                    }
                    Config.EridiumHandler.BlacklistedRoles.Add($"{Role.Id}");
                    await ReplyAsync($"{Role} has been added."); break;

                case Actions.Remove:
                    if (!Config.EridiumHandler.BlacklistedRoles.Contains($"{Role.Id}"))
                    {
                        await ReplyAsync($"{Role} doesn't exists in roles blacklist."); return;
                    }
                    Config.EridiumHandler.BlacklistedRoles.Remove($"{Role.Id}");
                    await ReplyAsync($"{Role} has been removed."); break;
            }
        }

        [Command("LevelAdd"), Summary("Adds a level to level up list."), Alias("LA")]
        public async Task LevelAddAsync(IRole Role, int Level)
        {
            if (Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} already exists in level up roles."); return;
            }
            Config.EridiumHandler.LevelUpRoles.TryAdd(Role.Id, Level);
            await ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles"), Alias("LR")]
        public async Task EridiumLevelAsync(IRole Role)
        {
            if (!Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} doesn't exists in level up roles."); return;
            }
            Config.EridiumHandler.LevelUpRoles.TryRemove(Role.Id, out int Value);
            await ReplyAsync($"{Role} has been removed.");
        }

        [Command("SetLevel"), Summary("Sets Max level for auto roles")]
        public async Task SetLevelAsync(int MaxLevel)
        {
            if (MaxLevel < 10)
            {
                await ReplyAsync("Max level can't be lower than 10"); return;
            }
            Config.EridiumHandler.MaxRoleLevel = MaxLevel;
            await ReplyAsync($"Max level has been set to: {MaxLevel}");
        }

        [Command("SetAutoRole"), Summary("Sets auto assign role for when user joins.")]
        public Task SetAutoAssignRoleAsync(IRole Role)
        {
            Config.ModLog.AutoAssignRole = $"{Role.Id}";
            return ReplyAsync($"**Auto assign role has been set to {Role}** :v:");
        }

        [Command("LevelUpMessage"), Alias("LUM"), Summary("Sets Level Up message.")]
        public async Task LevelUpMessageAsync([Remainder]string Message)
        {
            Config.EridiumHandler.LevelUpMessage = Message;
            await ReplyAsync("Done.");
        }

        [Command("EridiumRemove"), Summary("Removes a user from Eridium list."), Alias("ER")]
        public async Task EridiumRemoveAsync(IGuildUser User)
        {
            if (!Config.EridiumHandler.UsersList.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User} was not found in Eridium list.");
                return;
            }
            Config.EridiumHandler.UsersList.TryRemove(User.Id, out int Value);
            await ReplyAsync($"{User} was removed from Eridium list.");
        }
    }
}
