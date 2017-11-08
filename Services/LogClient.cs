using System;
using System.Net;

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
            switch (Source)
            {
                case Source.CONFIG: Append($"[{Source}]", ConsoleColor.DarkYellow); break;
                case Source.DISCORD: Append($"[{Source}]", ConsoleColor.DarkCyan); break;
                case Source.REST: Append($"[{Source}]", ConsoleColor.DarkGreen); break;
                case Source.SERVER: Append($"[{Source}]", ConsoleColor.DarkMagenta); break;
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
            Append($"   Author: Yucked\n   Github: https://github.com/Yucked/Valerie\n   Discord.Net Version: {Discord.DiscordConfig.Version}\n", ConsoleColor.Gray);
            Append("\n-> API Endpoint Status Check", ConsoleColor.Magenta);
            Check("\n   Config Endpoint: ", ApiResponse("http://localhost:51117/api/config/"));
            Check("\n   Server Endpoint: ", ApiResponse("http://localhost:51117/api/server/"));
            Console.WriteLine(Environment.NewLine);
        }

        static bool ApiResponse(string Url)
        {
            bool Result;
            try
            {
                var Endpoint = WebRequest.Create(Url);
                using (HttpWebResponse Response = (HttpWebResponse)Endpoint.GetResponse())
                {
                    Result = Response.StatusCode == HttpStatusCode.OK ? true : false;
                    Response.Close();
                }
            }
            catch
            {
                Result = false;
            }
            return Result;
        }

        static void Check(string EPN, bool IsWorking)
        {
            if (IsWorking)
            {
                Append(EPN, ConsoleColor.White);
                Append("OK", ConsoleColor.Green);
            }
            else
            {
                Append(EPN, ConsoleColor.White);
                Append("DOWN", ConsoleColor.Red);
            }
        }
    }

    public enum Source
    {
        REST,
        CONFIG,
        SERVER,
        DISCORD
    }
}