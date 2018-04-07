using System;
using Discord;
using System.Net;
using System.Linq;
using Valerie.Enums;
using Valerie.Addons;
using Valerie.Models;
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
            .WithThumbnailUrl(Emotes.DHeart.Url)
            .AddField("Gateway", $"{(Context.Client as DiscordSocketClient).Latency} ms", true)
            .AddField("Network", $"{(await new Ping().SendPingAsync(IPAddress.Loopback)).RoundtripTime} ms", true).Build());

        [Command("Feedback"), Summary("Give feedback on Valerie's performance or suggest new features!")]
        public async Task FeedbackAsync()
        {
            var ReportChannel = await Context.Client.GetChannelAsync(Context.Config.ReportChannel) as IMessageChannel;
            await ReplyAsync($"*Please provide your feedback in 2-3 sentences.*");
            var Response = await ResponseWaitAsync(Timeout: TimeSpan.FromSeconds(60));
            if (Response == null || Response.Content.Length < 20)
            {
                await ReplyAndDeleteAsync($"Hmm, I can't submit a blank feedback. Try again maybe?");
                return;
            }
            await ReportChannel.SendMessageAsync(string.Empty, embed: GetEmbed(Paint.Aqua)
                .WithAuthor($"Feedback from {Context.User}", Context.User.GetAvatarUrl())
                .WithDescription($"**Feedback:**\n{Response.Content}")
                .WithFooter($"{Context.Guild} | {Context.Guild.Id}")
                .Build());
            await ReplyAsync($"Thank you for sumbitting your feedback. {Emotes.DSupporter}");
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
            Embed.AddField("Memory", $"Heap Size: {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()} MB", true);
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
                    return ReplyAsync($"You are", Document: DocumentType.Server);
                case 'r':
                    if (!Context.Server.AFK.ContainsKey(Context.User.Id)) return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFK.Remove(Context.User.Id);
                    return ReplyAsync($"", Document: DocumentType.Server);
                case 'm':
                    if (!Context.Server.AFK.ContainsKey(Context.User.Id)) return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFK[Context.User.Id] = AFKMessage;
                    return ReplyAsync($"", Document: DocumentType.Server);
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
                .AddField("Is Hoisted?", Role.IsHoisted ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Managed?", Role.IsManaged ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Mentionable?", Role.IsMentionable ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Granted Permissions", string.Join(", ", Granted))
                .AddField("Denied Permissions", string.Join(", ", Denied)).Build());
        }

        [Command("UserInfo"), Summary("Displays information about a user.")]
        public Task UserInfoAsync(SocketGuildUser User = null)
        {
            User = User ?? Context.User as SocketGuildUser;
            return ReplyAsync(string.Empty, GetEmbed(Paint.Rose)
                .WithAuthor($"{User.Username} Information | {User.Id}", User.GetAvatarUrl())
                .WithThumbnailUrl(User.GetAvatarUrl())
                .AddField("Hierarchy", User.Hierarchy, true)
                .AddField("Is Bot", User.IsBot ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Deafened?", User.IsDeafened ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Muted?", User.IsMuted ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Self Deafened?", User.IsSelfDeafened ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Self Muted?", User.IsSelfMuted ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Suppressed?", User.IsSuppressed ? Emotes.TickYes : Emotes.TickNo, true)
                .AddField("Is Webhook?", User.IsWebhook ? Emotes.TickYes : Emotes.TickNo, true).Build());
        }
    }
}