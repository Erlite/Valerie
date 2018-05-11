using System;
using Discord;
using System.Net;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Valerie.Addons;
using Valerie.Helpers;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Valerie.Addons.Embeds;
using System.Net.NetworkInformation;

namespace Valerie.Modules
{
    [Name("General Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : Base
    {
        [Command("Ping"), Summary("Replies back with a pong?")]
        public async Task PingAsync() => await ReplyAsync(string.Empty, GetEmbed(Paint.Lime)
            .WithTitle("Beep Boop, Boop Beep!")
            .WithThumbnailUrl(Emotes.Shout.Url)
            .AddField("Gateway", $"{(Context.Client as DiscordSocketClient).Latency} ms", true)
            .AddField("Network", $"{(await new Ping().SendPingAsync(IPAddress.Loopback)).RoundtripTime} ms", true).Build());

        [Command("Feedback"), Summary("Give feedback on Valerie's performance or suggest new features!")]
        public async Task FeedbackAsync()
        {
            await ReplyAsync($"*Please provide your feedback in 2-3 sentences.*");
            var Response = await WaitForReaponseAsync(Timeout: TimeSpan.FromSeconds(60));
            if (Response == null || Response.Content.Length < 20)
            {
                await ReplyAndDeleteAsync($"Hmm, I can't submit a blank feedback. Try again maybe?");
                return;
            }
            await Context.WebhookService.SendMessageAsync(new WebhookOptions
            {
                Webhook = Context.Config.ReportWebhook,
                Embed = GetEmbed(Paint.Aqua)
                .WithAuthor($"Feedback from {Context.User}", Context.User.GetAvatarUrl())
                .WithDescription($"**Feedback:**\n{Response.Content}")
                .WithFooter($"{Context.Guild} | {Context.Guild.Id}")
                .Build(),
                Name = "User Feedback / Report"
            });
            await ReplyAsync($"Thank you for sumbitting your feedback. {Emotes.Squint}");
        }

        [Command("Stats"), Alias("About", "Info"), Summary("Displays information about Valerie and her stats.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            var Servers = Context.Session.Query<GuildModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            var Commits = await Context.MethodHelper.GetCommitsAsync();
            string Description = null;
            if (!Commits.Any()) Description = "Error fetching commits.";
            foreach (var Commit in Commits.Take(3))
                Description += $"[[{Commit.Sha.Substring(0, 6)}]({Commit.HtmlUrl})] {Commit.Commit.Message}\n";

            var Embed = GetEmbed(Paint.Magenta)
                .WithAuthor($"{Context.Client.CurrentUser.Username} Statistics 🔰", Context.Client.CurrentUser.GetAvatarUrl())
                .WithDescription((await Client.GetApplicationInfoAsync()).Description)
                .AddField("Latest Commits", Description, false)
                .AddField("Channels",
                $"Text: {Client.Guilds.Sum(x => x.TextChannels.Count)}\n" +
                $"Voice: {Client.Guilds.Sum(x => x.VoiceChannels.Count)}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Channels.Count)}", true)
                .AddField("Members",
                $"Bot: {Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == true).Count())}\n" +
                $"Human: { Client.Guilds.Sum(x => x.Users.Where(z => z.IsBot == false).Count())}\n" +
                $"Total: {Client.Guilds.Sum(x => x.Users.Count)}", true)
                .AddField("Database",
                $"Tags: {Servers.Sum(x => x.Tags.Count)}\n" +
                $"Stars: {Servers.Sum(x => x.Starboard.StarboardMessages.Sum(y => y.Stars))}\n" +
                $"Crystals: {Servers.Sum(x => x.Profiles.Sum(y => y.Value.Crystals))}", true);
            Embed.AddField("Severs", $"{Client.Guilds.Count}", true);
            Embed.AddField("Memory", $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB", true);
            Embed.AddField("Programmer", $"[{(await Context.Client.GetApplicationInfoAsync()).Owner}](https://github.com/Yucked)", true);
            await ReplyAsync("", embed: Embed.Build());
        }

        [Command("Avatar"), Summary("Shows users avatar in higher resolution.")]
        public Task UserAvatarAsync(IGuildUser User = null) => ReplyAsync((User ?? Context.User).GetAvatarUrl(size: 2048));

        [Command("AFK"), Summary("Adds Or Removes you from AFK list. Actions: Add/Remove/Modify")]
        public Task AFKAsync(char Action = 'a', [Remainder] string AFKMessage = "Hey I'm AFK. Leave a DM?")
        {
            switch (Action)
            {
                case 'a':
                    if (Context.Server.AFK.ContainsKey(Context.User.Id)) return ReplyAsync("Whoops, it seems you are already AFK.");
                    Context.Server.AFK.Add(Context.User.Id, AFKMessage);
                    return ReplyAsync("Users will be notified that you are AFK when you are mentioned.", Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.AFK.ContainsKey(Context.User.Id)) return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFK.Remove(Context.User.Id);
                    return ReplyAsync("You are no longer AFK.", Document: DocumentType.Server);
                case 'm':
                    if (!Context.Server.AFK.ContainsKey(Context.User.Id)) return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFK[Context.User.Id] = AFKMessage;
                    return ReplyAsync("Your AFK messages has been modified.", Document: DocumentType.Server);
            }
            return Task.CompletedTask;
        }

        [Command("GuildInfo"), Summary("Displays information about guild.")]
        public Task GuildInfoAsync()
        {
            var Guild = Context.Guild as SocketGuild;
            return ReplyAsync(string.Empty, GetEmbed(Paint.Lime)
                .WithAuthor($"{Context.Guild}'s Information | {Context.Guild.Id}", Context.Guild.IconUrl)
               .WithFooter($"Created On: {Guild.CreatedAt}")
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .AddField("Owner", Guild.Owner, true)
                .AddField("Default Channel", Guild.DefaultChannel.Name ?? "No Default Channel", true)
                .AddField("Message Notifications", Guild.DefaultMessageNotifications, true)
                .AddField("Verification Level", Guild.VerificationLevel, true)
                .AddField("MFA Level", Guild.MfaLevel, true)
                .AddField("Text Channels", Guild.TextChannels.Count, true)
                .AddField("Voice Channels", Guild.VoiceChannels.Count, true)
                .AddField("Bot Members", Guild.Users.Count(x => x.IsBot == true), true)
                .AddField("Human Members", Guild.Users.Count(x => x.IsBot == false), true)
                .AddField("Total Members", Guild.MemberCount, true)
                .AddField("Roles", string.Join(", ", Guild.Roles.OrderByDescending(x => x.Position).Select(x => x.Name))).Build());
        }

        [Command("RoleInfo"), Summary("Displays information about a role.")]
        public Task RoleInfoAsync(IRole Role)
        {
            var Permissions = Role.Permissions.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var Denied = new List<string>();
            var Granted = new List<string>();
            foreach (var Permission in Permissions.Where(x => x.PropertyType == typeof(bool)))
            {
                var GetValue = (bool)Permission.GetValue(Role.Permissions, null);
                if (GetValue == false) Denied.Add(Permission.Name);
                else Granted.Add(Permission.Name);
            }
            return ReplyAsync(string.Empty, GetEmbed(Paint.Rose)
                .WithTitle($"{Role.Name} Information")
                .WithFooter($"Created On: {Role.CreatedAt}")
                .AddField("ID", Role.Id, true)
                .AddField("Color", Role.Color, true)
                .AddField("Role Position", Role.Position, true)
                .AddField("Is Hoisted?", Role.IsHoisted ? Emotes.ThumbUp : Emotes.ThumbDown, true)
                .AddField("Is Managed?", Role.IsManaged ? Emotes.ThumbUp : Emotes.ThumbDown, true)
                .AddField("Is Mentionable?", Role.IsMentionable ? Emotes.ThumbUp : Emotes.ThumbDown, true)
                .AddField("Granted Permissions", Granted.Any() ? string.Join(", ", Granted) : "No permissions granted.")
                .AddField("Denied Permissions", Denied.Any() ? string.Join(", ", Denied) : "No permissions denied.").Build());
        }

        [Command("UserInfo"), Summary("Displays information about a user.")]
        public Task UserInfoAsync(SocketGuildUser User = null)
        {
            User = User ?? Context.User as SocketGuildUser;
            return ReplyAsync(string.Empty, GetEmbed(Paint.Rose)
                .WithAuthor($"{User.Username} Information | {User.Id}", User.GetAvatarUrl())
                .WithThumbnailUrl(User.GetAvatarUrl()).
                AddField("Muted?", User.IsMuted ? Emotes.ThumbUp : Emotes.ThumbDown, true)
                .AddField("Is Bot?", User.IsBot ? Emotes.ThumbUp : Emotes.ThumbDown, true)
                .AddField("Creation Date", User.CreatedAt, true)
                .AddField("Join Date", User.JoinedAt, true)
                .AddField("Status", User.Status, true)
                .AddField("Permissions", string.Join(", ", User.GuildPermissions.ToList()), true)
                .AddField("Roles", string.Join(", ", (User as SocketGuildUser).Roles.OrderBy(x => x.Position).Select(x => x.Name)), true).Build());
        }

        [Command("Discrim"), Summary("Gets all users who match a certain user's discriminator.")]
        public Task DiscrimAsync(IGuildUser User)
        {
            var MatchList = new List<string>();
            foreach (var Guilds in (Context.Client as DiscordSocketClient).Guilds)
                foreach (var user in Guilds.Users.Where(x => x.Discriminator == User.Discriminator && x.Id != User.Id))
                    if (!MatchList.Contains(user.Username)) MatchList.Add(user.Username);
            return ReplyAsync(MatchList.Any() ? $"**Users matching {User} discriminator:** {string.Join(", ", MatchList)}" :
                $"Couldn't find any users matching {User} discriminator");
        }

        [Command("Case"), Summary("Shows information about a specific case.")]
        public Task CaseAsync(int CaseNumber = 0)
        {
            if (CaseNumber == 0 && Context.Server.Mod.Cases.Any()) CaseNumber = Context.Server.Mod.Cases.LastOrDefault().CaseNumber;
            var Case = Context.Server.Mod.Cases.FirstOrDefault(x => x.CaseNumber == CaseNumber);
            if (Case == null) return ReplyAsync($"Case #{CaseNumber} doesn't exist.");
            return ReplyAsync(string.Empty, GetEmbed(Paint.Crimson)
                .AddField("User", StringHelper.CheckUser(Context.Client, Case.UserId), true)
                .AddField("Responsible Moderator", StringHelper.CheckUser(Context.Client, Case.ModId), true)
                .AddField("Case Type", Case.CaseType, true)
                .AddField("Reason", Case.Reason).Build());
        }

        [Command("Warnings"), Summary("Shows current number of warnings for specified user.")]
        public Task WarningsAsync(IGuildUser User = null)
            => ReplyAsync($"{User ?? Context.User} has {Context.GuildHelper.GetProfile(Context.Guild.Id, (User ?? Context.User).Id).Warnings} warnings.");

        [Command("Selfroles"), Summary("Shows a list of all assignable roles for current server.")]
        public Task SelfRolesAsync()
            => ReplyAsync(!Context.Server.AssignableRoles.Any() ? $"There are no self-assignable roles {Emotes.ThumbDown}" :
                $"**Current Self-Assignable Roles:**" +
                $"{string.Join("\n", $"{Emotes.Next}{ Context.Server.AssignableRoles.Select(x => StringHelper.CheckRole(Context.Guild as SocketGuild, x))}")}");

        [Command("Iam"), Summary("Adds you to the specified role. Role must be a self-assignable role.")]
        public Task IAmAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (!Context.Server.AssignableRoles.Contains(Role.Id)) return ReplyAsync($"`{Role.Name}` isn't an assignable role {Emotes.ThumbDown}");
            else if (User.Roles.Contains(Role)) return ReplyAsync($"You already have `{Role.Name}` role {Emotes.Shout}");
            return User.AddRoleAsync(Role);
        }

        [Command("IamNot"), Summary("Removes you from the specified role. Role must be a self-assignable role.")]
        public Task IAmNotAsync(IRole Role)
        {
            var User = Context.User as SocketGuildUser;
            if (User.Roles.Contains(Role)) return ReplyAsync($"You already have `{Role.Name}` role {Emotes.Shout}");
            return User.AddRoleAsync(Role);
        }

        [Command("Invite"), Summary("Valerie's invite link and support server")]
        public Task InviteAsync() => ReplyAsync($"**Invite Link:**\n" +
            $"Full Permission: https://discordapp.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=8&scope=bot\n" +
            $"Minimal Permisison: https://discordapp.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&permissions=805694647&scope=bot" +
            $"Feel free to join my server: https://discord.gg/nzYTzxD");

        [Command("Coffee"), Summary("Buy me a coffee!")]
        public Task CoffeeAsync()
            => ReplyAsync(string.Empty, GetEmbed(Paint.Yellow)
                .WithAuthor("Buy Me A Coffee", "https://www.buymeacoffee.com/assets/img/guidelines/logo-mark-2.svg")
                .WithDescription($"Hello! If you love Valerie feel free to buy me a coffee: https://www.buymeacoffee.com/Yucked")
                .WithImageUrl("https://www.buymeacoffee.com/assets/img/guidelines/bmc-coffee.gif").Build());
    }
}