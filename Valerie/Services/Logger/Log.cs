using System;
using Valerie.Services.Logger.Enums;

namespace Valerie.Services.Logger
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
            Append($" {Text}", ConsoleColor.Gray);
        }

        static void PrintArt(string Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(Text);
        }

        public static void PrintInfo()
        {
            var Art = new[]
            {
                @"____   ____      .__               .__        ",
                @"\   \ /   /____  |  |   ___________|__| ____   ",
                @" \   Y   /\__  \ |  | _/ __ \_  __ \  |/ __ \ ",
                @"  \     /  / __ \|  |_\  ___/|  | \/  \  ___/ ",
                @"   \___/  (____  /____/\___  >__|  |__|\___  >",
                @"               \/          \/              \/ "
            };
            foreach (string line in Art)
                PrintArt(line, ConsoleColor.Magenta);
            Append("+--------------------------------------------------------------+\n", ConsoleColor.Gray);
            Append("   Source Code(Github Repo): https://Github.com/Yucked/Valerie", ConsoleColor.Yellow);
            Append("\n         Build with love by Yucked | Powered by RavenDB", ConsoleColor.Red);
            Append("\n+--------------------------------------------------------------+", ConsoleColor.Gray);
        }
    }
}
