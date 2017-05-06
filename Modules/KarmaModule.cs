﻿using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Linq;
using Rick.Handlers;
using Rick.Services;
using System.Text;
using Rick.Classes;

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
                await ReplyAsync($"Added {user.Username} to Karma List and gave 1 Karma to {user.Username}");
            }
            else
            {
                int getKarma = karmalist[user.Id];
                getKarma++;
                karmalist[user.Id] = getKarma;
                await ReplyAsync($"Gave 1 Karma to {user.Username}");
            }
            GuildHandler.GuildConfigs[user.GuildId] = gldConfig;
            await GuildHandler.SaveAsync(GuildHandler.configPath, GuildHandler.GuildConfigs);
        }

        [Command("Rank"), Summary("Karma"), Remarks("Shows how much Karma you have")]
        public async Task GetKarmaAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.Karma;
            karmalist.TryGetValue(Context.User.Id, out int karma);
            if (karma <= 0 || !karmalist.ContainsKey(Context.User.Id))
                await ReplyAsync("User doesn't exist or no Karma was found!");

            var Level = MethodService.GetLevelFromXP(karma);
            string Description = $"{Context.User.Username} has a total Karma of **{karma}** and User level is {Level}";

            var embed = EmbedService.Embed(EmbedColors.Gold, Context.User.Username, Context.User.GetAvatarUrl(), null, Description);
            await ReplyAsync("", embed: embed);
        }

        [Command("Top"), Summary("Normal Command"), Remarks("Shows users with top Karma")]
        public async Task TopAsync()
        {
            var gldConfig = GuildHandler.GuildConfigs[Context.Guild.Id];
            var karmalist = gldConfig.Karma;
            var filter = karmalist.OrderByDescending(x => x.Value).Take(11);
            StringBuilder Builder = new StringBuilder();
            foreach (var val in filter)
            {
                var user = (await Context.Guild.GetUserAsync(val.Key)) as SocketGuildUser;
                var Level = MethodService.GetLevelFromXP(val.Value);
                Builder.AppendLine($"**{user.Username}** with {val.Value} karma and current level is **{Level}**");
            }
            var embed = EmbedService.Embed(EmbedColors.Pastle, $"Top 10 Users", Context.Guild.IconUrl, null, Builder.ToString());
            await ReplyAsync("", embed: embed);
        }
    }
}
