using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Interfaces
{
    public interface IHandler
    {
        Task Close();
    }
}
