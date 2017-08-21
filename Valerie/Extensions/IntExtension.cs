﻿using System;

namespace Valerie.Extensions
{
    public class IntExtension
    {
        public static int GiveEridium(int EridiumToGive, int TotalUsers)
        {
            if (EridiumToGive < 500)
                return EridiumToGive;
            else
                return EridiumToGive / (int)Math.Sqrt(TotalUsers);
        }

        public static int GetLevel(int Eridium) => 1 + (int)Math.Pow(Eridium, 1 / 4.0);

        public static int GetEridiumForNextLevel(int Level) => Math.Pow(Level, 4);
    }
}
