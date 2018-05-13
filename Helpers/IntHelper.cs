using System;
using System.Linq;
using Valerie.Addons;

namespace Valerie.Helpers
{
    public class IntHelper
    {
        static double Percentage { get => 0.085; }
        public static int GetLevel(int XP) => (int)Math.Floor(Percentage * Math.Sqrt(XP));
        public static int NextLevelXP(int Level) => (int)Math.Pow((Level + 1) / Percentage, 2);

        public static int GetGuildRank(IContext Context, ulong UserId)
        {
            var Order = Context.Server.Profiles.OrderByDescending(x => x.Value.ChatXP);
            var User = Order.FirstOrDefault(x => x.Key == UserId);
            return Order.ToList().IndexOf(User) + 1;
        }
    }
}