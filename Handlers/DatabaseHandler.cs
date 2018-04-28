using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

namespace Valerie.Handlers
{
    public class DatabaseHandler
    {
        [JsonProperty("DatabaseName")]
        public string DatabaseName { get; set; } = "Valerie";

        [JsonProperty("RavenDB-URL")]
        public string DatabaseUrl { get; set; } = "http://127.0.0.1:8080";

        [JsonProperty("X509CertificatePath")]
        private string CertificatePath { get; set; }

        [JsonIgnore]
        public X509Certificate2 Certificate { get => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null; }

        public static async Task<DatabaseHandler> LoadDBConfigAsync()
        {
            var DBConfigPath = $"{Directory.GetCurrentDirectory()}/DatabaseConfig.json";
            if (File.Exists(DBConfigPath)) return JsonConvert.DeserializeObject<DatabaseHandler>(await File.ReadAllTextAsync(DBConfigPath));

            await File.WriteAllTextAsync(DBConfigPath, JsonConvert.SerializeObject(new DatabaseHandler(), Formatting.Indented));
            return JsonConvert.DeserializeObject<DatabaseHandler>(await File.ReadAllTextAsync(DBConfigPath));
        }
    }
}