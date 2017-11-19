using System;

namespace Valerie.Extensions
{
    public class IntExt
    {
        public static int GiveXp(int Xp) => Xp / (int)Math.Sqrt(DateTime.Now.Millisecond);

        public static int GetLevel(int Xp) => 1 + (int)Math.Pow(Xp, 1 / 4.0);

        public static int GetXpForNextLevel(int Level) => (int)Math.Pow(Level, 4);
    }
}