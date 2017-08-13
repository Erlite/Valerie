using System;

namespace Valerie.Extensions
{
    public class IntExtension
    {
        public static int GiveKarma(int KarmaToGive, int TotalUsers)
        {
            return (int)Math.Ceiling(Math.Pow(KarmaToGive, 2) / TotalUsers);
        }

        public static int GetLevel(int Karma)
        {
            return 1 + (int)Math.Floor(Math.Pow(Karma, 1 / 3.0));
        }
        public static int GetKarmaForNextLevel(int Level)
        {
            return Level * Level * Level;
        }
    }
}
