using System.Collections.Generic;

using GTANetworkAPI;


namespace mtgvrp.core
{
    public class Whitelist : Script
    {
        private static bool _useWhitelist = true;

        private static readonly List<string> WhitelistedNames = new List<string>
        {
             "NortonPlays",
             "Ahmad45123",
             "TheSmoothie",
             "iMerle",
             "Charpur",
             "Westingham",
             "xVicee",
             "MTGCharlie",
             "Leuma0",
             "KingstonEU"
        };

        public Whitelist()
        {
            Event.OnPlayerConnected += WhiteList_OnPlayerConnect;
            API.ConsoleOutput("[WHITELIST] Whitelist is " + ((_useWhitelist == true) ? ("Active") : ("Inactive")));
        }

        public void WhiteList_OnPlayerConnect(Client player)
        {
            if (_useWhitelist == true)
            {
                if (!WhitelistedNames.Contains(player.socialClubName))
                {
                    API.KickPlayer(player, "You are not whitelisted.");
                }
            }
        }

    }
}
