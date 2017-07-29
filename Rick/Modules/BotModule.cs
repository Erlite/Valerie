using System.Threading.Tasks;
using Discord.Commands;
using Rick.Handlers.ConfigHandler;
using Rick.Handlers.ConfigHandler.Enum;
using System.IO;
using Discord;

namespace Rick.Modules
{
    [Group("Bot"), RequireOwner]
    public class BotModule : ModuleBase
    {
        [Command("Prefix"), Summary("Changes bot's prefix.")]
        public async Task PrefixAsync(string NewPrefix)
        {
            if (NewPrefix == null) { await ReplyAsync("New prefix can't be null."); return; }
            await BotDB.UpdateConfigAsync(ConfigValue.Prefix, NewPrefix);
            await ReplyAsync($"Bot's Prefix has been set to: {NewPrefix}");
        }

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
        public async Task GameAsync([Remainder] string GameName)
        {
            await BotDB.UpdateConfigAsync(ConfigValue.Games, GameName);
            await ReplyAsync("Bot's game has been added to games list.");
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
    }
}
