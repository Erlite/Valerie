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
using System.Drawing.Imaging;


namespace Rick.Services
{
    public class ProfileService
    {
        static string CacheFolder = Path.Combine(BotHandler.Data, "Cache");
        static string UserImages = Path.Combine(CacheFolder, "Downloads");
        static string Resources = Path.Combine(CacheFolder, "Resources");
        public static string EditImages = Path.Combine(CacheFolder, "Edits");

        public static void DownloadImage(Uri Link, string Name)
        {
            string FileName = Path.Combine(UserImages, $"{Name}.png");
            var Client = new WebClient();
           Client.DownloadFile(Link, FileName);
            Client.Dispose();
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
            if (!Directory.Exists(Resources))
            {
                Directory.CreateDirectory(Resources);
                ConsoleService.Log("Config", "Creating Resources Folder ...");
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
            string SavePath = $"{EditImages}/{UserImage}.png";
            Image User = Image.FromFile(GetImage);

            var image = new Bitmap(600, 400, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(image))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(User, 470, 60);
            }
            image.Save(SavePath);

            image.Dispose();

            User.Dispose();
        }

    }
}
