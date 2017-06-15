using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.core;
using RoleplayServer.group_manager;
using RoleplayServer.inventory;

namespace RoleplayServer.player_manager
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
                c?.update_ped();
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

                var account = player.GetAccount();
                account.Save();
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

        [Command("stats")]          //Stats command
        public void GetStatistics(Client sender, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character character = API.getEntityData(sender.handle, "Character");
            Account account = API.shared.getEntityData(sender.handle, "Account");

            if (account.AdminLevel == 0)
            {
                if (receiver != sender)
                {
                    API.sendNotificationToPlayer(sender, "You can't see other player's stats.");
                }
                ShowStats(sender);
            }
            ShowStats(sender, receiver);
        }

        //Show time and time until paycheck.
        [Command("time")]
        public void CheckTime(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            var secondsLeft = 3600 - character.GetTimePlayed();
            API.sendChatMessageToPlayer(player, "The current server time is: " + TimeWeatherManager.CurrentTime.ToString("h:mm:ss tt"));
            API.sendChatMessageToPlayer(player, string.Format("Time until next paycheck: {0}" + " minutes.", secondsLeft / 60));
        }


        //Show player stats (admins can show stats of other players).
        public void ShowStats(Client sender)
        {
            ShowStats(sender, sender);
        }
 
        public void ShowStats(Client sender, Client receiver)
        {
            Character character = API.getEntityData(receiver.handle, "Character");
            Account account = API.shared.getEntityData(receiver.handle, "Account");
            Account senderAccount = API.shared.getEntityData(receiver.handle, "Account");

            API.sendChatMessageToPlayer(sender, "________________PLAYER STATS________________");
            API.sendChatMessageToPlayer(sender, "~g~General:~g~");
            API.sendChatMessageToPlayer(sender,
                $"~h~Character name:~h~ {sender.name} ~h~Account name:~h~ {account.AccountName} ~h~ID:~h~ {character.Id} ~h~Money:~h~ {Money.GetCharacterMoney(character)} ~h~Bank balance:~h~ {character.BankBalance} ~h~Playing hours:~h~ {character.TimePlayed}");
            API.sendChatMessageToPlayer(sender, "~b~Faction:~b~");
            API.sendChatMessageToPlayer(sender,
                $"~h~Faction ID:~h~ {character.GroupId} ~h~Rank:~h~ {character.GroupRank}");
            API.sendChatMessageToPlayer(sender, "~r~Property:~r~");
            //Show property info..

            if (senderAccount.AdminLevel > 0)
            {
                API.sendChatMessageToPlayer(sender, "~y~Admin:~y~");
                API.sendChatMessageToPlayer(sender,
                    $"~h~Admin level:~h~ {account.AdminLevel} ~h~ Admin name:~h~ {account.AdminName} ~h~Last vehicle:~h~ {character.LastVehicle} ~h~Dimension:~h~ {character.LastDimension} ~h~Last IP:~h~ {account.LastIp}");
            }
        }
    }
}