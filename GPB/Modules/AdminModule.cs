using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using GPB.Services;

namespace GPB.Modules
{
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase
    {
        private LogService log;

        public AdminModule(LogService Logger)
        {
            log = Logger;
        }

    }
}