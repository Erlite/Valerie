using System;
using Discord;
using System.Net;
using System.Linq;
using Valerie.Addons;
using Valerie.Models;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;
using static Valerie.Addons.Embeds;
using System.Net.NetworkInformation;

namespace Valerie.Modules
{
    [Name("General Commands"), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : Base
    {
        [Command("Ping"), Summary("Replies back with a pong?")]
        public async Task PingAsync() => await ReplyAsync(string.Empty, BuildEmbed(Paint.Lime)
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
            await ReportChannel.SendMessageAsync($"**New Feedback From {Context.Guild}**\n\n**User:** {Context.User} ({Context.User.Id})\n" +
                $"**Response:** {Response}");
            await ReplyAsync("Thank you for sumbitting your feedback. 😊");
        }

        [Command("Stats"), Summary("Displays information about Valerie and her stats.")]
        public async Task StatsAsync()
        {
            var Client = Context.Client as DiscordSocketClient;
            var Servers = Context.Session.Query<GuildModel>().Customize(x => x.WaitForNonStaleResults()).ToList();
            var Commits = await Context.MethodHelper.GetCommitsAsync();
            string Description = null;
            if (!Commits.Any()) Description = "Error fetching commits.";
            foreach (var Commit in Commits.Take(3))
                Description += $"[[{Commit.Sha.Substring(0, 6)}]({Commit.HtmlUrl})] {Commit.Commit.Message}\n";

            var Embed = BuildEmbed(Paint.Magenta)
                .WithAuthor(x =>
                {
                    x.Name = $"{Context.Client.CurrentUser.Username} Statistics 🔰";
                    x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
                })
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
            Embed.AddField("Programmer", $"[Yucked](https://github.com/Yucked)", true);
            await ReplyAsync("", embed: Embed.Build());
        }
    }
}