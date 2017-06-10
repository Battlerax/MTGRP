using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.rp_scripts
{
    public class Advertisements : Script
    {
        public Vector3 AdAgencyLoc = new Vector3();

        public Advertisements()
        {

        }

        // Commands

        [Command("advertisement", Alias = "ad", GreedyArg = true)]
        public void advertisement_cmd(Client player, string text)
        {
            if (text.Length < 5)
            {
                player.sendChatMessage("Advertisement text must be longer than 5 characters.");
                return;
            }

            foreach (var p in PlayerManager.Players) //CHECK IF PLAYER HAS A PHONE
            {
                p.Client.sendChatMessage("~g~[AD]: " + text);
                p.Client.sendChatMessage("Advertiser information: ~y~Phone:~w~ "); //ADD PHONE NUMBER
            }

        }
        }
    }
}
