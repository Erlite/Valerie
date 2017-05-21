using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Rick.Services
{
    public class ConsoleService
    {
        public static void TitleCard(string title, string version = null, ConsoleColor? color = null)
        {
            if (color == null)
                color = ConsoleColor.Cyan;

            var card = new List<string>();
            card.Add($"┌{new string('─', 12)}{new string('─', title.Count())}{new string('─', 12)}┐");
            card.Add($"│{new string(' ', 12)}{title}{new string(' ', 12)}│");
            if (version != null)
            {
                int diff = title.Count() - version.Count() / 2;

                if (diff > 0)
                    card.Add($"│{new string(' ', 12 + diff)}{version}{new string(' ', 12 + diff)}│");
            }
            card.Add($"└{new string('─', 12)}{new string('─', title.Count())}{new string('─', 12)}┘");

            Console.Title = title;
            Console.Write(Environment.NewLine + (string.Join(Environment.NewLine, card)));
        }

        public static void Append(string text, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(text);
        }

        public static void Log(LogSeverity Severity, string source, string message)
        {
            Console.Write(Environment.NewLine);
            Append($"[{Severity}]", ConsoleColor.Cyan);
            Append($"[{source}] ", ConsoleColor.Red);
            Append(message, ConsoleColor.Yellow);
        }

        public static void Log(string source, string message)
        {
            Console.Write(Environment.NewLine);
            Append($"[{source}] ", ConsoleColor.DarkCyan);
            Append(message, ConsoleColor.DarkYellow);
        }
    }
}