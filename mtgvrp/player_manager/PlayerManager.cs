using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;


using GTANetworkAPI;


using mtgvrp.vehicle_manager;
using mtgvrp.weapon_manager;
using mtgvrp.inventory;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.group_manager;

using Color = mtgvrp.core.Color;

namespace mtgvrp.player_manager
{
    class PlayerManager : Script
    {
        private static readonly Dictionary<int, Character> _players = new Dictionary<int, Character>();

        public static List<Character> Players => _players.Values.ToList();

        public Timer PlayerSaveTimer = new Timer();

        public static void AddPlayer(Character c)
        {
            int id = -1;
            for (var i = 0; i < API.Shared.GetMaxPlayers(); i++)
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

            Event.OnPlayerConnected += OnPlayerConnected;
            Event.OnPlayerDisconnected += OnPlayerDisconnected;
            Event.OnPlayerDeath += API_onPlayerDeath;
            Event.OnPlayerDamage += API_onPlayerHealthChange;

            //Setup save timer.
            PlayerSaveTimer.Interval = 900000;
            PlayerSaveTimer.Elapsed += PlayerSaveTimer_Elapsed;
            PlayerSaveTimer.Start();

            //setup paycheck timer
            _payCheckTimer = new Timer {Interval = 1000, AutoReset = true };
            _payCheckTimer.Elapsed += _payCheckTimer_Elapsed; ;
            _payCheckTimer.Start();

            DebugManager.DebugMessage("[PlayerM] Player Manager initalized.");
        }

