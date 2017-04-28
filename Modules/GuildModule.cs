using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Handlers;
using Discord.WebSocket;
using Rick.Classes;
using Rick.Attributes;

namespace Rick.Modules
{
    [Group("Guild"), RequireUserPermission(GuildPermission.Administrator), CheckBlacklist]
    public class GuildModule : ModuleBase
    {
        private GuildHandler model;
        private EventService Log;

        public GuildModule(GuildHandler gld, EventService Logger)
        {
            model = gld;
            Log = Logger;
        }

        [Command("ModChannel"), Summary("ModChannel #ChannelName"), Remarks("Sets the Modchannel to log bans, etc")]
        public async Task SetModLogChannelAsync(ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.ModChannelID = channel.Id;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Mod Channel has been set to **{channel.Name}**");
        }

        [Command("SetPrefix"), Summary("SetPrefix ?"), Remarks("Sets Guild prefix")]
        public async Task SetPrefixAsync(string prefix)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.GuildPrefix = prefix;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Prefix has been set to: **{prefix}**");
        }

        [Command("WelcomeMsg"), Summary("WelcomeMsg This is a welcome Msg"), Remarks("Sets welcome message for your server")]
        public async Task WelcomeMessageAsync([Remainder]string msg)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            gldConfig.WelcomeMessage = msg;
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
            await ReplyAsync($"Guild Welcome Message has been set to:\n```{msg}```");
        }

        [Command("Logs"), Summary("Normal Command"), Remarks("Shows what Actions are being logged")]
        public async Task ListLogActionsAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.IconUrl = Guild.IconUrl;
                    x.Name = $"{Guild.Name} || {Guild.Owner.Username}";
                })
                .WithDescription($"**Server Mod Channel:** {GuildHandler.GuildConfigs[Guild.Id].ModChannelID}\n**Guild Prefix:** {GuildHandler.GuildConfigs[Guild.Id].GuildPrefix}\n"  +
                $"**Welcome Message:** {GuildHandler.GuildConfigs[Guild.Id].WelcomeMessage}\n**User Join Logging:** {GuildHandler.GuildConfigs[Guild.Id].JoinLogs}\n**User Leave Logging:** {GuildHandler.GuildConfigs[Guild.Id].LeaveLogs}\n" +
                $"**Username Change Logging:** {GuildHandler.GuildConfigs[Guild.Id].NameChangesLogged}\n **Nickname Change Logging:** {GuildHandler.GuildConfigs[Guild.Id].NickChangesLogged}\n" +
                $"**User Ban Logging:** {GuildHandler.GuildConfigs[Guild.Id].UserBannedLogged}\n**Auto Respond:** {GuildHandler.GuildConfigs[Guild.Id].AutoRespond}")
                .WithColor(new Color(66, 244, 232));
            await ReplyAsync("", embed: embed);
        }

        [Command("Joins"), Summary("Normal Command"), Remarks("Toggles Join logging")]
        public async Task LogJoinsAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildHandler.GuildConfigs[Guild.Id].JoinLogs)
            {
                Log.EnableJoinLogging();
                gldConfig.JoinLogs = true;
                await ReplyAsync(":gear:   Now logging joins.");
            }
            else
            {
                Log.DisableJoinLogging();
                gldConfig.JoinLogs = false;
                await ReplyAsync(":skull_crossbones:   No longer logging joins.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Leaves"), Summary("Normal Command"), Remarks("Toggle Leaves logging")]
        public async Task LogLeavesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildHandler.GuildConfigs[Guild.Id].LeaveLogs)
            {
                gldConfig.LeaveLogs = true;
                Log.EnableLeaveLogging();
                await ReplyAsync(":gear:   Now logging leaves.");
            }
            else
            {
                gldConfig.LeaveLogs = false;
                Log.DisableLeaveLogging();
                await ReplyAsync(":skull_crossbones:  No longer logging leaves.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Name"), Summary("Normal Command"), Remarks("Toggles Name change logging")]
        public async Task LogNameChangesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildHandler.GuildConfigs[Guild.Id].NameChangesLogged)
            {
                gldConfig.NameChangesLogged = true;
                Log.EnableNameChangeLogging();
                await ReplyAsync(":gear:   Now logging username changes.");
            }
            else
            {
                gldConfig.NameChangesLogged = false;
                Log.DisableNameChangeLogging();
                await ReplyAsync(":skull_crossbones:  No longer logging username changes.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Nick"), Summary("Normal Command"), Remarks("Toggles Nickname changes loggig")]
        public async Task LogNickChangesAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildHandler.GuildConfigs[Guild.Id].NickChangesLogged)
            {
                gldConfig.NickChangesLogged = true;
                Log.EnableNickChangeLogging();
                await ReplyAsync(":gear:   Now logging nickname changes.");
            }
            else
            {
                gldConfig.NickChangesLogged = false;
                Log.DisableNickChangeLogging();
                await ReplyAsync(":skull_crossbones:   No longer logging nickname changes.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Bans"), Summary("Normal Command"), Remarks("Toggles ban logging")]
        public async Task BanLogAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            if (!GuildHandler.GuildConfigs[Guild.Id].UserBannedLogged)
            {
                gldConfig.UserBannedLogged = true;
                await ReplyAsync(":gear:   Now logging bans.");
            }
            else
            {
                gldConfig.UserBannedLogged = false;
                await ReplyAsync(":skull_crossbones:  No longer logging bans.");
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Channel"), Summary("Channel Add #ChannelName/Channel AddId #ChannelName"), Remarks("Adds/Removes channel names/ids from the list")]
        public async Task ChannelAsync(GlobalEnums Prop, ITextChannel channel)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalEnums.Add:
                    gldConfig.RequiredChannelNames.Add(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalEnums.Remove:
                    gldConfig.RequiredChannelNames.Remove(channel.Name);
                    await ReplyAsync($"Channel **{channel.Name}** has been removed from RequiredChannel Attribute.");
                    break;

                case GlobalEnums.AddId:
                    gldConfig.RequiredRoleIDs.Add(channel.Id);
                    await ReplyAsync($"Channel **{channel.Id}** has been added to RequiredChannel Attribute.");
                    break;

                case GlobalEnums.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(channel.Id);                    
                    await ReplyAsync($"Channel **{channel.Id}** has been removed from RequiredChannel Attribute.");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Role"), Summary("Role AddId RoleName"), Remarks("Adds/Removes role ids from the list")]
        public async Task RoleAsync(GlobalEnums Prop, IRole Role)
        {
            var Guild = Context.Guild as SocketGuild;
            var gldConfig = GuildHandler.GuildConfigs[Guild.Id];
            switch (Prop)
            {
                case GlobalEnums.AddId:
                    gldConfig.RequiredRoleIDs.Add(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been added to RequiredRoleIDs Attribute");
                    break;

                case GlobalEnums.RemoveId:
                    gldConfig.RequiredRoleIDs.Remove(Role.Id);
                    await ReplyAsync($"Role **{Role.Id}** has been removed to RequiredRoleIDs Attribute");
                    break;
            }
            GuildHandler.GuildConfigs[Context.Guild.Id] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }
    }
}
