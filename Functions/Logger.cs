using System;
using System.Collections.Generic;
using System.Linq;
using Rick.Enums;

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
                case LogSource.Configuration:
                    Append($"[{Source}]", ConsoleColor.DarkGreen);
                    break;
                case LogSource.ParseError:
                    Append($"[{Severity}]", ConsoleColor.DarkRed);
                    break;
                case LogSource.PreConditionError:
                    Append($"[{Severity}]", ConsoleColor.DarkRed);
                    break;
            }

            Append($" {message}", ConsoleColor.Gray);
        }

        public static void Log(string Text)
        {
            Append(Text, ConsoleColor.Yellow);
        }
    }
}
