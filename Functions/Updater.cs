using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using Rick.Enums;
using Rick.Handlers;

namespace Rick.Functions
{
    public class Updater
    {
        //public static async void ProgramUpdater()
        //{
        //    var BotConfig = ConfigHandler.IConfig;
        //    if (BotConfig.AutoUpdate)
        //    {
        //        Logger.Log(LogType.Info, LogSource.Configuration, "Checking for updates ..");
        //        var Http = new HttpClient();

        //        Int32.TryParse(await Http.GetStringAsync("https://rickbot.cf/Downloads/version.txt"), out int version);

        //        if (ConfigHandler.BotVersion < version)
        //        {
        //            Logger.Log(LogType.Info, LogSource.Configuration, $"New version is available! Version: {version}.\nWould you like to update now? ");
        //            Logger.Log(LogType.Info, LogSource.Configuration, "Type Yes to update!");
        //            var Response = Console.ReadLine().ToLower();
        //            if (Response == "yes")
        //            {
        //                Logger.Log(LogType.Info, LogSource.Configuration, $"Downloading Update .... ");
        //                Uri url = new Uri("https://rickbot.cf/Downloads/Installer.bat");
        //                await Http.DownloadAsync(url, "Installer.bat");
        //                Process.Start("Installer.bat");
        //                await Task.Delay(5000);
        //                Process.GetCurrentProcess().Kill();
        //            }
        //            else
        //                Logger.Log(LogType.Warning, LogSource.Configuration, $"Ignoring Update ...");
        //        }
        //        else
        //            Logger.Log(LogType.Info, LogSource.Configuration, $"Already using the latest version: {ConfigHandler.BotVersion}");
        //    }
        //    else
        //        Logger.Log(LogType.Warning, LogSource.Configuration, $"Update is disabled! Continuing..");
        //}
    }
}
