using System.Collections.Generic;
using GTANetworkServer;

namespace RoleplayServer.core
{
    public class Whitelist : Script
    {

       

        private static bool _useWhitelist = true;

        private static readonly List<string> WhitelistedNames = new List<string>
        {
             "ChenkoRules",
             "nickson1993",
             "NortonPlays",
             "Millingtonlol",
             "Ahmad45123",

             //Kevin & Friends
             "maniac1994",
             "DontCallMeKevin",
             "Wilko0103",
             "Maxispio",
        };

        public Whitelist()
        {
            API.onPlayerConnected += WhiteList_OnPlayerConnect;
            API.consoleOutput("[WHITELIST] Whitelist is " + ((_useWhitelist == true) ? ("Active") : ("Inactive")));
        }

        public void WhiteList_OnPlayerConnect(Client player)
        {
            if (_useWhitelist == true)
            {
                if (!WhitelistedNames.Contains(player.socialClubName))
                {
                    API.kickPlayer(player, "You are not whitelisted.");
                }
            }
        }

    }
}