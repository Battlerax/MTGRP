using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.group_manager;

namespace RoleplayServer.resources.player_manager
{
    class PlayerManager : Script
    {
        public static List<Character> Players = new List<Character>();

        public PlayerManager()
        {
            DebugManager.DebugMessage("[PlayerM] Initalizing player manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;

            API.onClientEventTrigger += API_onClientEventTrigger;

            DebugManager.DebugMessage("[PlayerM] Player Manager initalized.");
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
            var account = new Account();
            account.AccountName = player.socialClubName;

            API.setEntityData(player.handle, "Account", account);
        }

        public void OnPlayerDisconnected(Client player, string reason)
        {
            //Save data
            Character character = API.getEntityData(player.handle, "Character");

            if (character != null)
            {
                if (character.Group != Group.None)
                {
                    GroupManager.SendGroupMessage(player,
                        character.CharacterName + " from your group has left the server. (" + reason + ")");
                }


                character.LastPos = player.position;
                character.LastRot = player.rotation;
                character.GetTimePlayed(); //Update time played before save.
                character.Save();

                API.resetEntityData(player.handle, "Character");
                Players.Remove(character);

                UpdatePlayerNametags(); //IDs change when a player logs off
            }
        }

        public static void UpdatePlayerNametags()
        {
            foreach(var c in Players)
            {
                c.update_nametag();
            }
        }

        public static Client GetPlayerByName(string name)
        {
            foreach(var c in Players)
            {
                if(string.Equals(c.CharacterName, name, StringComparison.OrdinalIgnoreCase))
                {
                    return c.Client;
                }
            }

            return null;
        }

        public static Client GetPlayerById(int id)
        {
            if (id < 0 || id > Players.Count - 1)
            {
                return null;
            }

            var c = (Character) Players.ToArray().GetValue(id);

            if (c.Client != null)
                return c.Client;

            return null;
        }

        public static int GetPlayerId(Character c)
        {
            if (Players.Contains(c))
                return Players.IndexOf(c);
            else
                return -1;
        }

        public static Client ParseClient(string input)
        {
            var c = GetPlayerByName(input);

            if (c != null) return c;

            var id = -1;
            if(int.TryParse(input, out id))
            {
                c = GetPlayerById(id);
            }

            return c;
        }

        public static string GetName(Client player)
        {
            Character c = API.shared.getEntityData(player.handle, "Character");
            return c.CharacterName;
        }

        public static string GetAdminName(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            return account.AdminName;
        }

        [Command("getid", GreedyArg = true, Alias = "id")]
        public void getid_cmd(Client sender, string playerName)
        {
            API.sendChatMessageToPlayer(sender, Color.White, "----------- Searching for: " + playerName + " -----------");
            foreach(var c in Players)
            {
                if(c.CharacterName.StartsWith(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    API.sendChatMessageToPlayer(sender, Color.Grey, c.CharacterName + " - ID " + GetPlayerId(c));
                }
            }
            API.sendChatMessageToPlayer(sender, Color.White, "------------------------------------------------------------");
        }
    }
}
