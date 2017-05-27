using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rick.Models;
using Rick.Services;
using System.IO;

namespace Rick.Handlers
{
    class ProfilesHandler
    {
        const string SavePath = "Data/Profiles.json";

        public List<ProfileModel> Profiles = new List<ProfileModel>();
    }
}
