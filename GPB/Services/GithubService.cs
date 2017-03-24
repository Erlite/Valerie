using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GPB.Handlers;

namespace GPB.Services
{
    public class GithubService
    {
        private readonly Regex IssueRegex = new Regex(@"##([0-9]+)");
        private readonly DiscordSocketClient client;
        private readonly LogService config;

        public GithubService(IDependencyMap map)
        {
            client = map.Get<DiscordSocketClient>();
            config = map.Get<LogService>();

            client.MessageReceived += ParseMessage;
        }
        public async Task ParseMessage(SocketMessage message)
        {
            if (config.ServerLogChannelId == 0) return;
            if (message.Author.Id == client.CurrentUser.Id) return;

            MatchCollection matches = IssueRegex.Matches(message.Content);
            if (matches.Count > 0)
            {
                StringBuilder outStr = new StringBuilder();
                foreach (Match match in matches)
                {
                    outStr.AppendLine($"{match.Value} - https://github.com/ExceptionDev/DiscordExampleBot/pull/{match.Value.Substring(2)}");
                }
                await message.Channel.SendMessageAsync(outStr.ToString());
            }
        }
    }
}