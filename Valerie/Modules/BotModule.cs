﻿using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Extensions;
using Valerie.Handlers.ConfigHandler;
using Valerie.Handlers.ConfigHandler.Enum;
using Valerie.Modules.Enums;

namespace Valerie.Modules
{
    [Group("Bot"), RequireOwner, RequireBotPermission(ChannelPermission.SendMessages)]
    public class BotModule : CommandBase
    {
        [Command("Prefix"), Summary("Changes bot's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            if (NewPrefix == null) { await ReplyAsync("New prefix can't be null."); return; }
            await BotDB.UpdateConfigAsync(ConfigValue.Prefix, NewPrefix);
            await ReplyAsync($"Bot's Prefix has been set to: {NewPrefix}");
        }

        [Command("Avatar", RunMode = RunMode.Async), Summary("Changes Bot's avatar.")]
        public async Task AvatarAsync([Remainder] string Path)
        {
            using (var stream = new FileStream(Path, FileMode.Open))
            {
                await Context.Client.CurrentUser.ModifyAsync(x
                    => x.Avatar = new Image(stream));
                stream.Dispose();
            }
            await ReplyAsync("Avatar has been updated.");
        }

        [Command("Game"), Summary("Adds a game to bot's game list and sets it as current bot's game.")]
        public async Task GameAsync(Actions Action, [Remainder] string GameName)
        {
            switch (Action)
            {
                case Actions.Add:
                    if (BotDB.Config.Games.Contains(GameName))
                    {
                        await ReplyAsync("Game already exists in Games list.");
                        return;
                    }
                    await BotDB.UpdateConfigAsync(ConfigValue.GamesAdd, GameName);
                    await ReplyAsync("Game has been added to games list.");
                    break;
                case Actions.Remove:
                    if (!BotDB.Config.Games.Contains(GameName))
                    {
                        await ReplyAsync("Game doesn't exist in Games list.");
                        return;
                    }
                    await BotDB.UpdateConfigAsync(ConfigValue.GamesRemove, GameName);
                    await ReplyAsync("Game has been removed from games list.");
                    break;
            }
        }

        [Command("Username"), Summary("Changes Bot's username.")]
        public async Task UsernameAsync([Remainder] string Username)
        {
            await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Username);
            await ReplyAsync("Username has been updated.");
        }

        [Command("Nickname"), Summary("Changes Bot's nickname")]
        public async Task NicknameAsync([Remainder] string Nickname)
        {
            await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = Nickname);
            await ReplyAsync("Nickname has been updated.");
        }

        [Command("JoinWelcome"), Summary("Custom Guild Join Message")]
        public async Task GuildJoinMessageAsync([Remainder] string JoinMessage)
        {
            await BotDB.UpdateConfigAsync(ConfigValue.GuildWelcome, JoinMessage);
            await ReplyAsync("Guild Join message has been updated.");
        }

        [Command("ReportChannel"), Summary("Sets report channel.")]
        public async Task ReportChannelAsync(ITextChannel Channel)
        {
            await BotDB.UpdateConfigAsync(ConfigValue.ReportChannel, Channel.Id.ToString());
            await ReplyAsync($"Report channel has been set to: {Channel.Name}");
        }
    }
}
