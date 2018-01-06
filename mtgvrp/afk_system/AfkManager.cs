using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkAPI;
using mtgvrp.core;

namespace mtgvrp.afk_system
{
    public class AfkManager : Script
    {
        Timer _afkTimer;

        public AfkManager()
        {
            _afkTimer = new Timer()
            {
                Interval = 10000,
                AutoReset = true
            };
            _afkTimer.Elapsed += _afkTimer_Elapsed;
            _afkTimer.Start();
        }

        //10 Minutes
        private const int KickInterval = 60;

        //9 Minute warning 
        private const int WarningTimer = 54;


        private void _afkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var p in API.GetAllPlayers())
            {
                if(p == null)
                    continue;
                var c = p.GetCharacter();
                if (c == null)
                    return;
                if (!c.IsCreated)
                    return;

                if (c.LastPos == p.Position)
                {
                    c.AfkTimer++;
                    if (c.AfkTimer == WarningTimer)
                    {
                        API.SendChatMessageToPlayer(p,"~r~[AFK WARNING] You will be kicked in one minute for being AFK!");
                    }
                    else if (c.AfkTimer >= KickInterval)
                    {
                        API.KickPlayer(p, "AFK for longer than 10 minutes.");
                    }
                }
                else
                {
                    c.LastPos = p.Position;
                    c.AfkTimer = 0;
                }
            }
        }
    }
}
