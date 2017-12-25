using System;
using Valerie.JsonModels;

namespace Valerie.Extensions
{
    public class IntExt
    {
        public static int GetLevel(int Xp) => 1 + (int)Math.Pow(Xp, 1 / 4.0);

        public static int GetXpForNextLevel(int Level) => (int)Math.Pow(Level, 4);

        public static (Memory, float) GetMemory(float Byte)
        {
            if (Byte < 1000) return (Memory.Byte, Byte);
            else if (Byte / 512 <= 512) return (Memory.Kilobyte, Byte / 512);
            else if (Byte / Math.Pow(512, 2) <= Math.Pow(512, 2)) return (Memory.Megabyte, Byte / (float)Math.Pow(512, 2));
            else if (Byte / Math.Pow(512, 3) <= Math.Pow(512, 3)) return (Memory.Gigabyte, Byte / (float)Math.Pow(512, 3));
            else if (Byte / Math.Pow(512, 4) <= Math.Pow(512, 4)) return (Memory.Terabyte, Byte / (float)Math.Pow(512, 4));
            else if (Byte / Math.Pow(512, 5) <= Math.Pow(512, 5)) return (Memory.Petabyte, Byte / (float)Math.Pow(512, 5));
            else if (Byte / Math.Pow(512, 6) <= Math.Pow(512, 6)) return (Memory.Exabyte, Byte / (float)Math.Pow(512, 6));
            else if (Byte / Math.Pow(512, 7) <= Math.Pow(512, 7)) return (Memory.Zettabyte, Byte / (float)Math.Pow(512, 7));
            else if (Byte / Math.Pow(512, 8) <= Math.Pow(512, 8)) return (Memory.Yottabyte, Byte / (float)Math.Pow(512, 8));
            else return (Memory.Hellabyte, Byte / (float)Math.Pow(512, 9));
        }
    }
}