using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using System.Timers;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;

namespace RoleplayServer.resources.AdminSystem
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
                    AdminReports.InsertReport(3, player.nametag + " (ID:" + playerid + ")", (string)arguments[0]);
                    sendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + playerid + "): " + (string)arguments[0]);
                    startReportTimer(player);
                    character.HasActiveReport = true;
                    break;

                case "OnReportMade":
                    Character senderchar = API.getEntityData(player.handle, "Character");
                    int senderid = PlayerManager.GetPlayerId(senderchar);
                    string id = (string)arguments[1];
                    var receiver = PlayerManager.ParseClient(id);
                    AdminReports.InsertReport(2, player.nametag + " (ID:" + senderid + ")", (string)arguments[0], PlayerManager.GetName(receiver) + " (ID:" + id + ")");
                    sendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + senderid + ")" + " reported " + PlayerManager.GetName(receiver) + " (ID:" + id + ") for " + (string)arguments[0]);
                    startReportTimer(player);
                    senderchar.HasActiveReport = true;
                    break;

            }
        }

        public void startReportTimer(Client player)
        {
            Character senderchar = API.getEntityData(player.handle, "Character");
            senderchar.ReportCreated = true;
            senderchar.ReportTimer = new Timer() { Interval = 15000 };
            senderchar.ReportTimer.Elapsed += delegate { reportTimer(player); };
            senderchar.ReportTimer.Start();
        }

        public void sendtoAllAdmins(string text)
        {
            foreach (var c in API.getAllPlayers())
            {
                Account receiverAccount = API.getEntityData(c.handle, "Account");

                if (receiverAccount.AdminLevel > 0)
                {
                    API.sendChatMessageToPlayer(c, Color.AdminChat, text);
                }
            }
        }

        [Command("gotopos")]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            var pos = new Vector3(x, y, z);
            API.setEntityPosition(player, pos);
            API.sendChatMessageToPlayer(player, "Teleported");
        }

        [Command("get")]
        public static void get_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.shared.getEntityPosition(player);
            API.shared.setEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendNotificationToPlayer(player, "You teleported ~b~" + PlayerManager.GetName(receiver) + " (ID:" + id + ")~w~ to your position.");
            API.shared.sendNotificationToPlayer(receiver, "You were teleported to ~b~" + PlayerManager.GetName(player) + "~w~.");

        }

        [Command("goto")]
        public static void goto_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.shared.getEntityPosition(receiver);
            API.shared.setEntityPosition(player, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.shared.sendChatMessageToPlayer(player, "You have teleported to " + PlayerManager.GetName(receiver) +" (ID:" + id +").");

        }

        [Command("agiveweapon")]
        public void agiveweapon_cmd(Client player, string id, WeaponHash weaponHash, int ammo)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.givePlayerWeapon(receiver, weaponHash, ammo, true, true);
            API.sendChatMessageToPlayer(player, "You have given Player ID: " + id + " a weapon.");
        }

        [Command("sethealth")]
        public void sethealth_cmd(Client player, string id, int health)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
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

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.setPlayerArmor(receiver, armour);
            API.sendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s armour to " + armour + ".");
        }

        [Command("spec")]
        public static void spec_cmd(Client player, string id)
        {
            var target = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (target == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            account.IsSpectating = true;
            API.shared.setPlayerToSpectatePlayer(player, target);
            API.shared.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.GetName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");

        }

        [Command("specoff")]
        public void specoff_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (account.IsSpectating == false)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are not specing anyone.");
                return;
            }
            account.IsSpectating = false;
            API.unspectatePlayer(player);
            API.sendChatMessageToPlayer(player, "You are no longer spectating anyone.");
        }

        [Command("slap")]
        public void slap_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
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

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.freezePlayer(receiver, true);
            API.sendChatMessageToPlayer(receiver, "You have been frozen by an admin");
        }

        [Command("unfreeze")]
        public void unfreeze_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.freezePlayer(receiver, false);
            API.sendChatMessageToPlayer(receiver, "You have been unfrozen by an admin");
        }

        [Command("setmymoney")]
        public void setmymoney_cmd(Client player, int money)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel == 0)
                return;

            character.Money = money;
            API.sendChatMessageToPlayer(player, $"You have sucessfully changed your money to ${money}.");
        }

        [Command("showplayercars")]
        public void showplayercars_cmd(Client player, string id)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = API.getEntityData(player.handle, "Character");
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
            if (account.AdminLevel == 0)
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
            Account account = API.getEntityData(player.handle, "Account");
            Account receiverAccount = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
                return;

            if (receiverAccount.AdminLevel == 0)
            {
                API.sendChatMessageToPlayer(player, "This player is not an admin.");
                return;
            }
            receiverAccount.AdminName = name;
        }

        [Command("admins")]
        public void admins_cmd(Client player)
        {
            API.sendChatMessageToPlayer(player, "=====ADMINS ONLINE NOW=====");
            foreach (var c in API.getAllPlayers())
            {
                Account receiverAccount = API.getEntityData(c.handle, "Account");

                if (receiverAccount.AdminLevel > 0 && receiverAccount.AdminDuty == 0)
                {
                    API.sendChatMessageToPlayer(player, "~g~" + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                }
            }
        }
        //============REPORT SYSTEM=============

        [Command("report", Alias = "re", GreedyArg = true)]
        public void report_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.ReportMuted == true)
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
            if (account.AdminLevel == 0)
                return;


            API.sendChatMessageToPlayer(player, "=======REPORTS=======");
            foreach (var i in AdminReports.Reports.ToList())
            {
                if(i.Type == 1)
                {
                    return;
                }

                if (i.Type == 2)
                {
                    API.sendChatMessageToPlayer(player, "~b~" + i.Name + "reported ~r~" + i.Target + "~w~" + " | " + i.ReportMessage);
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

            if (account.AdminLevel == 0)
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
            sendtoAllAdmins("[REPORT] " + player.nametag + "has taken " + receiver.nametag + "'s report.");
            character.AdminActions++;
        }

        [Command("trashreport", GreedyArg = true)]
        public void trashreport_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel == 0)
                return;


            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if(receivercharacter.HasActiveReport == false)
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
        }

        [Command("ask", GreedyArg = true)]
        public void ask_cmd(Client player, string message)
        {
            Character character = API.getEntityData(player, "Character");

            if(character.ReportMuted == true)
            {
                API.sendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (character.ReportCreated == true)
            {
                API.sendChatMessageToPlayer(player, "Please wait 15 seconds before creating another request.");
                return;
            }
            character.HasActiveReport = true;
            AdminReports.InsertReport(1, player.nametag, message);
            sendtoAllAdmins("~g~[ASK]~w~ " + PlayerManager.GetName(player) + ": " + message);
            API.sendChatMessageToPlayer(player, "~b~Your ask request has been submitted. ~w~Moderators have been informed and will be with you soon.");
            startReportTimer(player);
        }

        [Command("reportmute", GreedyArg = true)]
        public void reportmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.ReportMuted == false)
            {
                receivercharacter.ReportMuted = true;
                API.sendChatMessageToPlayer(player, "You have muted ~b~" + receiver.nametag + "~w~from creating reports.");
                API.sendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from making reports.");
            }
            else
            {
                receivercharacter.ReportMuted = false;
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from making reports.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~from creating reports.");
            }

        }

        [Command("asklist", GreedyArg = true)]
        public void asklist_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
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
            if (account.AdminLevel == 0)
                return;

            AdminReports.Reports.Clear();
            API.sendChatMessageToPlayer(player, "All reports (including ask) have been cleared.");
        }

        //TODO: REMOVE THIS: 
        [Command("makemeadmin")]
        public void makemeadmin_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            account.AdminLevel = 7;
            API.sendChatMessageToPlayer(player, "You are now a king.");
        }

        public void reportTimer(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            character.ReportCreated = false;
        }
    }
}
