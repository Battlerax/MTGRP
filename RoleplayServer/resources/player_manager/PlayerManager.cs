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

            API.onClientEventTrigger += API_onClientEventTrigger;

            DebugManager.debugMessage("[PlayerM] Player Manager initalized.");
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if(eventName == "update_ped_for_client")
            {
                var player = (NetHandle)arguments[0];

                Character c = API.getEntityData(player, "Character");
                c.update_ped();
            }
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
            character.getTimePlayed();//Update time played before save.
            character.save();

            API.resetEntityData(player.handle, "Character");
            players.Remove(character);

            updatePlayerNametags();//IDs change when a player logs off
        }

        public static void updatePlayerNametags()
        {
            foreach(Character c in players)
            {
                c.update_nametag();
            }
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
            Character c = API.shared.getEntityData(player.handle, "Character");
            return c.character_name;
        }

        public static string getAdminName(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            return account.admin_name;
        }

        [Command("getid", GreedyArg = true, Alias = "id")]
        public void getid_cmd(Client sender, string player_name)
        {
            API.sendChatMessageToPlayer(sender, Color.White, "----------- Searching for: " + player_name + " -----------");
            foreach(Character c in players)
            {
                if(c.character_name.StartsWith(player_name, StringComparison.OrdinalIgnoreCase))
                {
                    API.sendChatMessageToPlayer(sender, Color.Grey, c.character_name + " - ID " + PlayerManager.getPlayerId(c));
                }
            }
            API.sendChatMessageToPlayer(sender, Color.White, "-----------------------------------------------------------------------------");
        }
    }
}
