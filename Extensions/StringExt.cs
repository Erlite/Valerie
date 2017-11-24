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

        public static string Replace(this string Message, string Param1, string Param2)
        {
            StringBuilder Builder = new StringBuilder(Message);
            Builder.Replace("{guild}", Param1);
            Builder.Replace("{user}", Param2);
            Builder.Replace("{rank}", Param1);
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