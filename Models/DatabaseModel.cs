using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace Valerie.Models
{
    public class DatabaseModel
    {
        public string DatabaseName { get; set; } = "Valerie";
        public string DatabaseUrl { get; set; } = "http://127.0.0.1:8080";
        [JsonProperty("X509CertificatePath")]
        public string CertificatePath { get; set; }
        public string BackupLocation
        {
            get => Directory.Exists($"{Directory.GetCurrentDirectory()}/Backup") ? $"{Directory.GetCurrentDirectory()}/Backup"
                : Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}/Backup").FullName;
        }
        [JsonIgnore]
        public X509Certificate2 Certificate { get => !string.IsNullOrWhiteSpace(CertificatePath) ? new X509Certificate2(CertificatePath) : null; }
    }
}
