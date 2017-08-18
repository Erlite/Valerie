using System;

namespace Valerie.Extensions
{
    public class IntExtension
    {
        public static int GiveKarma(int KarmaToGive, int TotalUsers) => (int)Math.Ceiling(Math.Pow(KarmaToGive, 3) / Math.Pow(TotalUsers, 2));

        public static int GetLevel(int Karma) => 1 + (int)Math.Floor(Math.Pow(Karma, 1 / 4.0));

        public static int GetKarmaForNextLevel(int Level) => Level * Level * Level;
    }
}
