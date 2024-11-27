using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butter
{
    public class Program
    {
        public static async Task Main()
        {
            BotMain.Instance.Init();
            await Task.Delay(-1);
        }
    }
}
