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
            Append($"{Text}", ConsoleColor.Gray);
        }

        static void PrintArt(string Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.WriteLine(Text);
        }

        public static void PrintInfo()
        {
            Console.WriteLine(Environment.NewLine + "+-----------------------------------------------------------+");
            var Art = new[]
            {
                @"    ▄   ██   █     ▄███▄   █▄▄▄▄ ▄█ ▄███▄   ",
                @"     █  █ █  █     █▀   ▀  █  ▄▀ ██ █▀   ▀  ",
                @"█     █ █▄▄█ █     ██▄▄    █▀▀▌  ██ ██▄▄    ",
                @" █    █ █  █ ███▄  █▄   ▄▀ █  █  ▐█ █▄   ▄▀ ",
                @"  █  █     █     ▀ ▀███▀     █    ▐ ▀███▀   ",
                @"   █▐     █                 ▀               ",
                @"   ▐     ▀                                  "
            };
            foreach (string line in Art)
                PrintArt(line, ConsoleColor.Red);

            Append(
                "+-----------------------------------------------------------+\n" +
                "|  Discord.Net 1.0  |  Yucked  |  RavenDB  |  .Net Core 2.0 |\n" +
                "+-----------------------------------------------------------+", ConsoleColor.White);
        }
    }
}
