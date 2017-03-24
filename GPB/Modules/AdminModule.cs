using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using GPB.Services;
using System.IO;
using Newtonsoft.Json;

namespace GPB.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
    {
        private LogService log;

        public AdminModule(LogService Logger)
        {
            log = Logger;
        }

        [Command("Kick")]
        public async Task KickAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var embed = new EmbedBuilder()
                .WithTitle("===== Kicked User =====")
                .WithDescription($"**Username: **{user.Username} || {user.Discriminator}\n**Responsilble Mod: **{Context.User}\n**Reason: **{reason}")
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Mention;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithColor(new Color(232, 226, 53))
                .WithFooter(x =>
                {
                    x.Text = $"Kicked by {Context.User}";
                    x.IconUrl = Context.User.GetAvatarUrl();
                });
            var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            await ModLog.SendMessageAsync("", embed: embed);
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT KICKED*** :ok_hand: ");
            await user.KickAsync();
        }

        [Command("Ban")]
        public async Task BanAsync(SocketGuildUser user = null, [Remainder] string reason = null)
        {
            if (user == null)
                throw new ArgumentException("You must mention a user!");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason");

            var gld = Context.Guild as SocketGuild;

            var embed = new EmbedBuilder();
            embed.Color = new Color(206, 47, 47);
            embed.Title = "=== Banned User ===";
            embed.Description = $"**Username: ** {user.Username} || {user.Discriminator}\n**Responsilbe Mod: ** {Context.User}\n**Reason: **{reason}";
            var ModLog = await Context.Client.GetChannelAsync(log.ModLogChannelId) as ITextChannel;
            await ModLog.SendMessageAsync("", embed: embed);
            await ReplyAsync($"***{user.Username + '#' + user.Discriminator} GOT BENT*** :hammer: ");
            await gld.AddBanAsync(user);
        }

        [Command("ServerList"), RequireOwner]
        public async Task ServerListAsync()
        {
            var cl = Context.Client as DiscordSocketClient;
            var embed = new EmbedBuilder();
            foreach (SocketGuild guild in cl.Guilds)
            {
                embed.AddField(x =>
                {
                    x.Name = $"{guild.Name} || {guild.Id}";
                    x.Value = $"Guild Owner: { guild.Owner} || { guild.OwnerId}\nGuild Members: {guild.MemberCount}";
                    x.IsInline = true;
                });
            }
            embed.Title = "=== Server List ===";
            embed.Color = new Color(244, 66, 113);
            embed.Footer = new EmbedFooterBuilder()
            {
                Text = $"Total Guilds: {cl.Guilds.Count.ToString()}",
                IconUrl = "http://tabard.gnomeregan.info/result/faction_Alliance_icon_emblem_00_border_border_00_iconcolor_ffffff_bgcolor_000000_bordercolor_ffffff.png"
            };

            await ReplyAsync("", embed: embed);
        }

        [Command("Leave"), RequireOwner]
        public async Task Leave(ulong ID, [Remainder] string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                throw new Exception("You must provide a reason!");
            var client = Context.Client;
            var gld = await client.GetGuildAsync(ID);
            var ch = await gld.GetDefaultChannelAsync();
            var embed = new EmbedBuilder();
            embed.Description = $"Hello, I've been instructed by my owner to leave this guild!\n**Reason: **{msg}";
            embed.Color = new Color(186, 24, 66);
            embed.Author = new EmbedAuthorBuilder()
            {
                Name = Context.User.Username,
                IconUrl = Context.User.GetAvatarUrl()
            };
            await ch.SendMessageAsync("", embed: embed);
            await Task.Delay(5000);
            await gld.LeaveAsync();
            await ReplyAsync("Message has been sent and I've left the guild!");
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
            embed.Color = new Color(191, 30, 60);
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Response")]
        public async Task AddResponse(string name, [Remainder]string response)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new NullReferenceException("Name can't be empty");
            if (string.IsNullOrWhiteSpace(response))
                throw new NullReferenceException("Response can't be empty");
            var resp = LogService.GetResponses();
            if (!(resp.ContainsKey(name)))
            {
                resp.Add(name, response.ToString());
                File.WriteAllText(LogService.DictPath, JsonConvert.SerializeObject(resp, Formatting.Indented));
                var embed = new EmbedBuilder()
                    .WithAuthor(x => { x.Name = "New response added!"; x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); })
                    .WithDescription($"**Response Trigger:** {name}\n**Response: **{response}");
                await ReplyAsync("", embed:embed);
            }
            else
                await ReplyAsync("I wasn't able to add the response to the response list! :x:");
        }
    }
}