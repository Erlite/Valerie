using System.Threading.Tasks;
using Valerie.Handlers.ModuleHandler;

namespace Valerie.Extensions
{
    public class StringExt
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
    }
}