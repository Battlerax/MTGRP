using System;
using System.Linq;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.weapon_manager;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lspd;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;

namespace mtgvrp.AdminSystem
{
    public class AdminCommands : Script
    {

        public AdminCommands()
        {
            DebugManager.DebugMessage("[AdminSys] Initalizing Admin System...");
            DebugManager.DebugMessage("[AdminSys] Admin System initalized.");
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        public void OnClientEventTrigger(Client player, string eventId, params object[] arguments)
        {
            switch (eventId)
            {
                case "OnRequestSubmitted":
                    Character character = API.getEntityData(player.handle, "Character");
                    int playerid = PlayerManager.GetPlayerId(character);
                    AdminReports.InsertReport(3, player.nametag, (string)arguments[0]);
                    SendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + playerid + "): " + (string)arguments[0]);
                    API.sendChatMessageToPlayer(player, "Report submitted.");
                    startReportTimer(player);
                    character.HasActiveReport = true;
                    break;

                case "OnReportMade":
                    Character senderchar = API.getEntityData(player.handle, "Character");
                    int senderid = PlayerManager.GetPlayerId(senderchar);
                    string id = (string)arguments[1];
                    var receiver = PlayerManager.ParseClient(id);
                    if (receiver == null)
                    {
                        API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                        return;
                    }
                    AdminReports.InsertReport(2, player.nametag, (string)arguments[0], PlayerManager.GetName(receiver) + " (ID:" + id + ")");
                    SendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + senderid + ")" + " reported " + PlayerManager.GetName(receiver) + " (ID:" + id + ") for " + (string)arguments[0]);
                    API.sendChatMessageToPlayer(player, "Report submitted.");
                    startReportTimer(player);
                    senderchar.HasActiveReport = true;
                    break;

                case "teleport":
                    Vector3 pos = (Vector3) arguments[0];
                    player.position = pos;
                    break;

            }
        }

        [Command("set", GreedyArg = true)]
        public void SetCharacterData(Client player, string target, string var, string value)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                var receiver = PlayerManager.ParseClient(target);
                if (receiver == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                    return;
                }

