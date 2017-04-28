using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System;
using Discord.WebSocket;
using System.Linq;
using Rick.Handlers;

namespace Rick.Modules
{
    public class KarmaModule : ModuleBase
    {
        [Command("Karma"), Summary("Karma @Username"), Remarks("Gives another user karma")]
        public async Task KarmaAsync(IGuildUser user)
        {
            if (user.Id == Context.Client.CurrentUser.Id || user.Id == Context.User.Id) return;

            var gldConfig = GuildHandler.GuildConfigs[user.GuildId];
            var karmalist = gldConfig.Karma;
            if (!karmalist.ContainsKey(user.Id))
            {
                karmalist.Add(user.Id, 1);
                GuildHandler.GuildConfigs[user.GuildId] = gldConfig;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                await ReplyAsync($"Added {user.Username} to Karma List and gave 1 Karma to {user.Username}");
            }
            else
            {
                int getKarma = karmalist[user.Id];
                getKarma++;
                karmalist[user.Id] = getKarma;

                GuildHandler.GuildConfigs[user.GuildId] = gldConfig;
                await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
                await ReplyAsync($"Gave 1 Karma to {user.Username}");
            }
        }

        [Command("Karma"), Summary("Karma"), Remarks("Shows how much Karma you have")]
        public async Task GetKarmaAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.Karma;
            karmalist.TryGetValue(Context.User.Id, out int karma);
            if (karma <= 0)
                karma = 0;
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x.Name = Context.User.Username;
                    x.IconUrl = Context.User.GetAvatarUrl();
                })
                .WithDescription($"{Context.User.Username} has a total Karma of **{karma}**");
            await ReplyAsync("", embed: embed);
        }

        [Command("Top"), Summary("Normal Command"), Remarks("Shows users with top Karma")]
        public async Task TopAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.Karma;
            var filter = karmalist.OrderByDescending(x => x.Value).Take(11);

            await ReplyAsync(String.Join("\n", filter.Select(async x => $"{(await Context.Guild.GetUserAsync(x.Key) as SocketGuildUser).Username} with {x.Value} karma")));
            //foreach(var val in filter)
            //{
            //    await ReplyAsync($"{await Context.Guild.GetUserAsync(val.Key)} with {val.Value} karma");
            //}
        }
    }
}
