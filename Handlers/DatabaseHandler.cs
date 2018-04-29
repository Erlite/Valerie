using System.IO;
using System.Linq;
using Valerie.Enums;
using Valerie.Services;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Console = System.Drawing.Color;
using Raven.Client.ServerWide.Operations;
using Raven.Client.Documents.Operations.Backups;
using System.Security.Cryptography.X509Certificates;

namespace Valerie.Handlers
{
    public class DatabaseHandler
    {
        [JsonProperty("DatabaseName")]
        public string DatabaseName { get; set; } = "Velixa";

        [JsonProperty("RavenDB-URL")]
        public string DatabaseUrl { get; set; } = "http://127.0.0.1:8080";

        [JsonProperty("X509CertificatePath")]
        private string CertificatePath { get; set; }

        [JsonProperty("BackupLocation")]
        public string BackupLocation
        {
            get => Directory.Exists($"{Directory.GetCurrentDirectory()}/Backup") ? $"{Directory.GetCurrentDirectory()}/Backup"
                : Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Backup").FullName;
        }

        [JsonIgnore]
        public X509Certificate2 Certificate { get => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null; }

        public static async Task<DatabaseHandler> LoadDBConfigAsync()
        {
            var DBConfigPath = $"{Directory.GetCurrentDirectory()}/DatabaseConfig.json";
            if (File.Exists(DBConfigPath)) return JsonConvert.DeserializeObject<DatabaseHandler>(await File.ReadAllTextAsync(DBConfigPath));

            await File.WriteAllTextAsync(DBConfigPath, JsonConvert.SerializeObject(new DatabaseHandler(), Formatting.Indented));
            return JsonConvert.DeserializeObject<DatabaseHandler>(await File.ReadAllTextAsync(DBConfigPath));
        }

        public async Task LoadAndRestoreAsync(DatabaseHandler Database, IDocumentStore Store)
        {
            if (Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).Any(x => x == Database.DatabaseName)) return;

            LogService.Write(LogSource.DTB, $"Datbase {Database.DatabaseName} doesn't exist!", Console.IndianRed);
            await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(Database.DatabaseName)));
            LogService.Write(LogSource.DTB, $"Created Database {Database.DatabaseName}.", Console.ForestGreen);
            LogService.Write(LogSource.DTB, $"Beginning Backup Restore...", Console.GreenYellow);
            if (!Directory.GetFiles(Database.BackupLocation).Any())
            {
                LogService.Write(LogSource.DTB, $"No files found to restore.", Console.OrangeRed);
                return;
            }
            await (await Store.Maintenance.Server.SendAsync(new RestoreBackupOperation(new RestoreBackupConfiguration
            {
                DatabaseName = Database.DatabaseName,
                BackupLocation = Database.BackupLocation
            }))).WaitForCompletionAsync();
            LogService.Write(LogSource.DTB, $"Finished Database Restore!", Console.ForestGreen);
        }
    }
}