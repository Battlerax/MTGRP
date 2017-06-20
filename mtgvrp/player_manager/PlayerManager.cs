using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.weapon_manager;
using mtgvrp.inventory;
using mtgvrp.core;
using mtgvrp.group_manager;

namespace mtgvrp.player_manager
{
    class PlayerManager : Script
    {
        private static Dictionary<int, Character> _players = new Dictionary<int, Character>();
        public static List<Character> Players => _players.Values.ToList();

        public static void AddPlayer(Character c)
        {
            int id = -1;
            for (var i = 0; i < API.shared.getMaxPlayers(); i++)
            {
                if (_players.ContainsKey(i) == false)
                {
                    id = i;
                    break;
                }
            }

            if(id == -1) return;

            _players.Add(id, c);
        }

        public static void RemovePlayer(Character c)
        {
            _players.Remove(GetPlayerId(c));
        }


        public PlayerManager()
        {
            DebugManager.DebugMessage("[PlayerM] Initalizing player manager...");

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;
            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerRespawn += API_onPlayerRespawn;

            DebugManager.DebugMessage("[PlayerM] Player Manager initalized.");
        }

        private void API_onPlayerRespawn(Client player)
        {
            player.sendChatMessage("You were revived by the ~b~Los Santos Medical Department ~w~ and were charged 500$ for hospital fees.");
            WeaponManager.RemoveAllPlayerWeapons(player);
            InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), 500);
        }

        //TODO: CHANGED ONCE THE LS GOV IS ADDED
        public static int basepaycheck = 500;
        public static int taxationAmount = 4;
        public static int VIPBonusLevelOne = 10;
        public static int VIPBonusLevelTwo = 20;
        public static int VIPBonusLevelThree = 30;

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
                character.Health = API.getPlayerHealth(player);
                character.LastPos = player.position;
                character.LastRot = player.rotation;
                character.GetTimePlayed(); //Update time played before save.
                character.Save();

                API.resetEntityData(player.handle, "Character");
                RemovePlayer(character);

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
            if (!_players.ContainsKey(id))
            {
                return null;
            }

            var c = _players[id];

            return c.Client ?? null;
        }

        public static int GetPlayerId(Character c)
        {
            if (_players.ContainsValue(c))
                return _players.Single(x => x.Value == c).Key;
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

        public static int getVIPPaycheckBonus(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.VipLevel == 1) { return VIPBonusLevelOne; }
            if (account.VipLevel == 2) { return VIPBonusLevelTwo; }
            if (account.VipLevel == 3) { return VIPBonusLevelThree; }
            else { return 0; }
        }

        public static int getFactionBonus(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            return character.Group.FactionPaycheckBonus;

        }

        public static int CalculatePaycheck(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            return basepaycheck - (basepaycheck * taxationAmount/100) + (basepaycheck * getVIPPaycheckBonus(player)/100) + getFactionBonus(player) + character.BankBalance/1000;
        }

        public static void SendPaycheckToPlayer(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            if(character != null)
            if ( character.GetTimePlayed() % 3600 == 0)
            {
                int paycheckAmount = CalculatePaycheck(player);
                character.BankBalance += paycheckAmount;
                player.sendChatMessage("--------------PAYCHECK RECEIVED!--------------");
                player.sendChatMessage("Base paycheck: $" + basepaycheck + ".");
                player.sendChatMessage("Interest: $" + character.BankBalance / 1000 + ".");
                player.sendChatMessage("You were taxed at " + taxationAmount + "%.");
                player.sendChatMessage("VIP bonus: " + getVIPPaycheckBonus(player)+ "%.");
                player.sendChatMessage("Faction bonus: $" + getFactionBonus(player) + ".");
                player.sendChatMessage("----------------------------------------------");
                player.sendChatMessage("Total: ~g~$" + paycheckAmount + "~w~.");

                player.sendPictureNotificationToPlayer("Your paycheck for ~g~$" + paycheckAmount + " ~w~has been added to your balance.", "CHAR_BANK_MAZE", 0, 0, "Maze Bank", "Paycheck Received!");
            }
        }

        [Command("setvipbonus")]
        public void setvipbonus_cmd(Client player, string viplevel, string percentage)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 6) { return; }

            switch (viplevel)
            {
                case "1":
                    VIPBonusLevelOne = int.Parse(percentage);
                    break;

                case "2":
                    VIPBonusLevelTwo = int.Parse(percentage);
                    break;

                case "3":
                    VIPBonusLevelThree = int.Parse(percentage);
                    break;
            }
            player.sendChatMessage("You have set VIP level " + viplevel + "'s paycheck bonus to " + percentage + "%.");
        }

        [Command("settax")]
        public void settax_cmd(Client player, string percentage)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 6) { return; }
            taxationAmount = int.Parse(percentage);
        }

        [Command("setbasepaycheck", GreedyArg = true)]
        public void setbasepaycheck_cmd(Client player, string amount)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 6) { return; }
            basepaycheck = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "Base paycheck set to $" + amount + ".");
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

            if (receiver == null)
            {
                receiver = sender;
            }

            if (account.AdminLevel < 2 && receiver != sender)
            {
                if (receiver != sender)
                {
                    return;
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

            API.sendChatMessageToPlayer(player, Color.White, "__________________ TIME __________________");
            API.sendChatMessageToPlayer(player, Color.Grey, "The current server time is: " + DateTime.Now.ToString("h:mm:ss tt"));
            API.sendChatMessageToPlayer(player, Color.Grey, "The current in-game time is: " + TimeWeatherManager.CurrentTime.ToString("h:mm:ss tt"));
            API.sendChatMessageToPlayer(player, Color.Grey,
                $"Time until next paycheck: { (int)(3600 - (character.GetTimePlayed() % 3600)) / 60}" + " minutes.");
            API.sendChatMessageToPlayer(player, Color.White, "__________________ TIME __________________");
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

            API.sendChatMessageToPlayer(sender, string.Format("~h~Character name:~h~ {0} ~h~Account name:~h~ {1} ~h~ID:~h~ {2} ~h~Money:~h~ {3} ~h~Bank balance:~h~ {4} ~h~Playing hours:~h~ {5}", character.CharacterName, account.AccountName, character.Id, Money.GetCharacterMoney(character), character.BankBalance, character.GetPlayingHours()));
            API.sendChatMessageToPlayer(sender, string.Format("~h~Age:~h~ {0} ~h~Birthplace:~h~ {1} ~h~Birthday:~h~ {2} ~h~VIP level:~h~ {3} ~h~VIP expires:~h~ {4}", character.Age, character.Birthplace, character.Birthday, account.VipLevel, account.VipExpirationDate));
            API.sendChatMessageToPlayer(sender, "~b~Faction/Jobs:~b~");
            //API.sendChatMessageToPlayer(sender, string.Format("~h~Faction ID:~h~ {0} ~h~Rank:~h~ {1} ~h~Group name:~h~ {2} ~h~Job 1:~h~ {3} ~h~Job 2: {4}", character.GroupId, character.GroupRank, character.Group.Name, character.JobOne));
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