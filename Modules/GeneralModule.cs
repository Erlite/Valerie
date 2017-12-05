using Discord;
using System;
using System.Linq;
using Newtonsoft.Json;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.JsonModels;
using Discord.WebSocket;
using Valerie.Modules.Addons;
using System.Threading.Tasks;
using System.Collections.Generic;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Modules
{
    [Name("General Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : ValerieBase
    {
        [Command("Ping"), Summary("Pings discord gateway.")]
        public Task PingAsync() => ReplyAsync($"{(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Invite"), Summary("Gives an invite link for Valerie.")]
        public Task InviteAsync() => ReplyAsync(
            $"Here is my invite link: https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=2146958591\n" +
                $"Feel free to join my server: https://discord.gg/nzYTzxD");

        [Command("About"), Summary("Shows information about Valerie.")]
        public Task AboutAsync() => ReplyAsync($"Hello! I'm Valerie written by Yucked#1195.");

        [Command("Avatar"), Summary("Shows users avatar in higher resolution.")]
        public Task UserAvatarAsync(IGuildUser User = null) => ReplyAsync((User ?? Context.User).GetAvatarUrl(size: 2048));

        [Command("AFK"), Summary("Adds Or Removes you from AFK list. Actions: Add/Remove/Modify")]
        public Task AFKAsync(ModuleEnums Action = ModuleEnums.Add, [Remainder] string AFKMessage = "I'm busy.")
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are already AFK.");
                    Context.Server.AFKUsers.Add(Context.User.Id, AFKMessage);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers.Remove(Context.User.Id);
                    return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Modify:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers[Context.User.Id] = AFKMessage;
                    return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        [Command("GuildInfo"), Alias("GI", "Serverinfo"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = ValerieEmbed.Embed(EmbedColor.Random, Title: $"INFORMATION | {Context.Guild.Name}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");

            embed.AddField("ID", Context.Guild.Id, true);
            embed.AddField("Owner", await Context.Guild.GetOwnerAsync(), true);
            embed.AddField("Default Channel", await Context.Guild.GetDefaultChannelAsync(), true);
            embed.AddField("Voice Region", Context.Guild.VoiceRegionId, true);
            embed.AddField("Created At", Context.Guild.CreatedAt, true);
            embed.AddField("Users", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false), true);
            embed.AddField("Bots", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true), true);
            embed.AddField("Text Channels", (Context.Guild as SocketGuild).TextChannels.Count, true);
            embed.AddField("Voice Channels", (Context.Guild as SocketGuild).VoiceChannels.Count, true);
            embed.AddField($"Roles - {Context.Guild.Roles.Count}", string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position)), true);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("RoleInfo"), Alias("RI"), Summary("Displays information about a role.")]
        public async Task RoleInfoAsync(IRole Role)
        {
            var embed = new EmbedBuilder
            {
                Color = Role.Color,
                Title = $"INFORMATION | {Role.Name}"
            };
            embed.AddField("ID", Role.Id, true);
            embed.AddField("Color", Role.Color, true);
            embed.AddField("Creation Date", Role.CreatedAt, true);
            embed.AddField("Is Hoisted?", Role.IsHoisted ? "Yes" : "No", true);
            embed.AddField("Is Mentionable?", Role.IsMentionable ? "Yes" : "No", true);
            embed.AddField("Is Managed?", Role.IsManaged ? "Yes" : "No", true);
            embed.AddField("Permissions", string.Join(", ", Role.Permissions.ToList()), true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("UserInfo"), Alias("UI"), Summary("Displays information about a user.")]
        public async Task UserInfoAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: $"INFORMATION | {User}", ThumbUrl: User.GetAvatarUrl());
            string Roles = null;
            foreach (var Role in (User as SocketGuildUser).Roles.OrderBy(x => x.Position)) Roles = string.Join(", ", Role.Name);
            embed.AddField("Username", User.Username, true);
            embed.AddField("ID", User.Id, true);
            embed.AddField("Muted?", User.IsMuted ? "Yes" : "No", true);
            embed.AddField("Is Bot?", User.IsBot ? "Yes" : "No", true);
            embed.AddField("Creation Date", User.CreatedAt, true);
            embed.AddField("Join Date", User.JoinedAt, true);
            embed.AddField("Status", User.Status, true);
            embed.AddField("Permissions", string.Join(", ", User.GuildPermissions.ToList()), true);
            embed.AddField("Roles", string.Join(", ", (User as SocketGuildUser).Roles.OrderBy(x => x.Position).Select(x => x.Name)), true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Iam"), Summary("Adds you to one of the roles from assignable roles list.")]
        public Task IAmAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role == Context.Guild.EveryoneRole) return Task.CompletedTask;
            if (!Context.Server.AssignableRoles.Contains(Role.Id))
                return ReplyAsync($"{Role.Name} isn't an assignable role.");
            else if (User.Roles.Contains(Role))
                return ReplyAsync($"You already have **{Role.Name}** role.");
            return User.AddRoleAsync(Role);
        }

        [Command("IamNot"), Summary("Removes you from the specified role.")]
        public Task IAmNotAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (Role == Context.Guild.EveryoneRole) return Task.CompletedTask;
            if (User.Roles.Contains(Role)) return ReplyAsync($"You already have **{Role.Name}** role.");
            return User.AddRoleAsync(Role);
        }

        [Command("Discrim"), Summary("Gets all users who match a certain user's discriminator.")]
        public async Task DiscrimAsync(IGuildUser User)
        {
            var MatchList = new Dictionary<ulong, string>();
            foreach (var Guilds in (Context.Client as DiscordSocketClient).Guilds)
            {
                var Get = Guilds.Users.Where(x => x.Discriminator == User.Discriminator && x.Id != User.Id);
                foreach (var user in Get)
                    if (!MatchList.ContainsKey(User.Id))
                        MatchList.Add(User.Id, User.Username);
            }
            string Msg = !MatchList.Any() ? $"Couldn't find any user's matching {User} discriminator.!"
                : $"**Users Matching {User}'s Discriminator:** {string.Join(", ", MatchList.Values)}";
            await ReplyAsync(Msg);
        }

        [Command("Feedback"), Summary("Give Feedback on Valerie's Performance.")]
        public async Task FeedbackAsync()
        {
            var ReportChannel = await Context.Client.GetChannelAsync(Convert.ToUInt64(Context.Config.ReportChannel)) as ITextChannel;
            await ReplyAsync($"*Please provide your response in 2-3 sentences.*");
            var Response = await ResponseWaitAsync(Timeout: TimeSpan.FromSeconds(30));
            if (Response == null)
            {
                await ReplyAndDeleteAsync($"Hmm, I can't submit a blank feedback. Try again maybe?");
                return;
            }
            await ReportChannel.SendMessageAsync($"**New Feedback From {Context.Guild}**\n\n**User:** {Context.User} ({Context.User.Id})\n" +
                $"**Response:** {Response}");
            await ReplyAsync("Thank you for sumbitting your feedback. 😊");
        }

        [Command("Show Warnings"), Alias("Sw"), Summary("Shows current number of warnings for specified user.")]
        public Task WarningsAsync(IGuildUser User = null)
        {
            User = Context.User as IGuildUser ?? User;
            if (!Context.Server.ModLog.Warnings.ContainsKey(User.Id) || !Context.Server.ModLog.Warnings.Any())
                return ReplyAsync($"{User} has no previous warnings.");
            return ReplyAsync($"{User} has been warned {Context.Server.ModLog.Warnings[User.Id]} times.");
        }

        [Command("Show Selfroles"), Alias("Ssr"), Summary("Shows a list of all assignable roles for this server.")]
        public Task ShowSelfRolesAsync()
        {
            if (!Context.Server.AssignableRoles.Any()) return ReplyAsync($"{Context.Guild} has no self roles.");
            return ReplyAsync($"**Self Assignable Roles**\n{string.Join(", ", Context.Server.AssignableRoles.Select(x => StringExt.CheckRole(Context, x)))}");
        }

        [Command("Case"), Summary("Shows information about a specific case.")]
        public Task CaseAsync(int CaseNumber = 0)
        {
            if (CaseNumber == 0 && Context.Server.ModLog.Cases.Any())
                CaseNumber = Context.Server.ModLog.Cases.LastOrDefault().CaseNumber;
            var Case = Context.Server.ModLog.Cases.FirstOrDefault(x => x.CaseNumber == CaseNumber);
            if (Case == null) return ReplyAsync($"Case #{CaseNumber} doesn't exist.");
            return ReplyAsync(
                $"**Case Number:** {Case.CaseNumber} | **Case Type:** {Case.CaseType}\n" +
                $"**User:** {Case.UserInfo}\n" +
                $"**Responsible Mod:** {Case.ResponsibleMod}\n" +
                $"**Reason:** {Case.Reason}");
        }

        [Command("Stats"), Summary("Shows certain Valerie's stats.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            var Servers = Context.Session.Query<ServerModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            var Commits = await GitStatsAsync();
            string Description = null;
            if (!Commits.Any()) Description = "Error fetching commits.";
            foreach (var Commit in Commits.Take(3))
                Description += $"[[{Commit.Sha.Substring(0, 6)}]({Commit.HtmlUrl})] {Commit.Commit.Message}\n";

            var Embed = ValerieEmbed.Embed(EmbedColor.Red, Context.Client.CurrentUser.GetAvatarUrl(), $"{Context.Client.CurrentUser.Username} Statistics 🔰",
                Description: Description);
            Embed.AddField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}", true);
            Embed.AddField("Members",
                $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Users.Count)}", true);
            Embed.AddField("Database",
                $"Tags: {Servers.Sum(x => x.Tags.Count)}\n" +
                $"Stars: {Servers.Sum(x => x.Starboard.StarboardMessages.Sum(y => y.Stars))}\n" +
                $"Bytes: {Servers.Sum(x => x.Memory.Sum(y => y.Byte))}", true);
            Embed.AddField("Severs", $"{Client.Guilds.Count}", true);
            Embed.AddField("Memory", $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB", true);
            Embed.AddField("Programmer", $"[Yucked](https://github.com/Yucked)", true);
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Updates"), Summary("Shows the most recent changes made to Valerie. FYI: Some changes may not make sense.")]
        public async Task ShowUpdatesAsync()
        {
            var Commits = await GitStatsAsync();
            var Embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: "Latest Valerie Updates");
            foreach (var Commit in Commits.Take(10))
                Embed.AddField(Commit.Sha.Substring(0, 6), $"{Commit.Commit.Message}\n{Commit.HtmlUrl}");
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Streamer"), Summary("Love to notify people when you stream? Just Add yourself here. Action: Add, Remove")]
        public Task StreamerAsync(ModuleEnums Action = ModuleEnums.Add)
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.Streamers.ContainsKey(Context.User.Id)) return ReplyAsync("No need to add yourself again.");
                    Context.Server.Streamers.Add(Context.User.Id, 0); return SaveAsync(ModuleEnums.Server);
                case ModuleEnums.Remove:
                    if (!Context.Server.Streamers.ContainsKey(Context.User.Id)) return ReplyAsync("Seems as if you never added yourself.");
                    Context.Server.Streamers.Remove(Context.User.Id); return SaveAsync(ModuleEnums.Server);
            }
            return Task.CompletedTask;
        }

        async Task<IReadOnlyCollection<GitModel>> GitStatsAsync()
        {
            var httpClient = Context.HttpClient;
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            var Request = await httpClient.GetAsync("https://api.github.com/repos/Yucked/Valerie/commits");
            var Content = JsonConvert.DeserializeObject<IReadOnlyCollection<GitModel>>(await Request.Content.ReadAsStringAsync());
            httpClient.DefaultRequestHeaders.Clear();
            return Content;
        }
    }
}