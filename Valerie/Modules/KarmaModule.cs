using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Handlers.GuildHandler;
using Valerie.Extensions;
using Valerie.Handlers.GuildHandler.Enum;
using Valerie.Modules.Enums;

namespace Valerie.Modules
{
    public class KarmaModule : CommandBase
    {
        [Command("Rank"), Summary("Shows your current rank and how much Karma is needed for next level.")]
        public async Task RankAsync(IGuildUser User = null)
        {
            User = User ?? Context.User as IGuildUser;
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.KarmaHandler.UsersList.ContainsKey(User.Id))
            {
                await ReplyAsync($"{User.Username} isn't ranked yet! :weary:");
                return;
            }
            var UserKarma = Config.KarmaHandler.UsersList.TryGetValue(User.Id, out int Karma);
            string Reply =
                $"**TOTAL KARMA:** {Karma} | **LEVEL:** {IntExtension.GetLevel(Karma)} | " +
                $"**KARMA:** {Karma}/{IntExtension.GetKarmaForNextLevel(IntExtension.GetLevel(Karma))}";
            await ReplyAsync(Reply);
        }

        [Command("Top"), Summary("Shows top 10 users in the Karma list.")]
        public async Task KarmaAsync()
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (Config.KarmaHandler.UsersList.Count == 0)
            {
                await ReplyAsync("There are no top users for this guild.");
                return;
            }
            var embed = Vmbed.Embed(VmbedColors.Gold, Title: $"{Context.Guild.Name.ToUpper()} | Top 10 Users");
            var Karmalist = Config.KarmaHandler.UsersList.OrderByDescending(x => x.Value).Take(10);
            foreach (var Value in Karmalist)
            {
                var User = await Context.Guild.GetUserAsync(Value.Key) as IGuildUser;
                if (User == null)
                    await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.KarmaDelete, $"{User.Id}");
                embed.AddInlineField(User.Username, $"Rank: {Value.Value} | Level: {IntExtension.GetLevel(Value.Value)}");
            }
            await ReplyAsync("", embed: embed);
        }

        [Command("KarmaBlacklist"), Summary("Adds/removes a role to/from blacklisted roles"), Alias("KB")]
        public async Task BlacklistRoleAsync(Actions Action, IRole Role)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            switch (Action)
            {
                case Actions.Add:
                    if (Config.KarmaHandler.BlacklistRoles.Contains(Role.Id.ToString()))
                    {
                        await ReplyAsync($"{Role} already exists in roles blacklist."); return;
                    }
                    await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaBLAdd, Role.Id);
                    await ReplyAsync($"{Role} has been added."); break;

                case Actions.Remove:
                    if (!Config.KarmaHandler.BlacklistRoles.Contains(Role.Id.ToString()))
                    {
                        await ReplyAsync($"{Role} doesn't exists in roles blacklist."); return;
                    }
                    await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaBLRemove, Role.Id);
                    await ReplyAsync($"{Role} has been removed."); break;
            }
        }

        [Command("LevelAdd"), Summary("Adds a level to level up list."), Alias("LA")]
        public async Task LevelAddAsync(IRole Role, int Level)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (Config.KarmaHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} already exists in level up roles."); return;
            }
            await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaRoleAdd, Role.Id, Level);
            await ReplyAsync($"{Role} has been added.");
        }

        [Command("LevelRemove"), Summary("Removes a role from level up roles"), Alias("LR")]
        public async Task KarmaLevelAsync(IRole Role)
        {
            var Config = ServerDB.GuildConfig(Context.Guild.Id);
            if (!Config.KarmaHandler.LevelUpRoles.ContainsKey(Role.Id))
            {
                await ReplyAsync($"{Role} doesn't exists in level up roles."); return;
            }
            await ServerDB.KarmaHandlerAsync(Context.Guild.Id, ModelEnum.KarmaRoleRemove, Role.Id);
            await ReplyAsync($"{Role} has been removed.");
        }

        [Command("SetLevel"), Summary("Sets Max level for auto roles")]
        public async Task SetLevelAsync(int MaxLevel)
        {
            if (MaxLevel < 10)
            {
                await ReplyAsync("Max level can't be lower than 10"); return;
            }
            await ServerDB.UpdateConfigAsync(Context.Guild.Id, ModelEnum.KarmaMaxRoleLevel, MaxLevel.ToString());
            await ReplyAsync($"Max auto assign role leve has been set to: {MaxLevel}");
        }
    }
}