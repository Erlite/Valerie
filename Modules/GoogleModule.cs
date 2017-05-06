using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Rick.Services;
using Rick.Classes;
using Rick.Attributes;

namespace Rick.Modules
{
    [Group("Google"), CheckBlacklist]
    public class GoogleModule : ModuleBase
    {
    }
}
