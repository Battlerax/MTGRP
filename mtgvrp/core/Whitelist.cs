using System.Collections.Generic;

using GTANetworkAPI;


namespace mtgvrp.core
{
    public class Whitelist : Script
    {
        private static bool _useWhitelist = true;

        private static readonly List<string> WhitelistedNames = new List<string>
        {
             "battlerax",
             "Ahmad45123",
        };

        public Whitelist()
        {
            Event.OnPlayerConnected += WhiteList_OnPlayerConnect;
            API.ConsoleOutput("[WHITELIST] Whitelist is " + ((_useWhitelist == true) ? ("Active") : ("Inactive")));
        }

        public void WhiteList_OnPlayerConnect(Client player, CancelEventArgs e)
        {
            if (_useWhitelist == true)
            {
                if (!WhitelistedNames.Contains(player.SocialClubName))
                {
                    API.KickPlayer(player, "You are not whitelisted.");
                    e.Cancel = true;
                }
            }
        }

    }
}
