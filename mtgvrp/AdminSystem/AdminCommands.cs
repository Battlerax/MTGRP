using System;
using System.Linq;
using System.Reflection;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.group_manager.lspd;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.player_manager.login;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using System.Security.Cryptography;
using mtgvrp.weapon_manager;
using MongoDB.Driver;
using static mtgvrp.core.LogManager;
using Color = mtgvrp.core.Color;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

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
                    Vector3 pos = (Vector3)arguments[0];
                    player.position = pos;
                    break;

            }
        }

        [Command("setallsupplies"), Help(HelpManager.CommandGroups.AdminLevel5, "Used to set all non gas station properties' supplies.",
             new[] { "Amount of supplies to set." })]
        public void SetAllSupplies(Client player, int supply)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                PropertyManager.Properties.Where(y => y.Type != PropertyManager.PropertyTypes.GasStation).AsParallel().ForAll(x => { x.Supplies = supply; });
                API.sendChatMessageToPlayer(player, "Set all non gas station properties supplies to " + supply);
                Log(LogTypes.AdminActions, $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set all non gas station properties supplies to {supply}");
            }  
        }

        [Command("setgassupplies"), Help(HelpManager.CommandGroups.AdminLevel5, "Used to set all gas station properties' supplies.",
             new[] { "Amount of supplies to set." })]
        public void SetGasSupplies(Client player, int supply)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                PropertyManager.Properties.Where(y => y.Type == PropertyManager.PropertyTypes.GasStation).AsParallel().ForAll(x => { x.Supplies = supply; });
                API.sendChatMessageToPlayer(player, "Set all gas station properties supplies to " + supply);
                Log(LogTypes.AdminActions, $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set all gas station properties supplies to {supply}");
            }
        }

        [Command("resetpassword"), Help(HelpManager.CommandGroups.AdminLevel5, "Reset a player's password.",
             new[] { "The player", "The new passsword."})]
        public void resetpassword_cmd(Client player, string accountname, string newpass)
        {
            Account account = API.getEntityData(player, "Account");

            if (account.AdminLevel < 4)
                return;

            if (newpass.Length < 8)
            {
                player.sendChatMessage("Please choose a password that is at least 8 characters long.");
                return;
            }

            var salt = new byte[32];
            LoginManager.Randomizer.GetBytes(salt);

            //Add salt to the end of input_pass
            var inputPass = newpass + System.Text.Encoding.UTF8.GetString(salt);

            //Convert inputted password to bytes
            var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
            password = new SHA256Managed().ComputeHash(password);

            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();

            foreach (var c in foundAccount)
            {
                c.Password = System.Text.Encoding.UTF8.GetString(password);
                c.Salt = System.Text.Encoding.UTF8.GetString(salt);
            }

            player.sendChatMessage("Account password set.");
        }

        [Command("resetmypass"), Help(HelpManager.CommandGroups.AdminLevel5, "Reset your own password.",
             new[] { "Your new password." })]
        public void resetmypass_cmd(Client player, string newpass)
        {
            if (newpass.Length < 8)
            {
                player.sendChatMessage("Please choose a password that is at least 8 characters long.");
                return;
            }

            Account account = API.getEntityData(player, "Account");

            var salt = new byte[32];
            LoginManager.Randomizer.GetBytes(salt);

            //Add salt to the end of input_pass
            var inputPass = newpass + System.Text.Encoding.UTF8.GetString(salt);

            //Convert inputted password to bytes
            var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
            password = new SHA256Managed().ComputeHash(password);

            account.Password = System.Text.Encoding.UTF8.GetString(password);
            account.Salt = System.Text.Encoding.UTF8.GetString(salt);
            account.Save();

            player.sendChatMessage("Account password reset.");
        }

        [Command("makedev"),
         Help(HelpManager.CommandGroups.AdminLevel6, "Used to set a player as a developer.",
             new[] {"The id of target player.", "Dev level, 0 = none."})]
        public void SetDevLevel(Client player, string target, int level)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 6)
            {
                var receiver = PlayerManager.ParseClient(target);
                if (receiver == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                    return;
                }

                if (level < 0)
                {
                    API.sendChatMessageToPlayer(player, "Level cant be under 0");
                    return;
                }

                receiver.GetAccount().DevLevel = level;
                receiver.GetAccount().Save();
                API.sendChatMessageToPlayer(player, $"You've successfully set {receiver.GetCharacter().CharacterName}[{receiver.socialClubName}]'s dev level to " + level);
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set {receiver.GetCharacter().CharacterName}[{receiver.GetAccount().AccountName}] dev level to {level}");
            }
        }

        [Command("resetcharacterjob"), Help(HelpManager.CommandGroups.AdminLevel5, "Used to reset a player's job.", new[] { "The target player"})]
        public void resetcharacterjob_cmd(Client player, string target)
        {
            Account account = API.getEntityData(player, "Account");

            if (account.AdminLevel < 5)
                return;

            var receiver = PlayerManager.ParseClient(target);

            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            receiver.GetCharacter().JobOne = job_manager.Job.None;
        }


        [Command("set", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Used to set items/settings of a player.", new[] { "Id: The id of target player.", "Item: Name of the variable.", "Amount: New value of the variable." })]
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
                    Log(LogTypes.AdminActions, $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
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
                    Log(LogTypes.AdminActions, $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(recChar, value);
                    API.sendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                    Log(LogTypes.AdminActions, $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "Unknown Type.");
                }
            }
        }

        [Command("setadminlevel"), Help(HelpManager.CommandGroups.AdminLevel7, "Setting the admin level of admins, don't know how you made it to level 7 without knowing this honestly.", new[] { "Id: The id of target player.", "Rank set to" })]
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
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)}'s Admin Level to {level}.");
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "You cannot set a higher admin level than yours or set someone to a level above yours.");
            }
        }

        [Command("makeleader"), Help(HelpManager.CommandGroups.AdminLevel5, "Making someone the leader of a group.", new[] { "The id of target player.", "Group ID of the group." })]
        public void makeleader(Client player, string playerid, int groupId)
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
            Log(LogTypes.GroupInvites, $"{leaderChar.CharacterName}[{leaderChar.Client.GetAccount().AccountName}] has joined the group. (Made leader by {player.GetAccount().AdminName}[{player.GetAccount().AccountName}])");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(leaderClient)} as group loader of Group {leaderChar.Group.Name}");
        }

        [Command("gotopos"), Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to the given coordinates", new[] { "X coordinate", "Y coordinate", "Z coordinate" })]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var pos = new Vector3(x, y, z);
            API.setEntityPosition(player, pos);
            API.sendChatMessageToPlayer(player, "Teleported");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} teleported to {x}, {y}, {z}");
        }

        [Command("sendback"), Help(HelpManager.CommandGroups.AdminLevel2, "Sends a player to their original position before being teleported", new[] { "Id: The id of target player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has sent back {GetLogName(receiver)} to his orignal position.");
        }

        [Command("get"), Help(HelpManager.CommandGroups.AdminLevel2, "Used to TP a player to you.", new[] { "Id: The id of target player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported {GetLogName(receiver)} to his position.");
        }

        [Command("adminwarp", Alias = "aw", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Used to TP a player to the player spawn.", new[] { "Id: The id of target player." })]
        public void adminwarp_cmd(Client player, string id)
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has admin warped {receiver}");
        }

        [Command("goto"), Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to the selected player.", new[] { "Id: The id of target player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported to {GetLogName(receiver)}");
        }

        [Command("agiveweapon"), Help(HelpManager.CommandGroups.AdminLevel3, "Gives a weapon to a player", new[] { "Id: The id of target player.", "Weapon Hash (Can use name or number)" })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has given himself a {weaponHash}");
        }

        [Command("sethealth"), Help(HelpManager.CommandGroups.AdminLevel3, "Set the health of a player", new[] { "Id: The id of target player.", "Health amount" })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} health to {health}");
        }

        [Command("setarmour"), Help(HelpManager.CommandGroups.AdminLevel3, "Set the armour of a player.", new[] { "Id: The id of target player.", "Armour amount" })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} armour to {armour}");
        }

        [Command("setvehiclehp"), Help(HelpManager.CommandGroups.AdminLevel3, "Set a vehicles health.", new[] { "Id: The id of Vehicle.", "Health amount" })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set vehicleid {receiver.Id} health to {health}");
        }

        [Command("spec"), Help(HelpManager.CommandGroups.AdminLevel2, "View a player without them seeing you. Sneaky stuff.", new[] { "Id: The id of player." })]
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
            API.shared.setEntityTransparency(player, 0);
            player.GetCharacter().LastPos = player.position;
            API.shared.setEntityPosition(player, target.position);
            API.shared.setPlayerToSpectatePlayer(player, target);
            API.shared.setPlayerNametagVisible(player, false);
            API.shared.setEntityTransparency(player, 0);
            API.shared.sendChatMessageToPlayer(player, "You are now spectating " + PlayerManager.GetName(target) + " (ID:" + id + "). Use /specoff to stop spectating this player.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has specced {GetLogName(target)}");
        }

        [Command("specoff"), Help(HelpManager.CommandGroups.AdminLevel2, "Stop spectating.", null)]
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
            API.shared.setEntityTransparency(player, 255);
            API.unspectatePlayer(player);
            API.shared.setEntityPosition(player, player.GetCharacter().LastPos);
            API.shared.setPlayerNametagVisible(player, true);
            API.shared.setEntityTransparency(player, 255);
            API.sendChatMessageToPlayer(player, "You are no longer spectating anyone.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has stopped speccing.");
        }

        [Command("slap"), Help(HelpManager.CommandGroups.AdminLevel2, "Slaps the given player into the air.", new[] { "Id: The id of player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has slapped {GetLogName(receiver)}");
        }

        [Command("freeze"), Help(HelpManager.CommandGroups.AdminLevel2, "Stops the player from being able to move.", new[] { "Id: The id of player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has frozen {GetLogName(receiver)}");
        }

        [Command("gotowaypoint"), Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to your waypoint.", null)]
        public void gotowaypoint_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            API.triggerClientEvent(player, "getwaypoint");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has went to their waypoint.");
        }

        [Command("unfreeze"), Help(HelpManager.CommandGroups.AdminLevel2, "Allows the player to move once again.", new[] { "Id: The id of player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has unfrozen {GetLogName(receiver)}");
        }

        [Command("quitadmin"), Help(HelpManager.CommandGroups.AdminLevel1, "This will remove you from the team, thank you for helping out the team. o7", null)]
        public void QuitAdmin_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 0)
                return;

            account.AdminLevel = 0;
            account.Save();
            API.sendChatMessageToPlayer(player, "You have quit the admin team.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has quit the admin name.");
        }

        [Command("setmymoney"), Help(HelpManager.CommandGroups.AdminLevel7, "Sets your money to the specificed amount.", new[] { "The amount of money you want." })]
        public void setmymoney_cmd(Client player, int money)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel <= 6)
                return;

            InventoryManager.SetInventoryAmmount(character, typeof(Money), money);
            API.sendChatMessageToPlayer(player, $"You have sucessfully changed your money to ${money}.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set his money to {money}");
        }

        [Command("showplayercars"), Help(HelpManager.CommandGroups.AdminLevel2, "Give you a list of the vehicles a player owns.", new[] { "Id: The id of player." })]
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

        [Command("getvehicle"), Help(HelpManager.CommandGroups.AdminLevel2, "Teleports a vehicle to you.", new[] { "Vehicle ID" })]
        public void getplayercar_cmd(Client player, int vID)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var veh = VehicleManager.GetVehicleById(vID);

            if(veh == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That vehicle ID does not exist.");
                return;
            }

            if(veh.IsSpawned == false)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That vehicle is not spawned.");
                return;
            }

            API.setEntityPosition(veh.NetHandle, player.position);
            API.sendChatMessageToPlayer(player, "Sucessfully teleported the car to you.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported vehicle #{vID} to the Admin's position.");
        }

        [Command("setadminname"), Help(HelpManager.CommandGroups.AdminLevel6, "Can set the admin name for admins.", new[] { "Id: The id of player.", "Desired name they want." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} Admin name to {name}");
        }

        [Command("admins"), Help(HelpManager.CommandGroups.General, "A list of all admins online at the moment.", null)]
        public void admins_cmd(Client player)
        {
            API.sendChatMessageToPlayer(player, "=====ADMINS ONLINE NOW=====");
            foreach (var c in API.getAllPlayers())
            {
                if (c == null)
                    continue;

                Account receiverAccount = API.getEntityData(c.handle, "Account");

                if (receiverAccount.AdminLevel > 0)
                {
                    if (receiverAccount.AdminDuty)
                    {
                        API.sendChatMessageToPlayer(player, "~g~[ONDUTY] " + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                    }
                    else
                    {
                        API.sendChatMessageToPlayer(player, "~r~[OFFDUTY] " + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                    }
                }
            }
        }

        [Command("adminduty"), Help(HelpManager.CommandGroups.AdminLevel1, "Can go on and off of admin duty with this.", null)]
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
                API.setPlayerNametag(player, account.AdminName + " (" + PlayerManager.GetPlayerId(character) + ")");
                API.sendChatMessageToPlayer(player, "You are now on admin duty.");
                return;
            }

            account.AdminDuty = false;
            player.nametag = character.CharacterName + " (" + character.Id + ")";
            API.resetPlayerNametagColor(player);
            API.sendChatMessageToPlayer(player, "You are no longer on admin duty.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} became on admin duty.");
        }

        [Command("whereami"), Help(HelpManager.CommandGroups.General, "Give you your current location in X,Y,Z format.", null)]
        public void GetPlayerLocation(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel == 0)
            {
            }
            else
            {
                Vector3 CurrentPlayerPos = API.getEntityPosition(player);
                Vector3 CurrentPlayerRot = API.getEntityRotation(player);
                int playerDimension = API.getEntityDimension(player);
                API.sendChatMessageToPlayer(player, "-----Current Position-----");
                API.sendChatMessageToPlayer(player, "X: " + CurrentPlayerPos.X + " Y: " + CurrentPlayerPos.Y + " Z: " + CurrentPlayerPos.Z + " Dimension: " + playerDimension);
                API.consoleOutput($"POSITION: new Vector3({CurrentPlayerPos.X}, {CurrentPlayerPos.Y}, {CurrentPlayerPos.Z})");
                API.consoleOutput($"ROTATION: new Vector3({CurrentPlayerRot.X}, {CurrentPlayerRot.Y}, {CurrentPlayerRot.Z})");
            }

        }






        //============REPORT SYSTEM=============

        [Command("report", Alias = "re", GreedyArg = true), Help(HelpManager.CommandGroups.General, "Use this to make an ingame report for an admin to sort or to speak to an admin about an issue.", null)]
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

            if (character.ReportCreated)
            {
                API.sendChatMessageToPlayer(player, "Please wait 15 seconds before creating another report.");
                return;
            }
            API.triggerClientEvent(player, "show_report_menu");
        }

        [Command("reports", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "View all current ingame reports made.", null)]
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

        [Command("acceptreport", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "For you to take on a report.", new[] { "Id: The id of target player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has accepted report made by {GetLogName(receiver)}");
        }

        [Command("trashreport", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "For you to get rid of a report.", new[] { "Id: The id of target player." })]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has trashed report by {GetLogName(receiver)}");
        }

        [Command("maccept", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "For you to take on an ask.", new[] { "Id: The id of target player." })]
        public void maccept_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");
            Character receivercharacter = API.getEntityData(receiver, "Character");

            if (account.AdminLevel < 1)
                return;

            if (character.IsOnAsk)
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has accepted {GetLogName(receiver)}'s /ask.");
        }

        [Command("mfinish", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "When you're done with the player hit em with this.", null)]
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
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has finished their /ask.");
        }

        [Command("ask", GreedyArg = true), Help(HelpManager.CommandGroups.General, "To ask a mod/admin a question. Also useful for a checkpoint.", new[] { "Question for moderators" })]
        public void ask_cmd(Client player, string message)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.ReportMuteExpires > DateTime.Now)
            {
                API.sendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (character.ReportCreated)
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

        [Command("nmute", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "Used to mute players in /n.", new[] { "Id: The id of target player." })]
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
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from newbie chat.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~ from newbie chat.");
                receivercharacter.NMutedExpiration = DateTime.Now;
            }
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has nmuted {GetLogName(receiver)}");
        }

        [Command("vmute", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "Used to mute people in /v.", new[] { "Id: The id of target player." })]
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
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from VIP chat.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~ from VIP chat.");
                receivercharacter.VMutedExpiration = DateTime.Now;
            }
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has vmuted {GetLogName(receiver)}");
        }

        [Command("reportmute", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "Used to stop people from making reports.", new[] { "Id: The id of target player." })]
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
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from making reports.");
                API.sendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.nametag + "~w~from creating reports.");
                receivercharacter.ReportMuteExpires = DateTime.Now;
            }
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has reportmuted {GetLogName(receiver)}");
        }
        //here toro
        [Command("mlist", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel1, "Used to list the active moderator reques.", null)]
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

        [Command("clearreports", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel3, "Clears all of the curren reports and asks", null)]
        public void clearreports_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 3)
                return;

            AdminReports.Reports.Clear();
            API.sendChatMessageToPlayer(player, "All reports (including ask) have been cleared.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has trashed all reports and asks.");
        }

        //PLAYER-ADMIN STUFF

        [Command("prison", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Places a player into prison for the specificed amount of time.", new [] {"ID of the target player", "Time in minutes."})]
        public void prison_cmd(Client player, string id, string time)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            receiver.GetCharacter().JailTimeLeft = int.Parse(time) * 1000 * 60;
            Lspd.JailControl(receiver, int.Parse(time));
            API.sendChatMessageToPlayer(player, "You have jailed " + receiver.nametag + " for " + time + " minutes.");
            API.sendChatMessageToPlayer(receiver, "You have been jailed by " + player.nametag + " for " + time + " minutes.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has jailed {GetLogName(receiver)} for {time} second(s).");
        }

        [Command("kickplayer", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Kicks a player from the server", new [] {"ID of the target player", "The kick reason."})]
        public static void kick_cmd(Client player, string id, string reason)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = API.shared.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 2)
                return;

            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has kicked {GetLogName(receiver)}.");
            KickPlayer(receiver, reason);
        }

        [Command("remotewarn", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel3, "Applies a player warning to an offline player", new[] { "Account name of the target player", "The warning reason" })]
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
                Log(LogTypes.Warns, $"Admin {account.AdminName}[{player.socialClubName}] has remotewarned the player [{c.AccountName}]. Reason: '{reason}'");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remotewarned the player [{c.AccountName}]. Reason: '{reason}'");
                break;
            }
        }


        [Command("remoteban", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel3, "Bans an offline player", new[] { "Account name of the target player", "The ban reason" })]
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
                Log(LogTypes.Bans, $"Admin {account.AdminName}[{player.socialClubName}] has remotebanned the player [{c.AccountName}]. Reason: '{reason}'");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remotebanned the player [{c.AccountName}]. Reason: '{reason}'");
                break;
            }
        }

        [Command("getaccountname", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Gets the account name of a character", new[] { "Character name for the account" })]
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

        [Command("unban", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel3, "Unbans an account from the server", new[] { "Account name of the target player" })]
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
                Log(LogTypes.Unbans, $"Admin {account.AdminName}[{player.socialClubName}] has unbanned the player [{c.AccountName}].");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has unbanned {c.AccountName}");
                break;
            }
        }

        [Command("untempban", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel3, "Removes an accouns tempban", new[] { "ID of the target player" })]
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
                Log(LogTypes.Unbans, $"Admin {account.AdminName}[{player.socialClubName}] has untempbanned the player [{c.AccountName}].");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has untempbanned {c.AccountName}");
                break;
            }
        }

        [Command("banplayer", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Bans a player from the server", new[] { "ID of the target player", "Ban reason" })]
        public static void ban_cmd(Client player, string id, string reason)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);
            BanPlayer(receiver, reason);
            Log(LogTypes.Bans, $"Admin {account.AdminName}[{player.socialClubName}] has banned the player {receiver.GetCharacter().CharacterName}[{receiver.socialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has banned {GetLogName(receiver)} for <{reason}>");
        }


        [Command("warn", GreedyArg = false), Help(HelpManager.CommandGroups.AdminLevel2, "Applies a player warning to a player", new[] { "ID of the target player", "The reason for the warning"})]
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
            Log(LogTypes.Warns, $"Admin {account.AdminName}[{player.socialClubName}] has warned the player {receiver.GetCharacter().CharacterName}[{receiver.socialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has warned {GetLogName(receiver)} for <{reason}>");

            if (receiverAccount.PlayerWarns.Count() >= 3)
            {
                API.shared.sendNotificationToPlayer(player, "You were temporarily banned for reaching 3 player warns.");
                AddTempBanLevel(receiver);
                TempBanPlayer(receiver);
                receiverAccount.PlayerWarns.Clear();
                API.shared.kickPlayer(receiver);
            }
        }

        [Command("removewarns", GreedyArg = false), Help(HelpManager.CommandGroups.AdminLevel2, "Removes all of a players warns", new[] { "ID of the target player", "The reason for removing them" })]
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
            Log(LogTypes.Bans, $"Admin {account.AdminName}[{player.socialClubName}] has removed all the warns of the player {receiver.GetCharacter().CharacterName}[{receiver.socialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has removed all warns of {GetLogName(receiver)}");
        }

        [Command("remoteplayerwarns"), Help(HelpManager.CommandGroups.AdminLevel2, "View an offline player's warnings", new[] { "Account name of the player" })]
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


        [Command("playerwarns", GreedyArg = false), Help(HelpManager.CommandGroups.General, "View your player warnings", new[] { "ID of the target player <strong>[ADMIN ONLY]</strong>" })]
        public static void playerwarns_cmd(Client player, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2 && id != null)
            {
            }

            else if (id == null)
            {
                ShowWarns(player);
            }

            else if (account.AdminLevel >= 2 && id != null)
            {
                ShowWarns(player, receiver);
            }
        }

        [Command("respawnveh", GreedyArg = false), Help(HelpManager.CommandGroups.AdminLevel4, "Respawns a vehicle.", "Id of vehicle to respawn. (OPTIONAL)")]
        public void respawnveh_cmd(Client player, int id = 0)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 3)
            {
                return;
            }

            if (id == 0 && !player.isInVehicle)
            {
                player.sendChatMessage("You must enter a vehicle ID.");
                return;
            }

            Vehicle veh = null;

            if (player.isInVehicle)
            {
                veh = VehicleManager.GetVehFromNetHandle(API.getPlayerVehicle(player));
            }

            if (veh == null)
            {
                veh = VehicleManager.GetVehicleById(id);
                if (veh == null)
                {
                    player.sendChatMessage("Invalid vehicle ID.");
                    return;
                }
            }

            VehicleManager.respawn_vehicle(veh);
            player.sendChatMessage("Vehicle respawned.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has respawned vehicle id {veh.Id}");
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

        [Command("setcharacterslots"), Help(HelpManager.CommandGroups.AdminLevel3, "Set the amount of character slots a player may have", new[] { "ID of the target player", "The amount of character slots permitted" })]
        public void setcharacterslots(Client player, string id, int slots)
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

            receiverAccount.CharacterSlots = slots;
            receiverAccount.Save();
            player.sendChatMessage($"You have set {receiver.GetCharacter().CharacterName}'s character slots to {slots}.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} character slots to {slots}");
        }

        [Command("changeviplevel"), Help(HelpManager.CommandGroups.AdminLevel3, "Change a players VIP level", new[] { "ID of the target player", "The VIP level to change to", "The VIP amount in days" })]
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
                receiverAccount.VipLevel = 3;
                receiverAccount.Save();
                receiver.sendChatMessage("Your ~y~VIP~y~ level was set to " + 3 + " by " + account.AdminName + ".");
                return;

            }

            if (receiverAccount.VipLevel == level)
            {
                player.sendChatMessage("This player is already this VIP level.");
                return;
            }

            receiverAccount.VipLevel = level;
            receiverAccount.VipExpirationDate = DateTime.Now.AddDays(days);
            receiverAccount.Save();

            receiver.sendChatMessage("Your ~y~VIP~y~ level was set to " + level + " by " + account.AdminName + ".");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} VIP level to {level} for {days} days.");

            if (level > 0)
            {
                foreach (var p in API.getAllPlayers())
                {
                    if (p == null)
                        continue;

                    Account paccount = API.getEntityData(p.handle, "Account");

                    if (paccount.VipLevel > 0) { p.sendChatMessage(receiver.GetCharacter().CharacterName + " has become a level " + level + " ~y~VIP~y~!"); }
                }
            }
        }

        [Command("addviptime", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Adds VIP time to a specific players VIP", new[] { "ID of the target player", "The amount to add in days" })]
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
                player.sendChatMessage("You cannot set the VIP time of an administrator.");
                return;
            }

            account.VipExpirationDate = account.VipExpirationDate.AddDays(int.Parse(days));
            account.Save();

            receiver.sendChatMessage("Your ~y~VIP~y~ days were increased by " + int.Parse(days) + " days by " + account.AdminName + "!");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has increased {GetLogName(receiver)} VIP time by {days} days.");
        }

        [Command("closestveh"), Help(HelpManager.CommandGroups.AdminLevel3, "Sets you into the closest vehicle.")]
        public void closestveh_cmd(Client player)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 2)
                return;

            var ClosestVeh = GetClosestVeh(player);
            if (ClosestVeh == null)
            {
                player.sendChatMessage("There are no nearby vehicles.");
                return;
            }

            API.setPlayerIntoVehicle(player, ClosestVeh, -1);
        }

        public void startReportTimer(Client player)
        {
            Character senderchar = API.getEntityData(player.handle, "Character");
            senderchar.ReportCreated = true;
            senderchar.ReportTimer = new Timer { Interval = 15000 };
            senderchar.ReportTimer.Elapsed += delegate { ReportTimer(player); };
            senderchar.ReportTimer.Start();
        }

        public static void SendtoAllAdmins(string text)
        {
            foreach (var c in API.shared.getAllPlayers())
            {
                if (c == null)
                    continue;

                Account receiverAccount = API.shared.getEntityData(c.handle, "Account");

                if (receiverAccount == null)
                    return;

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

        public NetHandle GetClosestVeh(Client player)
        {
            var shortestDistance = 2000f;
            NetHandle closestveh = new NetHandle();
            foreach (var veh in API.getAllVehicles())
            {
                Vector3 Position = API.getEntityPosition(veh);
                var VehicleDistance = player.position.DistanceTo(Position);
                if (VehicleDistance < shortestDistance)
                {
                    shortestDistance = VehicleDistance;
                    closestveh = veh;
                }
            }
            return closestveh;
        }

        [Command("forceredochar"), Help(HelpManager.CommandGroups.AdminLevel3, "Force someone to redo character creation.", new[] { "Target player id." })]
        public void RedoCharacterSelection(Client player, string target)
        {
            if (player.GetAccount().AdminLevel >= 2)
            {
                var targetClient = PlayerManager.ParseClient(target);
                if (targetClient == null)
                    return;

                targetClient.setData("REDOING_CHAR", true);
                API.freezePlayer(targetClient, true);
                API.setEntityDimension(targetClient, targetClient.GetCharacter().Id + 1000);
                API.setEntitySyncedData(targetClient, "REG_DIMENSION", targetClient.GetCharacter().Id + 1000);
                targetClient.GetCharacter().Model.SetDefault();
                API.triggerClientEvent(targetClient, "show_character_creation_menu");
            }
        }

        [Command("testtext"), Help(HelpManager.CommandGroups.AdminLevel3, "Goes into testing on-screen text position.", new[] { "Text to display." })]
        public void TestText(Client player, string text = "")
        {
            if (player.GetAccount().AdminLevel >= 3)
            {
                API.triggerClientEvent(player, "texttest_settext", text);
            }
        }

        [Command("giveitem"), Help(HelpManager.CommandGroups.AdminLevel4, "Gives an inventory item to a player.<br/> <strong>This could cause problems with the player, use with caution.</strong>", new[] { "Target ID or name.", "Item name, you can get this from a dev.", "Amount to give." })]
        public void GiveItem(Client player, string target, string item, int amount)
        {
            if (player.GetAccount().AdminLevel < 4)
            {
                return;
            }

            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                API.sendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var type = InventoryManager.ParseInventoryItem(item);
            if (type == null)
            {
                API.sendChatMessageToPlayer(player, "Unexisting Item.");
                return;
            }

            var itema = InventoryManager.ItemTypeToNewObject(type);
            InventoryManager.GiveInventoryItem(targetPlayer.GetCharacter(), itema, amount);
            API.sendChatMessageToPlayer(player, "Done.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {player.GetAccount().AdminName} has given {GetLogName(targetPlayer)} an item {item}, amount: {amount}");
        }
    }
}
