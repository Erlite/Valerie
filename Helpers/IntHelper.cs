using System;

namespace Valerie.Helpers
{
    public class IntHelper
    {
        public static int GetLevel(int Xp) => 1 + (int)Math.Pow(Xp, 1 / 7);

        public static int GetXpForNextLevel(int Level) => (int)Math.Pow(Level, 7);
    }
}