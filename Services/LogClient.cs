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
                @"      (`-.     ('-.                 ('-.  _  .-')             ('-.   ",
                @"    _(OO  )_  ( OO ).-.           _(  OO)( \( -O )          _(  OO)  ",
                @",--(_/   ,. \ / . --. / ,--.     (,------.,------.  ,-.-') (,------. ",
                @"\   \   /(__/ | \-.  \  |  |.-')  |  .---'|   /`. ' |  |OO) |  .---' ",
                @" \   \ /   /.-'-'  |  | |  | OO ) |  |    |  /  | | |  |  \ |  |     ",
                @"  \   '   /, \| |_.'  | |  |`-' |(|  '--. |  |_.' | |  |(_/(|  '--.  ",
                @"   \     /__) |  .-.  |(|  '---.' |  .--' |  .  '.',|  |_.' |  .--'  ",
                @"    \   /     |  | |  | |      |  |  `---.|  |\  \(_|  |    |  `---. ",
                @"     `-'      `--' `--' `------'  `------'`--' '--' `--'    `------' ",
                @""
            };

            foreach (var Line in Header)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(Line);
            }
            Append("-> Application Information\n", ConsoleColor.Magenta);
            Append($"   Author: Yucked\n   Github: https://github.com/Yucked/Valerie\n   Discord.Net Version: {Discord.DiscordConfig.Version}", ConsoleColor.Gray);
        }        
    }

    public enum Source
    {
        CONFIG,
        SERVER,
        DISCORD
    }
}