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
            NAPI.Util.ConsoleOutput("[WHITELIST] Whitelist is " + ((_useWhitelist == true) ? ("Active") : ("Inactive")));
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            if (_useWhitelist == true)
            {
                if (!WhitelistedNames.Contains(player.SocialClubName))
                {
                    NAPI.Player.KickPlayer(player, "You are not whitelisted.");
                }
            }
        }

    }
}
