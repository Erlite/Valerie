using System.Text;
using Discord.WebSocket;

namespace Valerie.Helpers
{
    public class StringHelper
    {
        public static string CheckUser(DiscordSocketClient Client, ulong UserId)
        {
            var User = Client.GetUser(UserId);
            return User == null ? "Unknown User." : User.Username;
        }

        public static string CheckRole(SocketGuild Guild, ulong Id)
        {
            var Role = Guild.GetRole(Id);
            return Role == null ? "Unknown Role." : Role.Name;
        }

        public static string Star(int Stars)
        {
            if (Stars <= 5 && Stars > 0) return "⭐";
            else if (Stars > 5) return "🌟";
            else if (Stars > 15) return "💫";
            else return "✨";
        }

        public static string Replace(string Message, string Guild = null, string User = null, int Level = 0, int Crystals = 0)
        {
            StringBuilder Builder = new StringBuilder(Message);
            Builder.Replace("{guild}", Guild);
            Builder.Replace("{user}", User);
            Builder.Replace("{level}", $"{Level}");
            Builder.Replace("{crystals}", $"{Crystals}");
            return Builder.ToString();
        }
    }
}