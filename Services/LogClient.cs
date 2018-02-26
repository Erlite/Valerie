using System;

namespace Valerie.Services
{
    public class LogClient
    {
        static void Append(string Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.Write(Text);
        }

        public static void Write(Source Source, string Text)
        {
            Console.Write(Environment.NewLine);
            Append($"{DateTime.Now}\n", ConsoleColor.Gray);
            switch (Source)
            {
                case Source.CONFIG: Append($"  => [{Source}]", ConsoleColor.DarkYellow); break;
                case Source.DISCORD: Append($"  => [{Source}]", ConsoleColor.DarkCyan); break;
                case Source.SERVER: Append($"  => [{Source}]", ConsoleColor.DarkMagenta); break;
            }
            Append($" {Text}", ConsoleColor.White);
        }

        public static void AppInfo()
        {
            var Header = new[]
            {
                @"",
                @"      ██╗   ██╗ █████╗ ██╗     ███████╗██████╗ ██╗███████╗",
                @"      ██║   ██║██╔══██╗██║     ██╔════╝██╔══██╗██║██╔════╝",
                @"      ██║   ██║███████║██║     █████╗  ██████╔╝██║█████╗  ",
                @"      ╚██╗ ██╔╝██╔══██║██║     ██╔══╝  ██╔══██╗██║██╔══╝  ",
                @"       ╚████╔╝ ██║  ██║███████╗███████╗██║  ██║██║███████╗",
                @"        ╚═══╝  ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝╚══════╝",
                @""
            };

            foreach (var Line in Header)
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine(Line);
            }

            Append($"-> APPLICATION INFORMATION\n\n", ConsoleColor.Yellow);
            Append($"    -> GENERAL\n", ConsoleColor.Red);
            Append($"        Author: Yucked\n        License: GPL-3.0\n        Github Repo: Yucked/Valerie\n\n", ConsoleColor.Gray);
            Append($"    -> VERSIONS\n", ConsoleColor.Red);
            Append($"        Valerie: 18-Alpha-02-26\n        Discord: {Discord.DiscordConfig.Version}\n        RavenDB: RavenDB.Client 4.0.0-nightly-20180120-0500\n", ConsoleColor.Gray);
        }
    }

    public enum Source
    {
        CONFIG,
        SERVER,
        DISCORD
    }
}