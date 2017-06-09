using System;
using System.Collections.Generic;
using System.Linq;
using Rick.Enums;
using System.Drawing;
using Console = Colorful.Console;

namespace Rick.Services
{
    public class Logger
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

        static void Append(string text, Color foreground)
        {
            Console.ForegroundColor = foreground;
            Console.Write(text);
        }

        public static void Log(LogType Severity, LogSource Source, string message)
        {
            Console.Write(Environment.NewLine);

            switch (Severity)
            {
                case LogType.Critical:
                    Append($"[{Severity}]", Color.Red);
                    break;

                case LogType.Error:
                    Append($"[{Severity}]", Color.Maroon);
                    break;

                case LogType.Execute:
                    Append($"[{Severity}]", Color.OrangeRed);
                    break;

                case LogType.Info:
                    Append($"[{Severity}]", Color.Coral);
                    break;

                case LogType.Received:
                    Append($"[{Severity}]", Color.ForestGreen);
                    break;

                case LogType.Warning:
                    Append($"[{Severity}]", Color.Yellow);
                    break;

            }

            switch (Source)
            {
                case LogSource.Client:
                    Append($"[{Source}]", Color.Firebrick);
                    break;
                case LogSource.CommandExecution:
                    Append($"[{Source}]", Color.MediumOrchid);
                    break;
                case LogSource.Configuration:
                    Append($"[{Source}]", Color.Turquoise);
                    break;
                case LogSource.ExecutionError:
                    Append($"[{Severity}]", Color.HotPink);
                    break;
                case LogSource.ParseError:
                    Append($"[{Severity}]", Color.Ivory);
                    break;
                case LogSource.PreConditionError:
                    Append($"[{Severity}]", Color.IndianRed);
                    break;
            }

            Append($" {message}", Color.DimGray);
        }
    }
}