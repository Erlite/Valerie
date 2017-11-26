using System;
using Valerie.JsonModels;

namespace Valerie.Extensions
{
    public class IntExt
    {
        public static int GiveXp(int Xp) => Xp / (int)Math.Sqrt(DateTime.Now.Millisecond);

        public static int GetLevel(int Xp) => 1 + (int)Math.Pow(Xp, 1 / 4.0);

        public static int GetXpForNextLevel(int Level) => (int)Math.Pow(Level, 4);

        public static (Memory, double) GetMemory(double Byte)
        {
            if (Byte / 1024 <= 1024) return (Memory.Kilobyte, Byte / 1024);
            else if (Byte / Math.Pow(1024, 2) <= Math.Pow(1024, 2)) return (Memory.Megabyte, Byte / Math.Pow(1024, 2));
            else if (Byte / Math.Pow(1024, 3) <= Math.Pow(1024, 3)) return (Memory.Gigabyte, Byte / Math.Pow(1024, 3));
            else if (Byte / Math.Pow(1024, 4) <= Math.Pow(1024, 4)) return (Memory.Terabyte, Byte / Math.Pow(1024, 4));
            else if (Byte / Math.Pow(1024, 5) <= Math.Pow(1024, 5)) return (Memory.Petabyte, Byte / Math.Pow(1024, 5));
            else if (Byte / Math.Pow(1024, 6) <= Math.Pow(1024, 6)) return (Memory.Exabyte, Byte / Math.Pow(1024, 6));
            else if (Byte / Math.Pow(1024, 7) <= Math.Pow(1024, 7)) return (Memory.Zettabyte, Byte / Math.Pow(1024, 7));
            else if (Byte / Math.Pow(1024, 8) <= Math.Pow(1024, 8)) return (Memory.Yottabyte, Byte / Math.Pow(1024, 8));
            else return (Memory.Hellabyte, Byte / Math.Pow(1024, 9));
        }
    }
}