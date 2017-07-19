using System;
using Rick.Enums;
using System.Globalization;

namespace Rick.Functions
{
    public class Logger
    {
        static void Append(string text, ConsoleColor foreground)
        {
            Console.ForegroundColor = foreground;
            Console.Write(text);
        }

        public static void Log(LogType Severity, LogSource Source, string message)
        {
            Console.Write(Environment.NewLine);

            Append($"[{DateTime.Now.ToString("hh:mm:ss.fff tt", DateTimeFormatInfo.InvariantInfo)}]", ConsoleColor.DarkGray);

            switch (Severity)
            {
                case LogType.Error:
                    Append($"[{Severity}]", ConsoleColor.Red);
                    break;

                case LogType.Info:
                    Append($"[{Severity}]", ConsoleColor.Cyan);
                    break;

                case LogType.Warning:
                    Append($"[{Severity}]", ConsoleColor.Yellow);
                    break;

            }

            switch (Source)
            {
                case LogSource.Client:
                    Append($"[{Source}]", ConsoleColor.DarkMagenta);
                    break;
                case LogSource.Config:
                    Append($"[{Source}]", ConsoleColor.DarkGreen);
                    break;
            }

            Append($" {message}", ConsoleColor.White);
        }

        public static void Log(string Text)
        {
            Append(Text, ConsoleColor.Yellow);
        }
    }
}
