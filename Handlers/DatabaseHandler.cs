using System;
using System.IO;
using System.Linq;
using Valerie.Enums;
using Valerie.Models;
using Newtonsoft.Json;
using Valerie.Services;
using System.Diagnostics;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Color = System.Drawing.Color;
using System.Runtime.InteropServices;
using Raven.Client.ServerWide.Operations;
using Raven.Client.Documents.Operations.Backups;

namespace Valerie.Handlers
{
    public class DatabaseHandler
    {
        IDocumentStore Store { get; }
        ConfigHandler Config { get; }
        public DatabaseHandler(IDocumentStore store, ConfigHandler config)
        {
            Store = store;
            Config = config;
        }

        public static DatabaseModel DBConfig
        {
            get
            {
                var DBConfigPath = $"{Directory.GetCurrentDirectory()}/DBConfig.json";
                if (File.Exists(DBConfigPath)) return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(DBConfigPath));
                File.WriteAllText(DBConfigPath, JsonConvert.SerializeObject(new DatabaseModel(), Formatting.Indented));
                return JsonConvert.DeserializeObject<DatabaseModel>(File.ReadAllText(DBConfigPath));
            }
        }

        public async Task DatabaseCheck()
        {
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
            {
                LogService.Write(LogSource.DTB, "Raven Server isn't running. Please make sure RavenDB is running.\nExiting ...", Color.Crimson);
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }

            await DatabaseSetupAsync().ConfigureAwait(false);
            Config.ConfigCheck();
        }

        async Task DatabaseSetupAsync()
        {
            if (Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).Any(x => x == DBConfig.DatabaseName)) return;
            LogService.Write(LogSource.DTB, $"Database {DBConfig.DatabaseName} doesn't exist!", Color.IndianRed);

            await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(DBConfig.DatabaseName)));
            LogService.Write(LogSource.DTB, $"Created Database{DBConfig.DatabaseName}.", Color.ForestGreen);

            LogService.Write(LogSource.DTB, "Setting up backup operation...", Color.YellowGreen);
            await Store.Maintenance.SendAsync(new UpdatePeriodicBackupOperation(new PeriodicBackupConfiguration
            {
                Name = "Backup",
                BackupType = BackupType.Backup,
                FullBackupFrequency = "*/10 * * * *",
                IncrementalBackupFrequency = "0 2 * * *",
                LocalSettings = new LocalSettings { FolderPath = DBConfig.BackupLocation }
            })).ConfigureAwait(false);

            LogService.Write(LogSource.DTB, "Finished backup operation!", Color.YellowGreen);
            LogService.Write(LogSource.DTB, $"\n1: Restore Database\n2: Import Dump File  ", Color.LemonChiffon);

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1: await RestoreAsync().ConfigureAwait(false); break;
                case ConsoleKey.D2: Import(); break;
            }
        }

        async Task RestoreAsync()
        {
            LogService.Write(LogSource.DTB, $"Beginning Backup Restore...", Color.GreenYellow);
            LogService.Write(LogSource.DTB, "Move Backup files to CurrentDIR/Backup. Press any key to continue...", Color.YellowGreen);
            Console.ReadKey();
            if (!Directory.GetFiles(DBConfig.BackupLocation).Any())
            {
                LogService.Write(LogSource.DTB, $"No files found to restore.", Color.OrangeRed);
                return;
            }
            await (await Store.Maintenance.Server.SendAsync(new RestoreBackupOperation(new RestoreBackupConfiguration
            {
                DatabaseName = DBConfig.DatabaseName,
                BackupLocation = DBConfig.BackupLocation,
            }))).WaitForCompletionAsync();
            LogService.Write(LogSource.DTB, $"Finished Database Restore!", Color.ForestGreen);
        }

        void Import()
        {
            LogService.Write(LogSource.DTB, "Move Dump.ravendbdump file to root of current directory. Press any key to continue...", Color.YellowGreen);
            Console.ReadKey();
            if (!File.Exists($"{Directory.GetCurrentDirectory()}/Dump.ravendbdump"))
            {
                LogService.Write(LogSource.DTB, $"No dump file found to restore.", Color.OrangeRed);
                return;
            }
            string Arguments = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Arguments =
                    "curl -F 'importOptions={\"IncludeExpired\":true,\"RemoveAnalyzers\":false,\"OperateOnTypes\":\"DatabaseRecord,Documents,Conflicts,Indexes,RevisionDocuments,Identities,CompareExchange\"}'" +
                $" -F 'file=@Dump.ravendbdump' {DBConfig.DatabaseUrl}/databases/{DBConfig.DatabaseName}/smuggler/import";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Arguments = "";

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = Arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}