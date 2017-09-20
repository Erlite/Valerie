﻿using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Valerie.Enums;
using Valerie.Handlers;
using Valerie.Handlers.Config;

namespace Valerie.Modules
{
    [Group("Bot"), RequireOwner, RequireBotPermission(ChannelPermission.SendMessages)]
    public class BotModule : ValerieBase<ValerieContext>
    {
        [Command("Prefix"), Summary("Changes bot's prefix.")]
        public Task PrefixAsync(string NewPrefix) { BotConfig.Config.Prefix = NewPrefix; return ReplyAsync("Prefix Updates."); }

        [Command("Avatar"), Summary("Changes Bot's avatar.")]
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
                    if (BotConfig.Config.BotGames.Contains(GameName))
                    {
                        await ReplyAsync("Game already exists in Games list.");
                        return;
                    }
                    BotConfig.Config.BotGames.Add(GameName);
                    await ReplyAsync("Game has been added to games list.");
                    break;
                case Actions.Delete:
                    if (!BotConfig.Config.BotGames.Contains(GameName))
                    {
                        await ReplyAsync("Game doesn't exist in Games list.");
                        return;
                    }
                    BotConfig.Config.BotGames.Remove(GameName);
                    await ReplyAsync("Game has been removed from games list.");
                    break;
            }
        }

        [Command("Username"), Summary("Changes Bot's username.")]
        public async Task UsernameAsync([Remainder] string Username)
            => await Context.Client.CurrentUser.ModifyAsync(x => x.Username = Username);

        [Command("Nickname"), Summary("Changes Bot's nickname")]
        public async Task NicknameAsync([Remainder] string Nickname)
            => await (await Context.Guild.GetCurrentUserAsync()).ModifyAsync(x => x.Nickname = Nickname);

        [Command("ServerMessage"), Summary("Custom Guild Join Message")]
        public Task GuildJoinMessageAsync([Remainder] string ServerMessage)
        {
            BotConfig.Config.ServerMessage = ServerMessage;
            return ReplyAsync("Server message updated.");
        }

        [Command("ReportChannel"), Summary("Sets report channel.")]
        public Task ReportChannelAsync(ITextChannel Channel)
        {
            BotConfig.Config.ReportChannel = $"{Channel.Id}";
            return ReplyAsync("Report channel updated.");
        }
    }
}
