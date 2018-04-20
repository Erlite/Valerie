using System;
using System.Linq;
using Valerie.Addons;

namespace Valerie.Helpers
{
    public class IntHelper
    {
        public static int GetLevel(int Xp) => 1 + (int)Math.Pow(Xp, 1 / 7.0);
        public static int NextLevelXp(int Level) => (int)Math.Pow(Level, 4);

        public static int GetGuildRank(IContext Context, ulong UserId)
        {
            var Get = Context.Server.Profiles.OrderByDescending(x => x.Value.ChatXP).FirstOrDefault(x => x.Key == UserId);
            return Context.Server.Profiles.ToList().IndexOf(Get);
        }
    }
}