                var recChar = receiver.GetCharacter();
                var prop = recChar.GetType().GetProperties().SingleOrDefault(x => x.Name == var);
                if (prop == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White, "There is no such property.");
                    return;
                }

                if (prop.PropertyType == typeof(int))
                {
                    int val;
                    if (!int.TryParse(value, out val))
                    {
                        API.sendChatMessageToPlayer(player, "That property is an integer.");
                        return;
                    }

                    prop.SetValue(recChar, val);
                    API.sendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    bool val;
                    if (!bool.TryParse(value, out val))
                    {
                        API.sendChatMessageToPlayer(player, "That property is a bool.");
                        return;
                    }

                    prop.SetValue(recChar, val);
                    API.sendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(recChar, value);
                    API.sendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "Unknown Type.");
                }
            }
        }

        [Command("setadminlevel")]
        public void setrank_cmd(Client player, string id, int level)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");
            Account receiverAccount = API.shared.getEntityData(receiver.handle, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.shared.getEntityData(receiver.handle, "Character");

            if (account.AdminLevel < 7)
                return;

            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            if (account.AdminLevel >= receiverAccount.AdminLevel && account.AdminLevel > level)
            {
                var oldLevel = receiverAccount.AdminLevel;
                if (oldLevel > level)
                {
                    API.sendChatMessageToPlayer(receiver, "You have been demoted to admin level " + level + " by " + character.CharacterName + ".");
                }
                else
                {
                    API.sendChatMessageToPlayer(receiver, "You have been promoted to admin level " + level + " by " + character.CharacterName + ".");
                }
                API.sendChatMessageToPlayer(player, "You have changed " + receiverCharacter.CharacterName + "'s admin level to " + level + " (was " + oldLevel + ").");
                receiverAccount.AdminLevel = level;
                receiverAccount.Save();
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "You cannot set a higher admin level than yours or set someone to a level above yours.");
            }
        }

        [Command("makemeleader")]
        public void makemeleader(Client player, string playerid, int groupId)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 5)
            {
                return;
            }

            var leaderClient = PlayerManager.ParseClient(playerid);
            if (leaderClient == null)
            {
                API.sendChatMessageToPlayer(player, Color.Grey, "~r~[ERROR]~w~ That player is not online.");
                return;
            }

            var leaderChar = leaderClient.GetCharacter();

            var group = GroupManager.GetGroupById(groupId);
            if (group == Group.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ That group ID is not valid.");
                return;
            }

            leaderChar.Group = group;
            leaderChar.GroupId = group.Id;
            leaderChar.GroupRank = 10;
            leaderChar.Save();

            API.sendChatMessageToPlayer(leaderClient, Color.White, "You have been made the leader of " + group.Name);
            API.sendChatMessageToPlayer(player, Color.Grey, "You have made " + leaderChar.CharacterName + " the leader of " + group.Name);

            GroupManager.SendGroupMessage(player,
                leaderChar.CharacterName + " has joined the group. (Made leader by " + player.GetCharacter().CharacterName + ")");
        }

        [Command("gotopos")]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            var pos = new Vector3(x, y, z);
            API.setEntityPosition(player, pos);
            API.sendChatMessageToPlayer(player, "Teleported");
        }

        [Command("sendback")]
        public static void sendback_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character receiverCharacter = API.shared.getEntityData(receiver.handle, "Character");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            var playerPos = receiverCharacter.LastPos;
            API.shared.setEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendNotificationToPlayer(player, "You teleported ~b~" + PlayerManager.GetName(receiver) + " (ID:" + id + ")~w~ back to their previous position.");
            API.shared.sendNotificationToPlayer(receiver, "You were teleported back to your original position by ~b~" + PlayerManager.GetName(player) + "~w~.");

        }

        [Command("get")]
        public static void get_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character receiverCharacter = API.shared.getEntityData(receiver.handle, "Character");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.shared.getEntityPosition(player);
            var targetPos = API.shared.getEntityPosition(receiver);
            receiverCharacter.LastPos = targetPos;
            API.shared.setEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendNotificationToPlayer(player, "You teleported ~b~" + PlayerManager.GetName(receiver) + " (ID:" + id + ")~w~ to your position.");
            API.shared.sendNotificationToPlayer(receiver, "You were teleported to ~b~" + PlayerManager.GetName(player) + "~w~.");

        }

        [Command("adminwarp", Alias = "aw", GreedyArg = true)]
        public void adminwarp_cmd (Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            //WILL FINISH ONCE WE ARE SURE ON SPAWN POINTS ETC. FOR NOW IT'S THE TRAIN SPAWN:
            API.setEntityPosition(receiver, new Vector3(433.2354, -645.8408, 28.7263));
            API.sendChatMessageToPlayer(receiver, "You have been admin warped to the spawn by an admin.");
            API.sendChatMessageToPlayer(player, "You have admin warped " + receiver.nametag + " to the spawn.");
        }

        [Command("goto")]
        public static void goto_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.shared.getEntityPosition(receiver);
            API.shared.setEntityPosition(player, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendChatMessageToPlayer(player, "You have teleported to " + PlayerManager.GetName(receiver) + " (ID:" + id + ").");

        }

        [Command("agiveweapon")]
        public void agiveweapon_cmd(Client player, string id, WeaponHash weaponHash)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            WeaponManager.CreateWeapon(receiver, weaponHash, WeaponTint.Normal, false, true);
            API.sendChatMessageToPlayer(player, "You have given Player ID: " + id + " a " + weaponHash);
        }

        [Command("sethealth")]
        public void sethealth_cmd(Client player, string id, int health)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.setPlayerHealth(receiver, health);
            API.sendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s health to " + health + ".");
        }

        [Command("setarmour")]
        public void setarmour_cmd(Client player, string id, int armour)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.setPlayerArmor(receiver, armour);
            API.sendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s armour to " + armour + ".");
        }

        [Command("setvehiclehp")]
        public void SetVehHP_cmd(Client player, int vehid, int health)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            var receiver = VehicleManager.Vehicles.SingleOrDefault(x => x.Id == vehid);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid vehicleid entered.");
                return;
            }

            if (receiver.IsSpawned == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Vehicle not spawned.");
                return;
            }

            API.setVehicleHealth(receiver.NetHandle, health);
            API.sendChatMessageToPlayer(player, "You have set Vehicle ID: " + receiver.Id + "'s health to " + health + ".");
        }

        [Command("spec")]
        public static void spec_cmd(Client player, string id)
        {
            var target = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (target == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            account.IsSpectating = true;
            API.shared.setPlayerToSpectatePlayer(player, target);
            API.shared.setPlayerNametagVisible(player, false);
            API.shared.setEntityTransparency(player, 0);
            API.shared.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.GetName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");

        }

        [Command("specoff")]
        public void specoff_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (account.IsSpectating == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not specing anyone.");
                return;
            }
            account.IsSpectating = false;
            API.unspectatePlayer(player);
            API.shared.setPlayerNametagVisible(player, true);
            API.shared.setEntityTransparency(player, 255);
            API.sendChatMessageToPlayer(player, "You are no longer spectating anyone.");
        }

        [Command("slap")]
        public void slap_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.getEntityPosition(receiver);
            API.setEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y, playerPos.Z + 5));
            API.sendChatMessageToPlayer(receiver, "You have been slapped by an admin");
        }

        [Command("freeze")]
        public void freeze_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.freezePlayer(receiver, true);
            API.sendChatMessageToPlayer(receiver, "You have been frozen by an admin");
        }

        [Command("gotowaypoint")]
        public void gotowaypoint_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            API.triggerClientEvent(player, "getwaypoint");
        }

        [Command("unfreeze")]
        public void unfreeze_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.freezePlayer(receiver, false);
            API.sendChatMessageToPlayer(receiver, "You have been unfrozen by an admin");
        }

        [Command("quitadmin")]
        public void QuitAdmin_cmd(Client player, int money)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 0)
                return;

            account.AdminLevel = 0;
            account.Save();
            API.sendChatMessageToPlayer(player, "You have quit admin.");
        }

        [Command("setmymoney")]
        public void setmymoney_cmd(Client player, int money)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel <= 6)
                return;

            InventoryManager.SetInventoryAmmount(character, typeof(Money), money);
            API.sendChatMessageToPlayer(player, $"You have sucessfully changed your money to ${money}.");
        }

        [Command("showplayercars")]
        public void showplayercars_cmd(Client player, string id)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = API.getEntityData(receiver.handle, "Character");
            API.sendChatMessageToPlayer(player, "----------------------------------------------");
            API.sendChatMessageToPlayer(player, $"Vehicles Owned By {character.CharacterName}");
            foreach (var carid in character.OwnedVehicles)
            {
                var car = VehicleManager.Vehicles.SingleOrDefault(x => x.Id == carid);
                if (car == null)
                {
                    API.sendChatMessageToPlayer(player, $"(UNKNOWN VEHICLE) | ID ~r~{carid}~w~.");
                    continue;
                }
                API.sendChatMessageToPlayer(player, $"({API.getVehicleDisplayName(car.VehModel)}) | NetHandle ~r~{car.NetHandle.Value}~w~ | ID ~r~{car.Id}~w~.");
            }
            API.sendChatMessageToPlayer(player, "----------------------------------------------");
        }

        [Command("getplayercar")]
        public void getplayercar_cmd(Client player, string id, int nethandle)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = API.getEntityData(player.handle, "Character");
            var car = VehicleManager.Vehicles.Find(x => x.NetHandle.Value == nethandle && x.OwnerId == character.Id);
            API.setEntityPosition(car.NetHandle, player.position);
            API.sendChatMessageToPlayer(player, "Sucessfully teleported the car to you.");
        }

        [Command("setadminname")]
        public void setadminname_cmd(Client player, string id, string name)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Account receiverAccount = API.getEntityData(receiver.handle, "Account");
            if (account.AdminLevel < 6)
                return;

            if (receiverAccount.AdminLevel == 0)
            {
                API.sendChatMessageToPlayer(player, "This player is not an admin.");
                return;
            }

            receiverAccount.AdminName = name;
            API.sendChatMessageToPlayer(player, "You have set " + receiver.nametag + "'s admin name to '" + name + "'.");
            API.sendChatMessageToPlayer(receiver, receiver.nametag + " has set your admin name to '" + name + "'.");
        }

        [Command("admins")]
        public void admins_cmd(Client player)
        {
            API.sendChatMessageToPlayer(player, "=====ADMINS ONLINE NOW=====");
            foreach (var c in API.getAllPlayers())
            {
                Account receiverAccount = API.getEntityData(c.handle, "Account");

                if (receiverAccount.AdminLevel > 1 && receiverAccount.AdminDuty == true)
                {
                    API.sendChatMessageToPlayer(player, "~g~" + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                }
            }
        }

        [Command("adminduty")]
        public void adminduty_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 1)
                return;

            Character character = API.getEntityData(player, "Character");

            if (account.AdminDuty == false)
            {
                account.AdminDuty = true;
                API.setPlayerNametagColor(player, 51, 102, 255);
                API.setPlayerNametag(player, account.AdminName + " (" + character.Id + ")");
                API.sendChatMessageToPlayer(player, "You are now on admin duty.");
                return;
            }

            account.AdminDuty = false;
            player.nametag = character.CharacterName + " (" + character.Id + ")";
            API.resetPlayerNametagColor(player);
            API.sendChatMessageToPlayer(player, "You are no longer on admin duty.");

        }


        //============REPORT SYSTEM=============

        [Command("report", Alias = "re", GreedyArg = true)]
        public void report_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.ReportMuteExpires > DateTime.Now)
            {
                API.sendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (AdminReports.Reports.Count > 5)
            {
                API.sendChatMessageToPlayer(player, "~r~We are experiencing a high volume of reports. Please only use the report feature if it is absolutely necessary.");
            }

            if (character.ReportCreated == true)
            {
                API.sendChatMessageToPlayer(player, "Please wait 15 seconds before creating another report.");
                return;
            }
            API.triggerClientEvent(player, "show_report_menu");

        }

        [Command("reports", GreedyArg = true)]
        public void reports_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;


            API.sendChatMessageToPlayer(player, "=======REPORTS=======");
            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Type == 1)
                {
                    return;
                }

                if (i.Type == 2)
                {
                    API.sendChatMessageToPlayer(player, "~b~" + i.Name + " reported ~r~" + i.Target + "~w~" + " | " + i.ReportMessage);
                    return;
                }
                API.sendChatMessageToPlayer(player, "~b~" + i.Name + "~w~" + " | " + i.ReportMessage);

            }
        }

        [Command("acceptreport", GreedyArg = true)]
        public void acceptreport_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 2)
                return;


            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveReport == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active reports.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.nametag)
                {
                    AdminReports.Delete(i);
                }
            }

            receivercharacter.HasActiveReport = false;
            API.sendChatMessageToPlayer(player, "Report accepted.");
            API.sendChatMessageToPlayer(receiver, "Your report has been taken by ~b~" + player.nametag + ".");
            SendtoAllAdmins("[REPORT] " + player.nametag + " has taken " + receiver.nametag + "'s report.");
            character.AdminActions++;
        }

        [Command("trashreport", GreedyArg = true)]
        public void trashreport_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;


            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveReport == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active reports.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.nametag)
                {
                    AdminReports.Delete(i);
                }
            }
            receivercharacter.HasActiveReport = false;
            API.sendChatMessageToPlayer(player, "Request trashed.");
            API.sendChatMessageToPlayer(receiver, "Your admin/moderator request was trashed.");
            SendtoAllAdmins("[TRASHED] " + player.nametag + " trashed " + receiver.nametag + "'s report.");
        }

        [Command("maccept", GreedyArg = true)]
        public void maccept_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;

            if (character.IsOnAsk == true)
            {
                API.sendChatMessageToPlayer(player, "You are already helping someone.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveAsk == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active help requests.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.nametag)
                {
                    AdminReports.Delete(i);
                }
            }

            character.IsOnAsk = true;
            character.LastPos = API.getEntityPosition(player);
            receivercharacter.HasActiveAsk = false;
            API.sendChatMessageToPlayer(player, "Ask accepted.");
            API.sendChatMessageToPlayer(receiver, "Your help request has been taken by ~b~" + player.nametag + ".");
            SendtoAllAdmins("[REPORT] " + player.nametag + " has taken " + receiver.nametag + "'s ask request.");
            API.setEntityPosition(player, API.getEntityPosition(receiver));
            API.setPlayerNametagColor(player, 51, 102, 255);
            character.AdminActions++;
        }

        [Command("mfinish", GreedyArg = true)]
        public void mfinish_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel < 1)
                return;

            if (character.IsOnAsk == false)
            {
                API.sendChatMessageToPlayer(player, "You are not helping anyone.");
                return;
            }

            character.IsOnAsk = false;
            API.sendChatMessageToPlayer(player, "Ask finished.");
            API.setEntityPosition(player, character.LastPos);
            API.resetPlayerNametagColor(player);
        }

        [Command("ask", GreedyArg = true)]
        public void ask_cmd(Client player, string message)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.ReportMuteExpires > DateTime.Now)
            {
                API.sendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (character.ReportCreated == true)
            {
                API.sendChatMessageToPlayer(player, "Please wait 15 seconds before creating another request.");
                return;
            }

            character.HasActiveAsk = true;
            AdminReports.InsertReport(1, player.nametag, message);
            SendtoAllAdmins("~g~[ASK]~w~ " + PlayerManager.GetName(player) + ": " + message);
            API.sendChatMessageToPlayer(player, "~b~Ask request submitted. ~w~Moderators have been informed and will be with you soon.");
            startReportTimer(player);
        }

        [Command("nmute", GreedyArg = true)]
        public void nmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            int result = DateTime.Compare(receivercharacter.NMutedExpiration, DateTime.Now);

            if (result <= 0)
            {
                API.sendChatMessageToPlayer(player, "You have muted ~b~" + receiver.nametag + "~w~ from newbie chat for 1 hour.");
                API.sendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from newbie chat for 1 hour.");
                receivercharacter.NMutedExpiration = DateTime.Now.AddHours(1);
                return;
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from newbie chat.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~ from newbie chat.");
                receivercharacter.NMutedExpiration = DateTime.Now;
            }

        }

        [Command("vmute", GreedyArg = true)]
        public void vmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            int result = DateTime.Compare(receivercharacter.VMutedExpiration, DateTime.Now);

            if (result <= 0)
            {
                API.sendChatMessageToPlayer(player, "You have muted ~b~" + receiver.nametag + "~w~ from VIP chat for 1 hour.");
                API.sendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from VIP chat for 1 hour.");
                receivercharacter.VMutedExpiration = DateTime.Now.AddHours(1);
                return;
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from VIP chat.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~ from VIP chat.");
                receivercharacter.VMutedExpiration = DateTime.Now;
            }

        }

        [Command("reportmute", GreedyArg = true)]
        public void reportmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            int result = DateTime.Compare(receivercharacter.ReportMuteExpires, DateTime.Now);

            if (result <= 0)
            {
                API.sendChatMessageToPlayer(player, "You have muted ~b~" + receiver.nametag + "~w~from creating reports.");
                API.sendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from making reports.");
                receivercharacter.ReportMuteExpires = DateTime.Now.AddHours(1);
                return;
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from making reports.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~from creating reports.");
                receivercharacter.ReportMuteExpires = DateTime.Now;
            }

        }

        [Command("mlist", GreedyArg = true)]
        public void asklist_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 1)
                return;

            API.sendChatMessageToPlayer(player, "=======ASK LIST=======");
            foreach (var i in AdminReports.Reports)
            {
                if (i.Type == 2 || i.Type == 3)
                {
                    return;
                }

                API.sendChatMessageToPlayer(player, "~b~" + i.Name + "~w~" + " | " + i.ReportMessage);
            }
        }

        [Command("clearreports", GreedyArg = true)]
        public void clearreports_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 3)
                return;

            AdminReports.Reports.Clear();
            API.sendChatMessageToPlayer(player, "All reports (including ask) have been cleared.");
        }

        //PLAYER-ADMIN STUFF


        [Command("prison", GreedyArg = true)]
        public void prison_cmd(Client player, string id, string time)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            Lspd.JailControl(receiver, int.Parse(time));
            API.sendChatMessageToPlayer(player, "You have jailed " + receiver.nametag + " for " + time + " seconds.");
            API.sendChatMessageToPlayer(receiver, "You have been jailed by " + player.nametag + " for " + time + " seconds.");
        }

        [Command("kickplayer", GreedyArg = true)]
        public static void kick_cmd(Client player, string id, string reason)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = API.shared.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            KickPlayer(receiver, reason);
        }

        [Command("remotewarn", GreedyArg = true)]
        public static void remotewarn_cmd(Client player, string accountname, string reason)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (account.AdminLevel < 3)
                return;

            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    if (c.IsLoggedIn)
                    {
                        API.shared.sendChatMessageToPlayer(player, "This player is logged in. Use /warn.");
                        return;
                    }
                    var playerWarn = new PlayerWarns(c.AccountName, account.AccountName, reason);
                    c.PlayerWarns.Add(playerWarn);
                    character.AdminActions++;

                    if (c.PlayerWarns.Count() >= 3)
                    {
                        c.TempbanLevel++;
                        if (c.TempbanLevel >= 3)
                        {
                            c.IsBanned = true;
                            c.BanReason = "Reached a tempban level of 3";
                        }
                        else
                        {
                            if (c.TempbanLevel == 1)
                            {
                                c.TempBanExpiration = DateTime.Now.AddDays(3);
                                c.IsTempbanned = true;

                            }
                            if (c.TempbanLevel == 2)
                            {
                                c.TempBanExpiration = DateTime.Now.AddDays(7);
                                c.IsTempbanned = true;
                            }
                        }
                        c.PlayerWarns.Clear();
                        API.shared.sendChatMessageToPlayer(player, "Player has been tempbanned.");
                    }
                    c.Save();
                }
                API.shared.sendChatMessageToPlayer(player, "You remotewarned " + c.AccountName + " for '~r~" + reason + "~w~'.");
                break;
            }
        }


        [Command("remoteban", GreedyArg = true)]
        public static void remoteban_cmd(Client player, string accountname, string reason)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    c.IsBanned = true;
                    c.BanReason = reason;
                    c.Save();
                }
                API.shared.sendChatMessageToPlayer(player, "You have remote-banned " + c.AccountName+ " from the server.");
                break;
            }
        }

        [Command("getaccountname", GreedyArg = true)]
        public void getaccountname_cmd(Client player, string charactername)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            var filter = Builders<Character>.Filter.Eq("CharacterName", charactername);
            var foundCharacter = DatabaseManager.CharacterTable.Find(filter).ToList();

            foreach (var p in foundCharacter)
            {
                var accountFilter = Builders<Account>.Filter.Eq("Id", p.AccountId);
                var foundAccount = DatabaseManager.AccountTable.Find(accountFilter).ToList();

                foreach (var j in foundAccount)
                {
                    API.sendChatMessageToPlayer(player, charactername + "'s account name is '" + j.AccountName + "'.");
                }
            }
        }

        [Command("unban", GreedyArg = true)]
        public static void unban_cmd(Client player, string accountname)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    c.IsBanned = false;
                    c.BanReason = "";
                    c.Save();
                }
                API.shared.sendChatMessageToPlayer(player, "You have unbanned " + c.AccountName + " from the server.");
                break;
            }
        }

        [Command("untempban", GreedyArg = true)]
        public static void untempban_cmd(Client player, string accountname)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    c.IsTempbanned = false;
                    c.TempbanLevel -= 1;
                    c.Save();
                }
                API.shared.sendChatMessageToPlayer(player, "You have un-tempbanned " + c.AccountName + " from the server.");
                break;
            }
        }

        [Command("banplayer", GreedyArg = true)]
        public static void ban_cmd(Client player, string id, string reason)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            BanPlayer(receiver, reason);


        }


        [Command("warn", GreedyArg = false)]
        public static void warn_cmd(Client player, string id, string reason)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            if(receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid target.");
                return;
            }
            Account receiverAccount = API.shared.getEntityData(receiver, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.shared.getEntityData(receiver, "Character");

            var playerWarn = new PlayerWarns(receiverAccount.AccountName, account.AccountName, reason);
            receiverAccount.PlayerWarns.Add(playerWarn);
            character.AdminActions++;

            API.shared.sendChatMessageToPlayer(player, "You warned ~r~" + receiverCharacter.CharacterName + "~w~ for '~r~" + reason + "~w~'.");
            API.shared.sendChatMessageToPlayer(receiver, "You were warned by ~r~" + character.CharacterName + "~w~ for '~r~" + reason + "~w~'.");


            if (receiverAccount.PlayerWarns.Count() >= 3)
            {
                API.shared.sendNotificationToPlayer(player, "You were temporarily banned for reaching 3 player warns.");
                AddTempBanLevel(receiver);
                TempBanPlayer(receiver);
                receiverAccount.PlayerWarns.Clear();
                API.shared.kickPlayer(receiver);
            }
        }

        [Command("removewarns", GreedyArg = false)]
        public static void removewarns_cmd(Client player, string id, string reason)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
                return;

            Account receiverAccount = API.shared.getEntityData(receiver.handle, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.shared.getEntityData(receiver.handle, "Character");

            foreach (var w in receiverAccount.PlayerWarns)
            {
                receiverAccount.PlayerWarns.Remove(w);
            }

            API.shared.sendChatMessageToPlayer(player, "You removed all warns from ~r~" + receiverCharacter.CharacterName + "~w~.");
            API.shared.sendChatMessageToPlayer(receiver, "Your warns were removed by ~r~" + character.CharacterName + "~w~.");

        }

        [Command("remoteplayerwarns")]
        public static void remoteplayerwarns_cmd(Client player, string accountname)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    int j = 1;
                    foreach (var warn in c.PlayerWarns)
                    {
                        API.shared.sendChatMessageToPlayer(player, "Warning #" + j + " | ~r~Reason:~w~ " + warn.WarnReason + " | ~r~Given by:~w~ " + warn.WarnSender);
                        j++;
                    }
                    if (c.PlayerWarns.Count == 0)
                    {
                        API.shared.sendChatMessageToPlayer(player, "No warnings to show.");
                    }
                    API.shared.sendChatMessageToPlayer(player, "~r~Tempban level:~w~ " + c.TempbanLevel);

                }
                break;
            }
        }


        [Command("playerwarns", GreedyArg = false)]
        public static void playerwarns_cmd(Client player, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2 && id != null)
            {
                return;
            }

            else if (id == null)
            {
                ShowWarns(player);
                return;
            }

            else if (account.AdminLevel >= 2 && id != null)
            {
                ShowWarns(player, receiver);
                return;
            }
        }
    

        public static void ShowWarns(Client player, Client receiver = null)
        {

            Account account = API.shared.getEntityData(player.handle, "Account");
            if (receiver != null)
            {
                account = API.shared.getEntityData(receiver.handle, "Account");
            }


            int j = 1;
            foreach (var warn in account.PlayerWarns)
            {
                API.shared.sendChatMessageToPlayer(player, "Warning #" + j + " | ~r~Reason:~w~ " + warn.WarnReason + " | ~r~Given by:~w~ " + warn.WarnSender);
                    j++;
            }
            if(account.PlayerWarns.Count == 0)
            {
                API.shared.sendChatMessageToPlayer(player, "No warnings to show.");
            }
            API.shared.sendChatMessageToPlayer(player, "~r~Tempban level:~w~ " + account.TempbanLevel);
        }

        public static void AddTempBanLevel(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            Account account = API.shared.getEntityData(player.handle, "Account");

            account.TempbanLevel += 1;
            if (account.TempbanLevel >= 3)
            {
                BanPlayer(player, "You reached the maximum tempban level (3)");
            }
        }

        public static void TempBanPlayer(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            Account account = API.shared.getEntityData(player.handle, "Account");

            if(account.TempbanLevel == 1)
            {
                API.shared.sendChatMessageToPlayer(player, "3 player warns reached. You have been temporarily banned for 3 days");
                account.TempBanExpiration = DateTime.Now.AddDays(3);
                account.IsTempbanned = true;
                return;
            }
            if(account.TempbanLevel == 2)
            {
                API.shared.sendChatMessageToPlayer(player, "3 player warns reached. You have been temporarily banned for 7 days");
                account.TempBanExpiration = DateTime.Now.AddDays(7);
                account.IsTempbanned = true;
                return;
            }
        }

        public static void KickPlayer(Client player, string reason)
        {
            API.shared.kickPlayer(player);
            API.shared.sendNotificationToPlayer(player, "You were kicked from the server for ~r~'" + reason + "~w~'.");
            API.shared.sendChatMessageToPlayer(player, "You were kicked from the server for ~r~'" + reason + "~w~'.");
        }

        public static void BanPlayer(Client player, string reason)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            account.BanReason = reason;
            account.IsBanned = true;
            API.shared.sendNotificationToPlayer(player, "You were banned from the server for ~r~'" + reason + "~w~'.");
            API.shared.sendChatMessageToPlayer(player, "You were banned from the server for ~r~'" + reason + "~w~'.");
            API.shared.kickPlayer(player);
        }

        public static void UnbanPlayer(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            account.IsBanned = false;
        }

        [Command("changeviplevel", GreedyArg = true)]
        public void changeviplevel_cmd(Client player, string id, int level, int days)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Account account = API.getEntityData(player.handle, "Account");
            Account receiverAccount = API.getEntityData(receiver.handle, "Account");

            if (account.AdminLevel < 3)
            {
                return;
            }

            if (receiverAccount.AdminLevel > 0)
            {
                account.VipLevel = 3;
                account.VipExpirationDate = default(DateTime);
                return;
            }

            if (receiverAccount.VipLevel == level)
            {
                player.sendChatMessage("This player is already this VIP level.");
                return;
            }

            account.VipLevel = level;
            account.VipExpirationDate = DateTime.Now.AddDays(days);
            account.Save();

            receiver.sendChatMessage("Your ~y~VIP~y~ level was set to " + level + " by " + account.AdminName + ". Welcome!");
            foreach (var p in API.getAllPlayers())
            {
                Account paccount = API.getEntityData(p.handle, "Account");
                
                if (paccount.VipLevel > 0) { p.sendChatMessage(receiver.GetCharacter().CharacterName + " has become a level " + level + " ~y~VIP~y~!"); }
            }
        }

        [Command("addviptime", GreedyArg = true)]
        public void addviptime_cmd(Client player, string id, string days)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Account account = API.getEntityData(player.handle, "Account");
            Account receiverAccount = API.getEntityData(receiver.handle, "Account");

            if (account.AdminLevel < 3)
            {
                return;
            }

            account.VipExpirationDate = account.VipExpirationDate.AddDays(int.Parse(days));
            account.Save();

            receiver.sendChatMessage("Your ~y~VIP~y~ days were increased by " + int.Parse(days) + " days by " + account.AdminName + "!");
        }

        public void startReportTimer(Client player)
        {
            Character senderchar = API.getEntityData(player.handle, "Character");
            senderchar.ReportCreated = true;
            senderchar.ReportTimer = new Timer() { Interval = 15000 };
            senderchar.ReportTimer.Elapsed += delegate { ReportTimer(player); };
            senderchar.ReportTimer.Start();
        }

        public static void SendtoAllAdmins(string text)
        {
            foreach (var c in API.shared.getAllPlayers())
            {
                Account receiverAccount = API.shared.getEntityData(c.handle, "Account");

                if (receiverAccount.AdminLevel > 0)
                {
                    API.shared.sendChatMessageToPlayer(c, Color.AdminChat, text);
                }
            }
        }

        public void ReportTimer(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            character.ReportCreated = false;
            character.ReportTimer.Stop();
        }
    }
}
