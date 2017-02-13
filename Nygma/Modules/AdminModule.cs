using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using Nygma.Handlers;
using System.Linq;
using Nygma.Utils;

namespace Nygma.Modules
{
    public class AdminModule : ModuleBase
    {
        private IDependencyMap _map;
        private ConfigHandler config;
        public AdminModule(ConfigHandler con)
        {
            config = con;
        }
        private DiscordSocketClient client;

        [Command("Kick"), Summary("Kick @Username"), Remarks("KICK HIM FOR ONCE AND FOR ALL")]
        public async Task KickAsync(SocketGuildUser user , [Remainder] string reason)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");
            var embed = new EmbedBuilder();
            embed.Title = "User Kicked!";
            embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Kicked by: **{Context.User.Mention}!\n**Reason: **{reason}";
            embed.Color = Misc.RandColor();
            await user.KickAsync();
            var LC = await Context.Client.GetChannelAsync(config.LogChannel) as ITextChannel;
            await LC.SendMessageAsync("", false, embed);}

        [Command("Ban"), Summary("Ban @Username"), Remarks("Swing the ban hammer")]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var gld = Context.Guild as SocketGuild;
            var embed = new EmbedBuilder();
            embed.Title = $"**{user.Username}** was banned from **{user.Guild.Name}**";
            embed.Description = $"**Username: **{user.Username}\n**Guild Name: **{user.Guild.Name}\n**Banned by: **{Context.User.Mention}!\n**Reason: **{reason}";
            embed.Color = Misc.RandColor();
            await gld.AddBanAsync(user);
            var LC = await Context.Client.GetChannelAsync(config.LogChannel) as ITextChannel;
            await LC.SendMessageAsync("", false, embed);
        }

        [Command("Delete"), Summary("Delete 10"), Remarks("Deletes X amount of messages"), Alias("Del")]
        [RequireBotPermission(GuildPermission.ManageMessages), RequireContext(ContextType.Guild)]
        public async Task DeleteAsync(int range = 0)
        {
            if (range <= 0)
                throw new ArgumentException("Enter a valid amount");

            var messageList = await Context.Channel.GetMessagesAsync(range).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);
            var embed = new EmbedBuilder();
            embed.Title = "Messages Deleted";
            embed.Description = $"I've deleted {range} amount of messages.";
            embed.Color = Misc.RandColor();
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Purge"), Summary("Purge #ChannelName"), Remarks("No, not like the purge movie. It deletes 500 messages from given channel")]
        public async Task PurgeAsync(ITextChannel channel)
        {
            if (channel == null)
                throw new ArgumentException("Please mention a channel!");
            var msgs = await channel.GetMessagesAsync(99).Flatten();
            await channel.DeleteMessagesAsync(msgs);
            var embed = new EmbedBuilder();
            embed.Title = $"Purged {channel.Mention}";
            embed.Description = string.Format("Deleted {0:#,##0} message{2} from channel {1}.", msgs.Count(), channel.Mention, msgs.Count() > 1 ? "s" : "");
            embed.Color = Misc.RandColor();
            await ReplyAsync("", false, embed);
        }

        [Command("Addrole"), Summary("Addrole RoleName @Username"), Remarks("Adds role to a user"), Alias("Arole")]
        public async Task AroleAsync(IRole role, params IUser[] users)
        {
            var grp = role as SocketRole;
            if (grp == null)
                throw new ArgumentException("C'mon M8 supply a role!");

            var usrs = users.Cast<SocketGuildUser>();
            if (usrs.Count() == 0)
                throw new ArgumentException("Did you forget to mention the user???");

            foreach (var usm in usrs)
                await usm.AddRolesAsync(grp);

            var embed = new EmbedBuilder();
            embed.Title = "Info";
            embed.Description = string.Concat("The following user", usrs.Count() > 1 ? "s were" : " was", " added to role **", grp.Name, "**: ", string.Join(", ", usrs.Select(xusr => xusr.Mention)));
            embed.Color = Misc.RandColor();
            await ReplyAsync("", false, embed);
        }

        [Command("RemoveRole"), Summary("RemoveRole RoleName @Username"), Remarks("Removes role from a user"), Alias("Rrole")]
        public async Task RroleAsync(IRole role, params IUser[] users)
        {
            var grp = role as SocketRole;
            if (grp == null)
                throw new ArgumentException("C'mon M8 supply a role!");

            var usrs = users.Cast<SocketGuildUser>();
            if (usrs.Count() == 0)
                throw new ArgumentException("Did you forget to mention the user???");

            foreach (var usm in usrs)
                await usm.RemoveRolesAsync(grp);

            var embed = new EmbedBuilder();
            embed.Title = "Info";
            embed.Description = string.Concat("The following user", usrs.Count() > 1 ? "s were" : " was", " removed from role **", grp.Name, "**: ", string.Join(", ", usrs.Select(xusr => xusr.Mention)));
            embed.Color = Misc.RandColor();
            await ReplyAsync("", false, embed);
        }

        [Command("PBan")]
        [Summary("Usage: ;ban @person reason (reason is optional, will be posted to a modlog channel if it's set up, contact <@140605609694199808>)\nPermanently bans a user from the server.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task PermaBan([Summary("The user to ban")] IGuildUser user, [Summary("Reason for ban"), Remainder] string reason = "No reason found")
        {

            var embed = new EmbedBuilder();
            embed.Color = Misc.RandColor();
            var author = Context.User as IGuildUser;
            var authorsHighestRole = author.RoleIds.Select(x => Context.Guild.GetRole(x))
                                                   .OrderBy(x => x.Position)
                                                   .First();
            var usersHighestRole = user.RoleIds.Select(x => Context.Guild.GetRole(x))
                                               .OrderBy(x => x.Position)
                                               .First();

            if (usersHighestRole.Position > authorsHighestRole.Position)
            {
                embed.WithDescription(":x: You cannot ban someone above you in the role hierarchy.");
                await ReplyAsync("", embed: embed);
                return;
            }

            await Context.Guild.AddBanAsync(user, 2);
            var name = user.Nickname == null
                ? user.Username
                : $"{user.Username} (nickname: {user.Nickname})";
            var timestamp = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            var guild = Context.Guild.GetChannelAsync(config.LogChannel);
            var chan = (ITextChannel)guild.Result;

            if (Context.Guild.Id == config.LogGuild)
            {

                embed.WithDescription($"{author.Mention} permabanned {name} for `{reason}`");

               embed.WithAuthor(x =>
                {
                    x.IconUrl = author.AvatarUrl;
                    x.Name = $"RESPONSIBLE MOD";
                });
                await chan.SendMessageAsync("", embed: embed);
                await ReplyAsync(":ok_hand:");
            }
            else
            {
                embed.WithDescription($"{author.Mention} permabanned {name} for `{reason}`");
                await Context.Channel.SendMessageAsync("", embed: embed);
                await ReplyAsync(":ok_hand:");
            }
        }
    }
}