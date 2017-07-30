using System;

namespace Rick.Extensions
{
    public class IntExtension
    {
        public static int GiveKarma(int Karma)
        {
            return (int)Math.Round(Math.Log(Karma));
        }

        public static int GetLevel(int Karma)
        {
            return 1 + (int)Math.Floor(Math.Pow(Karma, 1 / 3.0));
        }

        public static int GetKarmaForLastLevel(int Level)
        {
            return (Level - 1) * (Level - 1) * (Level - 1);
        }

        public static int GetKarmaForNextLevel(int Level)
        {
            return Level * Level * Level;
        }
    }
}
