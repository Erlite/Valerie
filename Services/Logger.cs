using System;

namespace Valerie.Services
{
    public class Logger
    {
        public enum Status
        {
            ERR,
            WRN,
            KAY
        }

        public enum Source
        {
            Client,
            Server,
            Config,
            Database
        }

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
                case Source.Config: Append($"[{Source}]", ConsoleColor.Cyan); break;
                case Source.Client: Append($"[{Source}]", ConsoleColor.DarkMagenta); break;
                case Source.Server: Append($"[{Source}]", ConsoleColor.DarkCyan); break;
                case Source.Database: Append($"[{Source}]", ConsoleColor.White); break;
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
                @"",
                @" ██▒   █▓ ▄▄▄       ██▓    ▓█████  ██▀███   ██▓▓█████ ",
                @"▓██░   █▒▒████▄    ▓██▒    ▓█   ▀ ▓██ ▒ ██▒▓██▒▓█   ▀ ",
                @" ▓██  █▒░▒██  ▀█▄  ▒██░    ▒███   ▓██ ░▄█ ▒▒██▒▒███   ",
                @"  ▒██ █░░░██▄▄▄▄██ ▒██░    ▒▓█  ▄ ▒██▀▀█▄  ░██░▒▓█  ▄ ",
                @"   ▒▀█░   ▓█   ▓██▒░██████▒░▒████▒░██▓ ▒██▒░██░░▒████▒",
                @"   ░ ▐░   ▒▒   ▓▒█░░ ▒░▓  ░░░ ▒░ ░░ ▒▓ ░▒▓░░▓  ░░ ▒░ ░",
                @"   ░ ░░    ▒   ▒▒ ░░ ░ ▒  ░ ░ ░  ░  ░▒ ░ ▒░ ▒ ░ ░ ░  ░",
                @"     ░░    ░   ▒     ░ ░      ░     ░░   ░  ▒ ░   ░   ",
                @"      ░        ░  ░    ░  ░   ░  ░   ░      ░     ░  ░",
                @"     ░                                                "
            };
            foreach (string line in Art)
                PrintArt(line, ConsoleColor.Magenta);
            Append("\n           Discord bot perfect for moderation and fun\n", ConsoleColor.DarkRed);
            Append("   Source Code(Github Repo): https://Github.com/Yucked/Valerie", ConsoleColor.Yellow);
            Append("\n         Build with love by Yucked | Powered by RavenDB", ConsoleColor.DarkRed);
            Append("\n+--------------------------------------------------------------+", ConsoleColor.Gray);
        }
    }
}