        private void PlayerSaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var player in API.Shared.GetAllPlayers())
            {
                if (player == null)
                    continue;

                var character = player.GetCharacter();
                if (character == null)
                    continue;

                character.Save();

                player.SendChatMessage("Character saved.");
            }
        }

        private void API_onPlayerHealthChange(Client entity, float lossFirst, float lossSecond)
        {
            Client player = API.GetPlayerFromHandle(entity);
            var character = player.GetCharacter();
            Account account = player.GetAccount();
            if (account == null)
                return;
            if (character == null)
                return;

            if (API.GetPlayerHealth(player) < 100 && account.AdminDuty)
            {
                API.SetPlayerHealth(player, 100);
            }

            character.Health = API.GetPlayerHealth(player);
        }


        private void API_onPlayerDeath(Client player, Client entityKiller, uint weapon, CancelEventArgs cancel)
        {
            if (player.GetAccount().AdminDuty)
            {
                // Admins are losing weapons anyway, may as well remove the ghost weapons from inv.
                WeaponManager.RemoveAllPlayerWeapons(player);
                return;
            }

            var character = player.GetCharacter();
            player.SendChatMessage("You were revived by the ~b~Los Santos Medical Department ~w~ and were charged $200 for hospital fees.");
            WeaponManager.RemoveAllPlayerWeapons(player);
            int amount = -200;

            if (character.IsJailed)
            {
                group_manager.lspd.Lspd.JailControl(player, character.JailTimeLeft);
            }

            if (Money.GetCharacterMoney(character) < 200)
            {
                character.BankBalance += amount;
            }

            else
            {
                InventoryManager.DeleteInventoryItem(player.GetCharacter(), typeof(Money), 200);
            }

            Client killer = API.GetPlayerFromHandle(entityKiller);
            if (killer != null)
            {
                LogManager.Log(LogManager.LogTypes.Death, $"{character.CharacterName}[{player.SocialClubName}] has died. Killer: {killer.GetCharacter().rp_name()}[{killer.GetAccount().AccountName}]. Weapon: {((WeaponHash)weapon).ToString()}");
            }
            else
            {
                LogManager.Log(LogManager.LogTypes.Death, $"{character.CharacterName}[{player.SocialClubName}] has died. Weapon: {((WeaponHash)weapon).ToString()}");
            }
            
        }

        public static int basepaycheck = Properties.Settings.basepaycheck;
        public static int taxationAmount = Properties.Settings.taxationamount;

        [RemoteEvent("update_ped_for_client")]
        private void UpdatePedForClient(Client sender, params object[] arguments)
        {
            var player = (NetHandle)arguments[0];
            Character c = API.GetEntityData(player, "Character");
            c?.update_ped(sender);
        }

        public void OnPlayerConnected(Client player, CancelEventArgs e)
        {
            var account = new Account();
            account.AccountName = player.SocialClubName;

            API.SetEntityData(player, "Account", account);
        }

        public void OnPlayerDisconnected(Client player, byte type, string reason)
        {
            //Save data
            Character character = player.GetCharacter();

            if (character != null)
            {
                var account = player.GetAccount();

                if (account.AdminLevel > 0)
                {
                    foreach (var p in PlayerManager.Players)
                    {
                        if (p.Client.GetAccount().AdminLevel > 0)
                        {
                            p.Client.SendChatMessage($"Admin {account.AdminName} has left the server.");
                        }
                    }
                }

                if (character.Group != Group.None)
                {
                    GroupManager.SendGroupMessage(player,
                        character.rp_name() + " from your group has left the server. (" + reason + ")");
                }
                
                account.Save();
                character.Save();

                //Stop all timers
                foreach (var timerVar in typeof(Character).GetProperties().Where(x => x.PropertyType == typeof(Timer)))
                {
                    var tim = (Timer)timerVar.GetValue(character);
                    tim?.Stop();
                }

                RemovePlayer(character);
                LogManager.Log(LogManager.LogTypes.Connection, $"{character.CharacterName}[{player.SocialClubName}] has left the server.");
            }
            else
                LogManager.Log(LogManager.LogTypes.Connection, $"{player.SocialClubName} has left the server. (Not logged into a character)");
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
            if (name == null)
                return null;

            foreach (var c in Players)
            {
                if (c.Client.GetAccount().AdminName == null)
                    c.Client.GetAccount().AdminName = "";

                if (c.CharacterName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    c.Client.GetAccount().AdminName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    c.CharacterName.StartsWith(name, StringComparison.OrdinalIgnoreCase) ||
                    c.Client.GetAccount().AdminName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
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
            {
                Console.WriteLine("NEGATIVE ONE ID RETURNED");
                return -1;
            }
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
            Character c = player.GetCharacter();
            return c.CharacterName;
        }

        public static string GetAdminName(Client player)
        {
            Account account = player.GetAccount();
            return account.AdminName;
        }

        public static int getVIPPaycheckBonus(Client player)
        {
            Account account = player.GetAccount();

            if (account.VipLevel == 1) { return Properties.Settings.vipbonuslevelone; }
            if (account.VipLevel == 2) { return Properties.Settings.vipbonusleveltwo; }
            if (account.VipLevel == 3) { return Properties.Settings.vipbonuslevelthree; }
            else { return 0; }
        }

        public static int getFactionBonus(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None) { return 0; }

            if (Properties.Settings.governmentbalance * character.Group.FundingPercentage / 100 - character.Group.FactionPaycheckBonus < 0 && character.Group.FundingPercentage != -1)
            {
                return 0;
            }

            return character.Group.FactionPaycheckBonus;

        }

        public static int CalculatePaycheck(Client player)
        {
            Character character = player.GetCharacter();
            return basepaycheck - (Properties.Settings.basepaycheck * Properties.Settings.taxationamount/100) + /*(Properties.Settings.basepaycheck * getVIPPaycheckBonus(player)/100) +*/ getFactionBonus(player) + character.BankBalance/1000;
        }

        private Timer _payCheckTimer;
        private void _payCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var player in Players.Select(x => x.Client))
            {
                Character character = player.GetCharacter();
                if (!API.Shared.IsPlayerConnected(player))
                {
                    LogManager.Log(LogManager.LogTypes.Connection, $"ERROR: PLAYER {character.CharacterName} IS NOT LOGGED IN WHILE IN LIST");
                    continue;
                }

                if (character?.GetTimePlayed() % 3600 == 0 && character.TimePlayed > 0)
                {
                    int paycheckAmount = CalculatePaycheck(player);
                    character.BankBalance += paycheckAmount;
                    Properties.Settings.governmentbalance += paycheckAmount * taxationAmount / 100;
                    player.SendChatMessage("--------------PAYCHECK RECEIVED!--------------");
                    player.SendChatMessage("Base paycheck: $" + basepaycheck + ".");
                    player.SendChatMessage("Interest: $" + character.BankBalance / 1000 + ".");
                    player.SendChatMessage("You were taxed at " + taxationAmount + "%.");
                    //player.SendChatMessage("VIP bonus: " + getVIPPaycheckBonus(player) + "%.");
                    player.SendChatMessage("Faction bonus: $" + getFactionBonus(player) + ".");
                    player.SendChatMessage("----------------------------------------------");
                    player.SendChatMessage("Total: ~g~$" + paycheckAmount + "~w~.");

                    player.SendPictureNotificationToPlayer("Your paycheck for ~g~$" + paycheckAmount + " ~w~has been added to your balance.", "CHAR_BANK_MAZE", 0, 0, "Maze Bank", "Paycheck Received!");
                    Account account = player.GetAccount();
                    if (account.VipLevel > 0 && account.AdminLevel < 1)
                    {
                        if (account.VipExpirationDate < DateTime.Now)
                        {
                            player.SendChatMessage(
                                "Your ~y~VIP~w~ subscription has ran out. Visit www.mt-gaming.com to renew your subscription.");
                            account.VipLevel = 0;
                        }
                    }

                    account.TotalPlayingHours++;
                    account.Save();
                    character.Save();
                }
            }
        }

        [Command("getid", GreedyArg = true, Alias = "id"), Help(HelpManager.CommandGroups.General, "Used to find the ID of specific player name.", new [] {"Name of the target character. (Partial name accepted)"})]
        public void getid_cmd(Client sender, string playerName)
        {
            API.SendChatMessageToPlayer(sender, Color.White, "----------- Searching for: " + playerName + " -----------");
            foreach(var c in Players)
            {
                if(c.CharacterName.StartsWith(playerName, StringComparison.OrdinalIgnoreCase))
                {
                    API.SendChatMessageToPlayer(sender, Color.Grey, c.CharacterName + " - ID " + GetPlayerId(c));
                }
            }
            API.SendChatMessageToPlayer(sender, Color.White, "------------------------------------------------------------");
        }

        [Command("stats"), Help(HelpManager.CommandGroups.General, "Used to find your character statistics", new []{"ID of target character. <strong>[ADMIN ONLY]</strong>"})]          //Stats command
        public void GetStatistics(Client sender, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character character = sender.GetCharacter();
            Account account = sender.GetAccount();

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
        [Command("time"), Help(HelpManager.CommandGroups.General, "Used to find the server time, in-game time, various cooldowns, etc.", null)]
        public void CheckTime(Client player)
        {
            Character character = player.GetCharacter();

            API.SendChatMessageToPlayer(player, Color.White, "__________________ TIME __________________");
            API.SendChatMessageToPlayer(player, Color.Grey, "The current server time is: " + DateTime.Now.ToString("h:mm:ss tt"));
            API.SendChatMessageToPlayer(player, Color.Grey, $"The current in-game time is: {TimeWeatherManager.Hours:D2}:{TimeWeatherManager.Minutes:D2}");
            API.SendChatMessageToPlayer(player, Color.Grey,
                $"Time until next paycheck: { (int)(3600 - (character.GetTimePlayed() % 3600)) / 60}" + " minutes.");
            API.SendChatMessageToPlayer(player, Color.White, "__________________ TIME __________________");
        }

        [Command("dimreset"), Help(HelpManager.CommandGroups.General, "Reset your dimension.", null)]
        public void dimreset_cmd(Client player)
        {
            API.SetEntityDimension(player, 0);
            player.SendChatMessage("Dimension reset.");
        }

        [Command("attempt", GreedyArg = true), Help(HelpManager.CommandGroups.Roleplay, "Attempt to do something with a 50% chance of either success or fail.", "The attempt message")]
        public void attempt_cmd(Client player, string message)
        {
            Character character = player.GetCharacter();

            Random ran = new Random();
            var chance = ran.Next(100);

            string val = (chance <= 50) ? "succeeded" : "failed";

            ChatManager.RoleplayMessage(character, $"attempted to {message} and {val}.", ChatManager.RoleplayMe);
        }

        //Show player stats (admins can show stats of other players).
        public void ShowStats(Client sender)
        {
            ShowStats(sender, sender);
        }
 
        public void ShowStats(Client sender, Client receiver)
        {
            Character character = receiver.GetCharacter();
            Account account = receiver.GetAccount();
            Account senderAccount = sender.GetAccount();
            var playerveh = VehicleManager.GetVehFromNetHandle(API.GetPlayerVehicle(receiver))?.Id.ToString() ?? "None";

            API.SendChatMessageToPlayer(sender, "==============================================");
            API.SendChatMessageToPlayer(sender, "Player statistics for " + character.CharacterName);
            API.SendChatMessageToPlayer(sender, "==============================================");
            API.SendChatMessageToPlayer(sender, "~g~General:~g~");
            API.SendChatMessageToPlayer(sender,
                $"~h~Character name:~h~ {character.CharacterName} | ~h~ID:~h~ {character.Id} | ~h~Money:~h~ {Money.GetCharacterMoney(character)} | ~h~Bank balance:~h~ {character.BankBalance} | ~h~Playing hours:~h~ {character.GetPlayingHours()}  | ~h~Total hours:~h~ {account.TotalPlayingHours}");

            API.SendChatMessageToPlayer(sender,
                $"~h~Age:~h~ {character.Age} ~h~Birthplace:~h~ {character.Birthplace} ~h~Birthday:~h~ {character.Birthday} ~h~VIP level:~h~ {account.VipLevel} ~h~VIP expires:~h~ {account.VipExpirationDate}");

            API.SendChatMessageToPlayer(sender, "~b~Faction/Jobs:~b~");
            API.SendChatMessageToPlayer(sender,
                $"~h~Faction ID:~h~ {character.GroupId} ~h~Rank:~h~ {character.GroupRank} ~h~Group name:~h~ {character.Group.Name} ~h~Job 1:~h~ {character.JobOne.Name}");

            API.SendChatMessageToPlayer(sender, "~r~Property:~r~");
            API.SendChatMessageToPlayer(sender, $"~h~Owned vehicles:~h~ {character.OwnedVehicles.Count()}");

            if (senderAccount.AdminLevel > 0)
            {
                API.SendChatMessageToPlayer(sender, "~y~Admin:~y~");
                API.SendChatMessageToPlayer(sender,
                    $"~h~Admin level:~h~ {account.AdminLevel} ~h~Admin name:~h~ {account.AdminName} ~h~Dimension:~h~ {API.GetEntityDimension(receiver)} ~h~Last IP:~h~ {account.LastIp}");
                API.SendChatMessageToPlayer(sender, $"~h~Current vehicle:~h~{playerveh} ~h~Last vehicle: ~h~ { character?.LastVehicle?.Id}");
                API.SendChatMessageToPlayer(sender,
                    $"~h~Social Club Name:~h~ {account.AccountName} ~h~Admin actions: {account.AdminActions}");
            }
        }

        public static void ShowStats(Client sender, Character receiver, Account receiverAcc)
        {
            Account senderAccount = sender.GetAccount();

            API.Shared.SendChatMessageToPlayer(sender, "==============================================");
            API.Shared.SendChatMessageToPlayer(sender, "Player statistics for " + receiver.CharacterName);
            API.Shared.SendChatMessageToPlayer(sender, "==============================================");
            API.Shared.SendChatMessageToPlayer(sender, "~g~General:~g~");
            API.Shared.SendChatMessageToPlayer(sender,
                $"~h~receiver name:~h~ {receiver.CharacterName} | ~h~ID:~h~ {receiver.Id} | ~h~Money:~h~ {Money.GetCharacterMoney(receiver)} | ~h~Bank balance:~h~ {receiver.BankBalance} | ~h~Playing hours:~h~ {receiver.GetPlayingHours()}  | ~h~Total hours:~h~ {receiverAcc.TotalPlayingHours}");

            API.Shared.SendChatMessageToPlayer(sender,
                $"~h~Age:~h~ {receiver.Age} ~h~Birthplace:~h~ {receiver.Birthplace} ~h~Birthday:~h~ {receiver.Birthday} ~h~VIP level:~h~ {receiverAcc.VipLevel} ~h~VIP expires:~h~ {receiverAcc.VipExpirationDate}");

            API.Shared.SendChatMessageToPlayer(sender, "~b~Faction/Jobs:~b~");
            API.Shared.SendChatMessageToPlayer(sender,
                $"~h~Faction ID:~h~ {receiver.GroupId} ~h~Rank:~h~ {receiver.GroupRank} ~h~Group name:~h~ {receiver.Group.Name} ~h~Job 1:~h~ {receiver.JobOne.Name}");

            API.Shared.SendChatMessageToPlayer(sender, "~r~Property:~r~");
            API.Shared.SendChatMessageToPlayer(sender, $"~h~Owned vehicles:~h~ {receiver.OwnedVehicles.Count()}");

            if (senderAccount.AdminLevel > 0)
            {
                API.Shared.SendChatMessageToPlayer(sender, "~y~Admin:~y~");
                API.Shared.SendChatMessageToPlayer(sender,
                    $"~h~Admin level:~h~ {receiverAcc.AdminLevel} ~h~Admin name:~h~ {receiverAcc.AdminName} ~h~Dimension:~h~ {receiver.LastDimension} ~h~Last IP:~h~ {receiverAcc.LastIp}");
                API.Shared.SendChatMessageToPlayer(sender,
                    $"~h~Social Club Name:~h~ {receiverAcc.AccountName} ~h~Admin actions: {receiverAcc.AdminActions}");
            }
        }
    }
}
