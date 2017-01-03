using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    class PlayerManager : Script
    {
        public static List<Character> players = new List<Character>();

        public PlayerManager()
        {
            DebugManager.debugMessage("[PlayerM] Initalizing player manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;

            DebugManager.debugMessage("[PlayerM] Player Manager initalized.");
        }

        public void OnPlayerConnected(Client player)
        {
            Account account = new Account();
            account.account_name = player.socialClubName;

            API.setEntityData(player.handle, "Account", account);
        }

        public void OnPlayerDisconnected(Client player, string reason)
        {
            //Save data
            Character character = API.getEntityData(player.handle, "Character");

            character.last_pos = player.position;
            character.last_rot = player.rotation;
            character.save();

            API.resetEntityData(player.handle, "Character");
            players.Remove(character);
        }
    }
}
