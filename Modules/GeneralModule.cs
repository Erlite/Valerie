using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Attributes;
using Valerie.Handlers.ModuleHandler;
using Discord.WebSocket;
using System.Diagnostics;
using System.Linq;

namespace Valerie.Modules
{
    [Name("General & Fun Commands."), RequireBotPermission(ChannelPermission.SendMessages)]
    public class GeneralModule : ValerieBase
    {
        [Command("Ping"), Summary("Pings discord gateway.")]
        public Task PingAsync() => ReplyAsync($"{(Context.Client as DiscordSocketClient).Latency} ms.");

        [Command("Updoot"), Summary("Gives an updoot to a specified user.")]
        public Task UpdootAsync(IGuildUser User)
        {
            if (!Context.Server.Updoots.Any(x => x.Key == User.Id))
                Context.Server.Updoots.Add(User.Id, 1);
            else
                Context.Server.Updoots[User.Id]++;
            return SaveAsync($"Updooted {User}.");
        }

        [Command("Updoot"), Summary("Shows your updoots in this server.")]
        public Task UpdootAsync()
        {
            if (!Context.Server.Updoots.ContainsKey(Context.User.Id))
                return ReplyAsync("No-one updooted you. 😶");
            return ReplyAsync($"You have {Context.Server.Updoots[Context.User.Id]} updoots.");
        }

        [Command("AFK"), Summary("Adds Or Removes you from AFK list. Actions: Add/Remove/Modify")]
        public Task AFKAsync(ModuleEnums Action = ModuleEnums.Add, [Remainder] string AFKMessage = "I'm busy.")
        {
            switch (Action)
            {
                case ModuleEnums.Add:
                    if (Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are already AFK.");
                    Context.Server.AFKUsers.Add(Context.User.Id, AFKMessage);
                    return SaveAsync();
                case ModuleEnums.Remove:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers.Remove(Context.User.Id);
                    return SaveAsync();
                case ModuleEnums.Modify:
                    if (!Context.Server.AFKUsers.ContainsKey(Context.User.Id))
                        return ReplyAsync("Whoops, it seems you are not AFK.");
                    Context.Server.AFKUsers[Context.User.Id] = AFKMessage;
                    return SaveAsync();
            }
            return Task.CompletedTask;
        }

        [Command("Slotmachine"), Summary("Want to earn quick XP? That's how you earn some.")]
        public Task SlotMachineAsync(int Bet = 100)
        {
            string[] Slots = new string[] { ":heart:", ":eggplant:", ":poo:", ":eyes:", ":star2:", ":peach:", ":pizza:" };
            var UserXP = Context.Server.ChatXP.Rankings[Context.User.Id];
            if (UserXP < Bet) return ReplyAsync("You don't have enough XP for slot machine!");
            if (Bet <= 0) return ReplyAsync("Bet is too low. :-1:");

            var embed = new EmbedBuilder();
            int[] s = new int[]
            {
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length),
                Context.Random.Next(0, Slots.Length)
            };
            embed.AddField("Slot 1", Slots[s[0]], true);
            embed.AddField("Slot 2", Slots[s[1]], true);
            embed.AddField("Slot 3", Slots[s[2]], true);

            int win = 0;
            if (s[0] == s[1] & s[0] == s[2]) win = 10;
            else if (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) win = 2;

            if (win == 0)
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] -= Bet;
                embed.Description = $"You lost {Bet} XP. Your current XP is: {UserXP - Bet}. Better luck next time! :weary: ";
                embed.Color = new Color(0xff0000);
            }
            else
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] += Bet;
                embed.Description = $"You won {Bet} XP :tada: Your current XP is: {UserXP + Bet}";
                embed.Color = new Color(0x93ff89);
            }
            return SendEmbedAsync(embed.Build());
        }

        [Command("Flip"), Summary("Flips a coin! DON'T FORGOT TO BET MONEY!")]
        public Task FlipAsync(string Side, int Bet = 100)
        {
            if (int.TryParse(Side, out int res)) return ReplyAsync("Side can either be Heads Or Tails.");
            int UserXP = Context.Server.ChatXP.Rankings[Context.User.Id];
            if (UserXP < Bet || UserXP <= 0) return ReplyAsync("You don't have enough XP!");
            if (Bet <= 0) return ReplyAsync("Bet can't be lower than 0! Default bet is set to 50!");
            string[] Sides = { "Heads", "Tails" };
            var GetSide = Sides[Context.Random.Next(0, Sides.Length)];
            if (Side.ToLower() == GetSide.ToLower())
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] += Bet;
                return ReplyAsync($"Congratulations! You won {Bet} XP! You have {UserXP + Bet} XP.");
            }
            else
            {
                Context.Server.ChatXP.Rankings[Context.User.Id] -= Bet;
                return ReplyAsync($"You lost {Bet} XP! Your have {UserXP - Bet} XP left. :frowning:");
            }
        }

        [Command("GuildInfo"), Alias("GI"), Summary("Displays information about guild.")]
        public async Task GuildInfoAsync()
        {
            var embed = ValerieEmbed.Embed(EmbedColor.Pastel, Title: $"INFORMATION | {Context.Guild.Name}",
                ThumbUrl: Context.Guild.IconUrl ?? "https://png.icons8.com/discord/dusk/256");

            embed.AddField("ID", Context.Guild.Id, true);
            embed.AddField("Owner", await Context.Guild.GetOwnerAsync(), true);
            embed.AddField("Default Channel", await Context.Guild.GetDefaultChannelAsync(), true);
            embed.AddField("Voice Region", Context.Guild.VoiceRegionId, true);
            embed.AddField("Created At", Context.Guild.CreatedAt, true);
            embed.AddField("Roles", $"{Context.Guild.Roles.Count }\n{string.Join(", ", Context.Guild.Roles.OrderByDescending(x => x.Position))}", true);
            embed.AddField("Users", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == false), true);
            embed.AddField("Bots", (await Context.Guild.GetUsersAsync()).Count(x => x.IsBot == true), true);
            embed.AddField("Text Channels", (Context.Guild as SocketGuild).TextChannels.Count, true);
            embed.AddField("Voice Channels", (Context.Guild as SocketGuild).VoiceChannels.Count, true);
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
            embed.AddField("Roles", Roles, true);
            await ReplyAsync("", embed: embed.Build());
        }

        [Command("Rate"), Summary("Rates something for you out of 10.")]
        public async Task RateAsync([Remainder] string ThingToRate) 
            => await ReplyAsync($":thinking: I would rate '{ThingToRate}' a solid {Context.Random.Next(11)}/10");
    }
}