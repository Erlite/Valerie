using System.Text;
using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Extensions
{
    public static class StringExt
    {
        public static async Task<string> CheckUserAsync(IContext Context, ulong Id)
        {
            var User = await Context.Guild.GetUserAsync(Id);
            return User == null ? "Unknown User." : User.Username;
        }

        public static string CheckRole(IContext Context, ulong Id)
        {
            var Role = Context.Guild.GetRole(Id);
            return Role == null ? "Unknown Role." : Role.Name;
        }

        public static string Replace(this string Message, string Guild = null, string User = null, int Level = 0, double Bytes = 0)
        {
            StringBuilder Builder = new StringBuilder(Message);
            Builder.Replace("{guild}", Guild);
            Builder.Replace("{user}", User);
            Builder.Replace("{level}", $"{Level}");
            Builder.Replace("{bytes}", $"{Bytes}");
            return Builder.ToString();
        }

        public static string StarType(int Stars)
        {
            if (Stars <= 5 && Stars > 0) return "⭐";
            else if (Stars > 5) return "🌟";
            else if (Stars > 15) return "💫";
            else return "✨";
        }
    }
}