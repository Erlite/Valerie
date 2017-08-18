using System;

namespace Valerie.Extensions
{
    public class IntExtension
    {
        public static int GiveKarma(int KarmaToGive, int TotalUsers)
        {
            if (KarmaToGive < 500)
                return KarmaToGive;
            else
                return KarmaToGive / (int)Math.Sqrt(TotalUsers);
        }

        public static int GetLevel(int Karma) => 1 + (int)Math.Pow(Karma, 1 / 4.0);

        public static int GetKarmaForNextLevel(int Level) => Level * Level * Level;
    }
}
