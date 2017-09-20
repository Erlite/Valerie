using System;

namespace Valerie.Extensions
{
    public class IntExtension
    {
        public static int GiveEridium(int EridiumToGive, int TotalUsers)
        {
            int GetTotal = (TotalUsers > 450) ? TotalUsers : (int)Math.Sqrt(TotalUsers);
            return (EridiumToGive < 50) ? EridiumToGive : EridiumToGive / GetTotal;
        }

        public static int GetLevel(int Eridium) => 1 + (int)Math.Pow(Eridium, 1 / 7.0);

        public static int GetEridiumForNextLevel(int Level) => (int)Math.Pow(Level, 7);

        public static int ConvertToSchmeckles(int EridiumToConvert) => (int)Math.Pow(EridiumToConvert, 1 / 5);

        public static int ConvertToEridium(int SchmecklesToConvert) => (int)Math.Pow(SchmecklesToConvert, 5);
    }
}
