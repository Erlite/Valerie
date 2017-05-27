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
        public static void DownloadImage(Uri Link, string Name)
        {
            string FileName = Path.Combine(ProfilesHandler.UserImages, $"{Name}.png");
            var Client = new WebClient();
           Client.DownloadFile(Link, FileName);
            Client.Dispose();
        }

        public static void EditImage(string UserImage)
        {
            string GetImage = $"{ProfilesHandler.UserImages}/{UserImage}.png";
            string SavePath = $"{ProfilesHandler.EditImages}/{UserImage}.png";
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

        public static string GetProfile(string Username)
        {
            string Profile = $"{ProfilesHandler.EditImages}/{Username}.png";
            if (!File.Exists(Profile))
            {
                EditImage(Username);
                return Profile;
            }
            else
                return Profile;
        }

    }
}
