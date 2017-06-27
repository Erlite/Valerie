using System;

namespace Rick.Functions
{
    class Fomulas
    {
        public static int GiveKarma(int karma)
        {
            return (Convert.ToInt32(Math.Pow(karma, 3)) + 50 * karma) / 5;
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
