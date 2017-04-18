using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace Rick.Services
{
    public class ConsoleService
    {
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

        public static void NewLine(string text = "", ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (foreground == null)
                foreground = ConsoleColor.White;
            if (background == null)
                background = ConsoleColor.Black;

            Console.ForegroundColor = (ConsoleColor)foreground;
            Console.BackgroundColor = (ConsoleColor)background;
            Console.Write(Environment.NewLine + text);
        }

        public static void Log(LogSeverity severity, string source, string message)
        {
            NewLine($"[{DateTime.Now.ToString("hh:mm")}]", ConsoleColor.Gray);
            Append($"[{severity}]", ConsoleColor.DarkRed);
            Append($"[{source}] ", ConsoleColor.DarkYellow);
            Append(message, ConsoleColor.DarkCyan);
        }

        public static void Log(IUserMessage msg)
        {
            var channel = (msg.Channel as IGuildChannel);
            NewLine($"{DateTime.Now.ToString("hh:mm:ss")} ", ConsoleColor.Gray);

            if (channel?.Guild == null)
                Append($"PM ", ConsoleColor.DarkRed);
            else
                Append($"{channel.Guild.Name} #{channel.Name} => ", ConsoleColor.DarkRed);

            Append($"{msg.Author}: ", ConsoleColor.Green);
            Append(msg.Content, ConsoleColor.White);
        }

        public static void ResetColors()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
        }
    }
}