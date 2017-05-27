using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;

namespace Rick.Services
{
    public class ProfileService
    {
        static string CacheFolder = Path.Combine(Directory.GetCurrentDirectory(), "Cache");
        static string UserImages = Path.Combine(CacheFolder, "Downloads");
        static string EditImages = Path.Combine(CacheFolder, "Edits");

        public static void DownloadImage(Uri Link, string Name)
        {
            string FileName = Path.Combine(UserImages, $"{Name}.png");
            var Client = new WebClient();
           Client.DownloadFile(Link, FileName);
        }

        public static void DirectoryCheck()
        {
            if (!Directory.Exists(CacheFolder))
                Directory.CreateDirectory(CacheFolder);
            if (!Directory.Exists(UserImages))
                Directory.CreateDirectory(UserImages);
            if (!Directory.Exists(EditImages))
                Directory.CreateDirectory(EditImages);
            var UserImage = Directory.GetFiles(UserImages);
            var EditImage = Directory.GetFiles(EditImages);
            foreach (var x in UserImage)
                File.Delete(x);
            foreach (var x in EditImage)
                File.Delete(x);
        }

    }
}
