using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
            Config.Prefix = NewPrefix;
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

        [Command("Toggle"), Summary("Enables/Disables various guild's actions. ValueType include: Eridium, NoAds")]
        public async Task ToggleAsync(CommandEnums ValueType)
        {
            switch (ValueType)
            {
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
            }
        }

        [Command("Channel"), Summary("Sets channel for varios guild's actions. ValueType include: CB, Join, Eridium, Leave, Starboard, Mod.")]
        public async Task ChannelAsync(CommandEnums ValueType, ITextChannel Channel)
        {
            switch (ValueType)
            {
                case CommandEnums.CB:
                    Config.ChatterChannel = $"{Channel.Id}";
                    await ReplyAsync($"Chatterbot channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Join:
                    Config.JoinChannel = $"{Channel.Id}";
                    await ReplyAsync($"Join channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Leave:
                    Config.LeaveChannel = $"{Channel.Id}";
                    await ReplyAsync($"Leave channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Starboard:
                    Config.Starboard.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Starboard channel has been set to: {Channel.Mention}");
                    break;
                case CommandEnums.Mod:
                    Config.ModLog.TextChannel = $"{Channel.Id}";
                    await ReplyAsync($"Mod channel has been set to: {Channel.Mention}");
                    break;
            }
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
        public Task LevelAddAsync(IRole Role, int Level)
        {
            if (Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} already exists in level up roles.");
            }
            Config.EridiumHandler.LevelUpRoles.TryAdd(Role.Id, Level);
            return ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles"), Alias("LR")]
        public Task EridiumLevelAsync(IRole Role)
        {
            if (!Config.EridiumHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                return ReplyAsync($"{Role} doesn't exists in level up roles.");
            }
            Config.EridiumHandler.LevelUpRoles.TryRemove(Role.Id, out int Value);
            return ReplyAsync($"{Role} has been removed.");
        }

        [Command("SetLevel"), Summary("Sets Max level for auto roles")]
        public Task SetLevelAsync(int MaxLevel)
        {
            if (MaxLevel < 10)
            {
                return ReplyAsync("Max level can't be lower than 10");
            }
            Config.EridiumHandler.MaxRoleLevel = MaxLevel;
            return ReplyAsync($"Max level has been set to: {MaxLevel}");
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
