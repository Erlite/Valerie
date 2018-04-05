using System;
using System.IO;
using System.Threading.Tasks;

namespace Valerie.Services
{
    public class LogService
    {
        public void Initialize()
        {
            var LogPath = Path.Combine(Directory.GetCurrentDirectory(), "log.txt");
            if (!File.Exists(LogPath)) File.Create(LogPath);
        }

        static Task LogAsync(string Message)
            => File.AppendAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"), Message + Environment.NewLine);

        static void Append(string Text, ConsoleColor Color)
        {
            Console.ForegroundColor = Color;
            Console.Write(Text);
        }

        public static void Write(string Source, string Text, ConsoleColor? Color = null)
        {
            Console.Write(Environment.NewLine);
            Append($"{DateTime.Now.ToShortTimeString()} ", ConsoleColor.Gray);
            Append($"[{Source}]", Color ?? ConsoleColor.White);
            Append($" {Text}", ConsoleColor.White);
            _ = LogAsync($"[{DateTime.Now}] [{Source}] {Text}");
        }

        public static void PrintApplicationInformation()
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
}