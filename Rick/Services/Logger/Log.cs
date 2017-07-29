using System;
using Rick.Services.Logger.Enums;

namespace Rick.Services.Logger
{
    public class Log
    {
        static void Append(string Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.Write(Text);
        }

        public static void Write(Status Status, Source Source, string Text)
        {
            Console.Write(Environment.NewLine);
            switch (Status)
            {
                case Status.ERR: Append($"[{Status}]", ConsoleColor.Red); break;
                case Status.KAY: Append($"[{Status}]", ConsoleColor.Green); break;
                case Status.WRN: Append($"[{Status}]", ConsoleColor.Yellow); break;
            }
            switch (Source)
            {
                case Source.BotDatabase: Append($"[{Source}]", ConsoleColor.Cyan); break;
                case Source.Client: Append($"[{Source}]", ConsoleColor.DarkMagenta); break;
                case Source.ServerDatabase: Append($"[{Source}]", ConsoleColor.DarkCyan); break;
            }
            Append($"{Text}", ConsoleColor.Gray);
        }
    }
}
