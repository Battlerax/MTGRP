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

        public static Client getPlayerByName(string name)
        {
            foreach(Character c in players)
            {
                if(String.Equals(c.character_name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return c.client;
                }
            }

            return null;
        }

        public static Client getPlayerById(int id)
        {
            Character c = (Character) players.ToArray().GetValue(id);

            if (c.client != null)
                return c.client;

            return null;
        }

        public static int getPlayerId(Character c)
        {
            if (players.Contains(c))
                return players.IndexOf(c);
            else
                return -1;
        }

        public static Client parseClient(string input)
        {
            Client c = getPlayerByName(input);

            if(c == null)
            {
                int id = -1;
                if(Int32.TryParse(input, out id) == true)
                {
                    c = getPlayerById(id);
                }
            }

            if (c != null)
                return c;

            return null;
        }

        public static string getName(Client player)
        {
            Character c = API.shared.getEntityData(player, "Character");
            return c.character_name;
        }
    }
}
