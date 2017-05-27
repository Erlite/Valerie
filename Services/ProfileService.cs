using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using Rick.Handlers;
using System.Drawing;


namespace Rick.Services
{
    public class ProfileService
    {
        static string CacheFolder = Path.Combine(BotHandler.Data, "Cache");
        static string UserImages = Path.Combine(CacheFolder, "Downloads");
        public static string EditImages = Path.Combine(CacheFolder, "Edits");

        public static void DownloadImage(Uri Link, string Name)
        {
            string FileName = Path.Combine(UserImages, $"{Name}.png");
            var Client = new WebClient();
           Client.DownloadFile(Link, FileName);
        }

        public static void DirectoryCheck()
        {
            if (!Directory.Exists(BotHandler.Data))
            {
                Directory.CreateDirectory(BotHandler.Data);
                ConsoleService.Log("Config", "Creating Data Folder ...");
            }
            if (!Directory.Exists(CacheFolder))
            {
                Directory.CreateDirectory(CacheFolder);
                ConsoleService.Log("Config", "Creating Cache Folder ...");
            }
            if (!Directory.Exists(UserImages))
            {
                Directory.CreateDirectory(UserImages);
                ConsoleService.Log("Config", "Creating User Images Folder ...");
            }
            if (!Directory.Exists(EditImages))
            {
                Directory.CreateDirectory(EditImages);
                ConsoleService.Log("Config", "Creating Images Edit Folder ...");
            }
            var UserImage = Directory.GetFiles(UserImages);
            var EditImage = Directory.GetFiles(EditImages);
            foreach (var x in UserImage)
                File.Delete(x);
            foreach (var x in EditImage)
                File.Delete(x);
        }

        public static void EditImage(string UserImage)
        {
            string GetImage = $"{UserImages}/{UserImage}.png";
            Image Img = Image.FromFile(GetImage);
            using (Graphics g = Graphics.FromImage(Img))
                g.DrawLine(Pens.Black, 10, 10, 20, 20);
            string SavePath = $"{EditImages}/{UserImage}.png";
            Img.Save(SavePath);

        }

    }
}
