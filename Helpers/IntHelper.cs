using System;

namespace Valerie.Helpers
{
    public class IntHelper
    {
        static Random Random { get; set; }
        public IntHelper(Random random) => Random = random;

        public static int GiveXP => Random.Next(27) * DateTime.Now.Second /5 > 5 ? (int)Math.Sqrt(DateTime.Now.Second) : DateTime.Now.Second;

        public static int GetLevel(int XP) => 1 + (int)Math.Floor(Math.Pow(XP, 1 / 3.0));

        public static int LastLevelXP(int Level) => (Level - 1) * (Level - 1) * (Level - 1);

        public static int NextLevelXP(int Level) => (int)Math.Pow(Level, 3);
    }
}