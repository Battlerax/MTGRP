using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.group_manager;
using RoleplayServer.resources.inventory;

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

        [Command("stats", GreedyArg = false)]
        public void getStatistics(Client sender, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character character = API.getEntityData(sender.handle, "Character");
            Account account = API.shared.getEntityData(sender.handle, "Account");

            if (account.AdminLevel < 2 && receiver != sender)
            {
                showStats(sender, sender);
                return;
            }
            showStats(sender, receiver);
        }

        //Show time and time until paycheck.
        [Command("time")]
        public void checkTime(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            TimeSpan t = TimeSpan.FromMilliseconds(character.GetTimePlayed());

            var minutesLeft = 60 - t.TotalMinutes;
            API.sendChatMessageToPlayer(player, "The current server time is: " + DateTime.Now.ToString("h:mm:ss tt"));
            API.sendChatMessageToPlayer(player, "The current in-game time is: " + TimeWeatherManager.CurrentTime.ToString("h:mm:ss tt"));
            API.sendChatMessageToPlayer(player, string.Format("Time until next paycheck: {0}" + " minutes.", minutesLeft));
        }


        //Show player stats (admins can show stats of other players).

        public void showStats(Client sender, Client receiver = null)
        {
            Character character = API.getEntityData(sender.handle, "Character");
            Account account = API.shared.getEntityData(sender.handle, "Account");

            if (receiver != null)
            {
                character = API.getEntityData(receiver.handle, "Character");
                account = API.shared.getEntityData(receiver.handle, "Account");
            }

            Account senderAccount = API.shared.getEntityData(sender.handle, "Account");

            TimeSpan t = TimeSpan.FromMilliseconds(character.GetTimePlayed());
            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);

            API.sendChatMessageToPlayer(sender, "________________PLAYER STATS________________");
            API.sendChatMessageToPlayer(sender, "~g~General:~g~");
            API.sendChatMessageToPlayer(sender, string.Format("~h~Character name:~h~ {0} ~h~Account name:~h~ {1} ~h~ID:~h~ {2} ~h~Money:~h~ {3} ~h~Bank balance:~h~ {4} ~h~Playing hours:~h~ {5}", sender.name, account.AccountName, character.Id, Money.GetCharacterMoney(character), character.BankBalance, character.TimePlayed));
            API.sendChatMessageToPlayer(sender, string.Format("~h~Age:~h~ {0} ~h~Birthplace:~h~ {1} ~h~Birthday:~h~ {2} ~h~VIP level:~h~ {3} ~h~VIP expires:~h~ {4}", character.Age, character.Birthplace, character.Birthday, account.VipLevel, account.VipExpirationDate));
            API.sendChatMessageToPlayer(sender, "~b~Faction/Jobs:~b~");
            //API.sendChatMessageToPlayer(sender, string.Format("~h~Faction ID:~h~ {0} ~h~Rank:~h~ {1} ~h~Group name:~h~ {2} ~h~Job 1:~h~ {3} ~h~Job 2: {4}", character.GroupId, character.GroupRank, character.Group.Name, character.JobOne));
            API.sendChatMessageToPlayer(sender, "~r~Property:~r~");
            //Show property info..

            if (senderAccount.AdminLevel > 0)
            {
                API.sendChatMessageToPlayer(sender, "~y~Admin:~y~");
                API.sendChatMessageToPlayer(sender, string.Format("~h~Admin level:~h~ {0} ~h~ Admin name:~h~ {1} ~h~Last vehicle:~h~ {2} ~h~Dimension:~h~ {3} ~h~Last IP:~h~ {4}", account.AdminLevel, account.AdminName, character.LastVehicle, character.LastDimension, account.LastIp));
            }
        }
    }
}