using System;
using System.Linq;
using System.Reflection;
using System.Timers;


using GTANetworkAPI;



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
using GameVehicle = mtgvrp.vehicle_manager.GameVehicle;
using MongoDB.Bson;

namespace mtgvrp.AdminSystem
{
    public class AdminCommands : Script
    {
        public AdminCommands()
        {
            DebugManager.DebugMessage("[AdminSys] Admin System initalized.");
        }

        [ServerEvent(Event.PlayerDamage)]
        public void OnPlayerDamage(Client player, float lossFirst, float lossSecond)
        {
            Character c = player.GetCharacter();
            if (c.isAJailed || c.IsJailed)
            {
                player.Health = 100;
            }
        }

        private static readonly Vector3 aJailLoc = new Vector3(136.5146, -2203.149, 7.30914);

        [RemoteEvent("OnRequestSubmitted")]
        public void OnRequestSubmitted(Client player, params object[] arguments)
        {
            Character character = player.GetCharacter();
            int playerid = PlayerManager.GetPlayerId(character);
            AdminReports.InsertReport(3, player.Nametag, (string)arguments[0]);
            SendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + playerid + "): " +
                            (string)arguments[0]);
            NAPI.Chat.SendChatMessageToPlayer(player, "Report submitted.");
            startReportTimer(player);
            character.HasActiveReport = true;
        }

        [RemoteEvent("OnReportMade")]
        public void OnReportMade(Client player, params object[] arguments)
        {
            Character senderchar = player.GetCharacter();
            int senderid = PlayerManager.GetPlayerId(senderchar);
            string id = (string)arguments[1];
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            AdminReports.InsertReport(2, player.Nametag, (string)arguments[0],
                PlayerManager.GetName(receiver) + " (ID:" + id + ")");
            SendtoAllAdmins("~g~[REPORT]~w~ " + PlayerManager.GetName(player) + " (ID:" + senderid + ")" +
                            " reported " + PlayerManager.GetName(receiver) + " (ID:" + id + ") for " +
                            (string)arguments[0]);
            NAPI.Chat.SendChatMessageToPlayer(player, "Report submitted.");
            startReportTimer(player);
            senderchar.HasActiveReport = true;
        }

        [RemoteEvent("SET_PLAYER_CP")]
        public void SetPlayerCP(Client player, params object[] arguments)
        {
            var p = NAPI.Player.GetPlayerFromHandle((NetHandle)arguments[0]);
            NAPI.ClientEvent.TriggerClientEvent(p, "update_beacon", (Vector3)arguments[1]);
        }

        [RemoteEvent("teleport")]
        public void Teleport(Client player, params object[] arguments)
        {
            Vector3 pos = (Vector3)arguments[0];
            player.Position = new Vector3(pos.X, pos.Y, pos.Z);
        }

        [Command("arelease")]
        [Help(HelpManager.CommandGroups.AdminLevel2, "Release a player from admin prison.", "Player to free.")]
        public void arelease_cmd(Client sender, string id)
        {
            Account a = sender.GetAccount();
            if (a.AdminLevel < 2) return;
            var player = PlayerManager.ParseClient(id);

            Character c = player.GetCharacter();

            if (!c.isAJailed)
            {
                NAPI.Chat.SendChatMessageToPlayer(sender,"That player is not admin jailed!");
                return;
            }

            aSetFree(sender);
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {a.AdminName} has released {c.CharacterName} from admin jail");

        }

        [Command("setallsupplies"), Help(HelpManager.CommandGroups.AdminLevel5,
             "Used to set all non gas station properties' supplies.",
             new[] {"Amount of supplies to set."})]
        public void SetAllSupplies(Client player, int supply)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                PropertyManager.Properties.Where(y => y.Type != PropertyManager.PropertyTypes.GasStation).AsParallel()
                    .ForAll(x =>
                    {
                        x.Supplies = supply;
                        x.Save();
                    });
                NAPI.Chat.SendChatMessageToPlayer(player, "Set all non gas station properties supplies to " + supply);
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set all non gas station properties supplies to {supply}");
            }
        }

        [Command("setgassupplies"), Help(HelpManager.CommandGroups.AdminLevel5,
             "Used to set all gas station properties' supplies.",
             new[] {"Amount of supplies to set."})]
        public void SetGasSupplies(Client player, int supply)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                PropertyManager.Properties.Where(y => y.Type == PropertyManager.PropertyTypes.GasStation).AsParallel()
                    .ForAll(x => { x.Supplies = supply; });
                NAPI.Chat.SendChatMessageToPlayer(player, "Set all gas station properties supplies to " + supply);
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set all gas station properties supplies to {supply}");
            }
        }

        [Command("resetpassword"), Help(HelpManager.CommandGroups.AdminLevel5, "Reset a player's password.",
             new[] {"The player", "The new passsword."})]
        public void resetpassword_cmd(Client player, string accountname, string newpass)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 4)
                return;

            if (newpass.Length < 8)
            {
                player.SendChatMessage("Please choose a password that is at least 8 characters long.");
                return;
            }

            var salt = new byte[32];
            LoginManager.Randomizer.GetBytes(salt);

            //Add salt to the end of input_pass
            var inputPass = newpass + System.Text.Encoding.UTF8.GetString(salt);

            //Convert inputted password to bytes
            var password = System.Text.Encoding.UTF8.GetBytes(inputPass);
            password = new SHA256Managed().ComputeHash(password);

            var foundAccount = DatabaseManager.AccountTable.Find(x => x.AccountName == accountname).SingleOrDefault();
            if (foundAccount == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "No account with such name.");
                return;
            }

            foundAccount.Password = System.Text.Encoding.UTF8.GetString(password);
            foundAccount.Salt = System.Text.Encoding.UTF8.GetString(salt);

            foundAccount.Save();
            player.SendChatMessage("Account password set.");
        }

        [Command("resetmypass"), Help(HelpManager.CommandGroups.AdminLevel5, "Reset your own password.",
             new[] {"Your new password."})]
        public void resetmypass_cmd(Client player, string newpass)
        {
            if (newpass.Length < 8)
            {
                player.SendChatMessage("Please choose a password that is at least 8 characters long.");
                return;
            }

            Account account = player.GetAccount();

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

            player.SendChatMessage("Account password reset.");
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
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                    return;
                }

                if (level < 0)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Level cant be under 0");
                    return;
                }

                receiver.GetAccount().DevLevel = level;
                receiver.GetAccount().Save();
                NAPI.Chat.SendChatMessageToPlayer(player,
                    $"You've successfully set {receiver.GetCharacter().CharacterName}[{receiver.SocialClubName}]'s dev level to " +
                    level);
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set {receiver.GetCharacter().CharacterName}[{receiver.GetAccount().AccountName}] dev level to {level}");
            }
        }

        [Command("resetcharacterjob"),
         Help(HelpManager.CommandGroups.AdminLevel5, "Used to reset a player's job.", new[] {"The target player"})]
        public void resetcharacterjob_cmd(Client player, string target)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 5)
                return;

            var receiver = PlayerManager.ParseClient(target);

            if (receiver == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            receiver.GetCharacter().JobOne = job_manager.Job.None;
        }


        [Command("set", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel5, "Used to set items/settings of a player.", new[]
             {"Id: The id of target player.", "Item: Name of the variable.", "Amount: New value of the variable."})]
        public void SetCharacterData(Client player, string target, string var, string value)
        {
            var acc = player.GetAccount();
            if (acc.AdminLevel >= 5)
            {
                var receiver = PlayerManager.ParseClient(target);
                if (receiver == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                    return;
                }

                var recChar = receiver.GetCharacter();
                var prop = recChar.GetType().GetProperties().SingleOrDefault(x => x.Name == var);
                if (prop == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "There is no such property.");
                    return;
                }

                if (prop.PropertyType == typeof(int))
                {
                    int val;
                    if (!int.TryParse(value, out val))
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "That property is an integer.");
                        return;
                    }

                    prop.SetValue(recChar, val);
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                    Log(LogTypes.AdminActions,
                        $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    bool val;
                    if (!bool.TryParse(value, out val))
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "That property is a bool.");
                        return;
                    }

                    prop.SetValue(recChar, val);
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                    Log(LogTypes.AdminActions,
                        $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(recChar, value);
                    NAPI.Chat.SendChatMessageToPlayer(player, $"Sucessfully set {var} to the value: {value}");
                    recChar.Save();
                    Log(LogTypes.AdminActions,
                        $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {acc.AdminName} has set property {var} of {recChar.CharacterName}[{receiver.GetAccount().AccountName}] to {value}.");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "Unknown Type.");
                }
            }
        }

        [Command("setadminlevel"),
         Help(HelpManager.CommandGroups.AdminLevel7,
             "Setting the admin level of admins, don't know how you made it to level 7 without knowing this honestly.",
             new[] {"Id: The id of target player.", "Rank set to"})]
        public void setrank_cmd(Client player, string id, int level)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Account receiverAccount = receiver.GetAccount();
            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            if (account.AdminLevel < 7)
                return;

            if (receiver == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            if (account.AdminLevel >= receiverAccount.AdminLevel && account.AdminLevel > level)
            {
                var oldLevel = receiverAccount.AdminLevel;
                if (oldLevel > level)
                {
                    NAPI.Chat.SendChatMessageToPlayer(receiver,
                        "You have been demoted to admin level " + level + " by " + character.CharacterName + ".");
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(receiver,
                        "You have been promoted to admin level " + level + " by " + character.CharacterName + ".");
                }
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have changed " + receiverCharacter.CharacterName + "'s admin level to " + level + " (was " +
                    oldLevel + ").");
                receiverAccount.AdminLevel = level;
                receiverAccount.Save();
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)}'s Admin Level to {level}.");
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                    "You cannot set a higher admin level than yours or set someone to a level above yours.");
            }
        }

        [Command("makeleader"),
         Help(HelpManager.CommandGroups.AdminLevel5, "Making someone the leader of a group.",
             new[] {"The id of target player.", "Group ID of the group."})]
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
                NAPI.Chat.SendChatMessageToPlayer(player, Color.Grey, "~r~[ERROR]~w~ That player is not online.");
                return;
            }

            var leaderChar = leaderClient.GetCharacter();

            var group = GroupManager.GetGroupById(groupId);
            if (group == Group.None)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ That group ID is not valid.");
                return;
            }

            leaderChar.Group = group;
            leaderChar.GroupId = group.Id;
            leaderChar.GroupRank = 10;
            leaderChar.Save();

            NAPI.Chat.SendChatMessageToPlayer(leaderClient, Color.White, "You have been made the leader of " + group.Name);
            NAPI.Chat.SendChatMessageToPlayer(player, Color.Grey,
                "You have made " + leaderChar.CharacterName + " the leader of " + group.Name);

            GroupManager.SendGroupMessage(player,
                leaderChar.rp_name() + " has joined the group. (Made leader by " + player.GetCharacter().rp_name() +
                ")");
            Log(LogTypes.GroupInvites,
                $"{leaderChar.CharacterName}[{leaderChar.Client.GetAccount().AccountName}] has joined the group. (Made leader by {player.GetAccount().AdminName}[{player.GetAccount().AccountName}])");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(leaderClient)} as group loader of Group {leaderChar.Group.Name}");
        }

        [Command("gotopos"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to the given coordinates",
             new[] {"X coordinate", "Y coordinate", "Z coordinate"})]
        public void gotopos_cmd(Client player, double x, double y, double z)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var pos = new Vector3(x, y, z);
            NAPI.Entity.SetEntityPosition(player, pos);
            NAPI.Chat.SendChatMessageToPlayer(player, "Teleported");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} teleported to {x}, {y}, {z}");
        }

        [Command("sendback"),
         Help(HelpManager.CommandGroups.AdminLevel2,
             "Sends a player to their original position before being teleported",
             new[] {"Id: The id of target player."})]
        public static void sendback_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receiverCharacter = receiver.GetCharacter();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            var playerPos = receiverCharacter.LastPos;
            API.Shared.SetEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.Shared.SendNotificationToPlayer(player,
                "You teleported ~b~" + PlayerManager.GetName(receiver) + " (ID:" + id +
                ")~w~ back to their previous position.");
            API.Shared.SendNotificationToPlayer(receiver,
                "You were teleported back to your original position by ~b~" + PlayerManager.GetName(player) + "~w~.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has sent back {GetLogName(receiver)} to his orignal position.");
        }

        [Command("get"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Used to TP a player to you.",
             new[] {"Id: The id of target player."})]
        public static void get_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receiverCharacter = receiver.GetCharacter();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.Shared.GetEntityPosition(player);
            var targetPos = API.Shared.GetEntityPosition(receiver);
            receiverCharacter.LastPos = targetPos;
            API.Shared.SetEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.Shared.SendNotificationToPlayer(player,
                "You teleported ~b~" + PlayerManager.GetName(receiver) + " (ID:" + id + ")~w~ to your position.");
            API.Shared.SendNotificationToPlayer(receiver,
                "You were teleported to ~b~" + PlayerManager.GetName(player) + "~w~.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported {GetLogName(receiver)} to his position.");
        }

        [Command("adminwarp", Alias = "aw", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "Used to TP a player to the player spawn.",
             new[] {"Id: The id of target player."})]
        public void adminwarp_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            //WILL FINISH ONCE WE ARE SURE ON SPAWN POINTS ETC. FOR NOW IT'S THE TRAIN SPAWN:
            NAPI.Entity.SetEntityPosition(receiver, new Vector3(433.2354, -645.8408, 28.7263));
            NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been admin warped to the spawn by an admin.");
            NAPI.Chat.SendChatMessageToPlayer(player, "You have admin warped " + receiver.Nametag + " to the spawn.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has admin warped {receiver}");
        }

        [Command("goto"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to the selected player.",
             new[] {"Id: The id of target player."})]
        public static void goto_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = API.Shared.GetEntityPosition(receiver);
            API.Shared.SetEntityPosition(player, new Vector3(playerPos.X, playerPos.Y + 1, playerPos.Z));
            API.Shared.SendChatMessageToPlayer(player,
                "You have teleported to " + PlayerManager.GetName(receiver) + " (ID:" + id + ").");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported to {GetLogName(receiver)}");
        }

        [Command("agiveweapon"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Gives a weapon to a player", new[]
             {"Id: The id of target player.", "Weapon Hash (Can use name or number)"})]
        public void agiveweapon_cmd(Client player, string id, WeaponHash weaponHash)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            WeaponManager.CreateWeapon(receiver, weaponHash, WeaponTint.Normal, false, true);
            NAPI.Chat.SendChatMessageToPlayer(player, "You have given Player ID: " + id + " a " + weaponHash);
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has given himself a {weaponHash}");
        }

        [Command("sethealth"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Set the health of a player",
             new[] {"Id: The id of target player.", "Health amount"})]
        public void sethealth_cmd(Client player, string id, int health)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.SetPlayerHealth(receiver, health);
            NAPI.Chat.SendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s health to " + health + ".");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} health to {health}");
        }

        [Command("setarmour"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Set the armour of a player.",
             new[] {"Id: The id of target player.", "Armour amount"})]
        public void setarmour_cmd(Client player, string id, int armour)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.SetPlayerArmor(receiver, armour);
            NAPI.Chat.SendChatMessageToPlayer(player, "You have set Player ID: " + id + "'s armour to " + armour + ".");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} armour to {armour}");
        }

        [Command("setvehiclehp"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Set a vehicles health.",
             new[] {"Id: The id of Vehicle.", "Health amount"})]
        public void SetVehHP_cmd(Client player, int vehid, int health)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            var receiver = VehicleManager.Vehicles.SingleOrDefault(x => x.Id == vehid);

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid vehicleid entered.");
                return;
            }

            if (receiver.IsSpawned == false)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Vehicle not spawned.");
                return;
            }

            API.SetVehicleHealth(receiver.NetHandle, health);
            NAPI.Chat.SendChatMessageToPlayer(player,
                "You have set Vehicle ID: " + receiver.Id + "'s health to " + health + ".");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set vehicleid {receiver.Id} health to {health}");
        }

        [Command("spec"),
         Help(HelpManager.CommandGroups.AdminLevel2, "View a player without them seeing you. Sneaky stuff.",
             new[] {"Id: The id of player."})]
        public static void spec_cmd(Client player, string id)
        {
            var target = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (target == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (!account.IsSpectating)
            {
                player.GetCharacter().LastPos = player.Position;
            }

            account.IsSpectating = true;
            API.Shared.SetEntityPosition(player, target.Position);
            API.Shared.SetPlayerToSpectatePlayer(player, target);
            API.Shared.SetPlayerNametagVisible(player, false);
            API.Shared.SetEntityTransparency(player, 0);
            API.Shared.SendChatMessageToPlayer(player,
                "You are now spectating " + PlayerManager.GetName(target) + " (ID:" + id +
                "). Use /specoff to stop spectating this player.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has specced {GetLogName(target)}");
        }

        [Command("specoff"), Help(HelpManager.CommandGroups.AdminLevel2, "Stop spectating.", null)]
        public void specoff_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (account.IsSpectating == false)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ You are not specing anyone.");
                return;
            }
            account.IsSpectating = false;
            API.UnspectatePlayer(player);
            API.Shared.SetEntityPosition(player, player.GetCharacter().LastPos);
            API.Shared.SetPlayerNametagVisible(player, true);
            API.Shared.SetEntityTransparency(player, 255);
            NAPI.Chat.SendChatMessageToPlayer(player, "You are no longer spectating anyone.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has stopped speccing.");
        }

        [Command("slap"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Slaps the given player into the air.",
             new[] {"Id: The id of player."})]
        public void slap_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            var playerPos = NAPI.Entity.GetEntityPosition(receiver);
            NAPI.Entity.SetEntityPosition(receiver, new Vector3(playerPos.X, playerPos.Y, playerPos.Z + 5));
            NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been slapped by an admin");
            NAPI.Chat.SendChatMessageToPlayer(player,"You have slapped " + receiver.GetCharacter().rp_name());
            ChatManager.NearbyMessage(receiver, 10f,
                $"{receiver.GetCharacter().rp_name()} has been slapped by an admin.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has slapped {GetLogName(receiver)}");
        }

        [Command("freeze"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Stops the player from being able to move.",
             new[] {"Id: The id of player."})]
        public void freeze_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            NAPI.Player.FreezePlayer(receiver, true);
            NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been frozen by an admin");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has frozen {GetLogName(receiver)}");
        }

        [Command("gotowaypoint"), Help(HelpManager.CommandGroups.AdminLevel2, "Teleports you to your waypoint.", null)]
        public void gotowaypoint_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            NAPI.ClientEvent.TriggerClientEvent(player, "getwaypoint");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has went to their waypoint.");
        }

        [Command("unfreeze"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Allows the player to move once again.",
             new[] {"Id: The id of player."})]
        public void unfreeze_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            NAPI.Player.FreezePlayer(receiver, false);
            NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been unfrozen by an admin");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has unfrozen {GetLogName(receiver)}");
        }

        [Command("quitadmin"),
         Help(HelpManager.CommandGroups.AdminLevel1,
             "This will remove you from the team, thank you for helping out the team. o7", null)]
        public void QuitAdmin_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 0)
                return;

            account.AdminLevel = 0;
            account.Save();
            NAPI.Chat.SendChatMessageToPlayer(player, "You have quit the admin team.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has quit the admin name.");
        }

        [Command("setmymoney"),
         Help(HelpManager.CommandGroups.AdminLevel7, "Sets your money to the specificed amount.",
             new[] {"The amount of money you want."})]
        public void setmymoney_cmd(Client player, int money)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (account.AdminLevel <= 6)
                return;

            InventoryManager.SetInventoryAmmount(character, typeof(Money), money);
            NAPI.Chat.SendChatMessageToPlayer(player, $"You have sucessfully changed your money to ${money}.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set his money to {money}");
        }

        [Command("showplayercars"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Give you a list of the vehicles a player owns.",
             new[] {"Id: The id of player."})]
        public void showplayercars_cmd(Client player, string id)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Character character = receiver.GetCharacter();
            NAPI.Chat.SendChatMessageToPlayer(player, "----------------------------------------------");
            NAPI.Chat.SendChatMessageToPlayer(player, $"Vehicles Owned By {character.CharacterName}");
            foreach (var carid in character.OwnedVehicles)
            {
                if (carid == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, $"(UNKNOWN VEHICLE) | ID ~r~{carid}~w~.");
                    continue;
                }
                NAPI.Chat.SendChatMessageToPlayer(player, $"({VehicleOwnership.returnCorrDisplayName(carid.VehModel)}) | NetHandle ~r~{carid.NetHandle.Value}~w~ | ID ~r~{carid.Id}~w~.");
            }
            NAPI.Chat.SendChatMessageToPlayer(player, "----------------------------------------------");
        }

        [Command("noobs"),
         Help(HelpManager.CommandGroups.AdminLevel2, "List of players with less than 4 playing hours.", null)]
        public void noobs_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            player.SendChatMessage("==============================");
            player.SendChatMessage("NOOB PLAYERS");
            player.SendChatMessage("==============================");
            foreach (var p in PlayerManager.Players)
            {
                if (p.GetPlayingHours() < 4)
                {
                    player.SendChatMessage(
                        $"Name: {p.CharacterName} | Id: {PlayerManager.GetPlayerId(p)} | Hours: {p.GetPlayingHours()}");
                }
            }
        }

        [Command("gotocar"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Teleport to a vehicle.", new[] {"Vehicle ID"})]
        public void gotocar_cmd(Client player, int vID)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var veh = VehicleManager.GetVehicleById(vID);

            if (veh == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That vehicle ID does not exist.");
                return;
            }

            if (veh.IsSpawned == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That vehicle is not spawned.");
                return;
            }

            NAPI.Entity.SetEntityPosition(player, NAPI.Entity.GetEntityPosition(veh.NetHandle));
            NAPI.Chat.SendChatMessageToPlayer(player, "Sucessfully teleported to a vehicle.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported vehicle #{vID} to the Admin's position.");
        }

        [Command("getcar"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Teleports a vehicle to you.", new[] {"Vehicle ID"})]
        public void getplayercar_cmd(Client player, int vID)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var veh = VehicleManager.GetVehicleById(vID);

            if (veh == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That vehicle ID does not exist.");
                return;
            }

            if (veh.IsSpawned == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "That vehicle is not spawned.");
                return;
            }

            NAPI.Entity.SetEntityPosition(veh.NetHandle, player.Position);
            NAPI.Chat.SendChatMessageToPlayer(player, "Sucessfully teleported the car to you.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has teleported vehicle #{vID} to the Admin's position.");
        }

        [Command("setadminname"),
         Help(HelpManager.CommandGroups.AdminLevel6, "Can set the admin name for admins.",
             new[] {"Id: The id of player.", "Desired name they want."})]
        public void setadminname_cmd(Client player, string id, string name)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Account receiverAccount = receiver.GetAccount();
            if (account.AdminLevel < 6)
                return;

            if (receiverAccount.AdminLevel == 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This player is not an admin.");
                return;
            }

            receiverAccount.AdminName = name;
            NAPI.Chat.SendChatMessageToPlayer(player,
                "You have set " + receiver.Nametag + "'s admin name to '" + name + "'.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, receiver.Nametag + " has set your admin name to '" + name + "'.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} Admin name to {name}");
        }

        [Command("admins"), Help(HelpManager.CommandGroups.General, "A list of all admins online at the moment.", null)]
        public void admins_cmd(Client player)
        {
            NAPI.Chat.SendChatMessageToPlayer(player, "=====ADMINS ONLINE NOW=====");
            foreach (var c in API.GetAllPlayers())
            {
                if (c == null)
                    continue;

                Account receiverAccount = c.GetAccount();

                if (receiverAccount?.AdminLevel > 0)
                {
                    if (receiverAccount.AdminDuty)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player,
                            "~g~[ONDUTY] " + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player,
                            "~r~[OFFDUTY] " + receiverAccount.AdminName + " | LEVEL " + receiverAccount.AdminLevel);
                    }
                }
            }
        }

        [Command("adminduty"),
         Help(HelpManager.CommandGroups.AdminLevel1, "Can go on and off of admin duty with this.", null)]
        public void adminduty_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 1)
                return;

            Character character = player.GetCharacter();

            if (account.AdminDuty == false)
            {
                account.AdminDuty = true;
                API.SetPlayerNametagColor(player, 51, 102, 255);
                API.SetPlayerNametag(player, account.AdminName + " (" + PlayerManager.GetPlayerId(character) + ")");
                NAPI.Chat.SendChatMessageToPlayer(player, "You are now on admin duty.");
                SendtoAllAdmins($"{account.AdminName} has gone on admin duty.");
                return;
            }

            account.AdminDuty = false;
            API.SetPlayerNametag(player, character.CharacterName + " (" + PlayerManager.GetPlayerId(character) + ")");
            API.ResetPlayerNametagColor(player);
            NAPI.Chat.SendChatMessageToPlayer(player, "You are no longer on admin duty.");
            SendtoAllAdmins($"{account.AdminName} has gone off admin duty.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} became on admin duty.");
        }

        [Command("whereami"),
         Help(HelpManager.CommandGroups.General, "Give you your current location in X,Y,Z format.", null)]
        public void GetPlayerLocation(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel == 0)
            {
            }
            else
            {
                Vector3 CurrentPlayerPos = NAPI.Entity.GetEntityPosition(player);
                Vector3 CurrentPlayerRot = API.GetEntityRotation(player);
                uint playerDimension = API.GetEntityDimension(player);
                NAPI.Chat.SendChatMessageToPlayer(player, "-----Current Position-----");
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "X: " + CurrentPlayerPos.X + " Y: " + CurrentPlayerPos.Y + " Z: " + CurrentPlayerPos.Z +
                    " Dimension: " + playerDimension);
                NAPI.Util.ConsoleOutput(
                    $"POSITION: new Vector3({CurrentPlayerPos.X}, {CurrentPlayerPos.Y}, {CurrentPlayerPos.Z})");
                NAPI.Util.ConsoleOutput(
                    $"ROTATION: new Vector3({CurrentPlayerRot.X}, {CurrentPlayerRot.Y}, {CurrentPlayerRot.Z})");
            }

        }






        //============REPORT SYSTEM=============

        [Command("report", Alias = "re", GreedyArg = true),
         Help(HelpManager.CommandGroups.General,
             "Use this to make an ingame report for an admin to sort or to speak to an admin about an issue.", null)]
        public void report_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.ReportMuteExpires > TimeManager.GetTimeStamp)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (AdminReports.Reports.Count > 5)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "~r~We are experiencing a high volume of reports. Please only use the report feature if it is absolutely necessary.");
            }

            if (character.ReportCreated)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Please wait 15 seconds before creating another report.");
                return;
            }
            NAPI.ClientEvent.TriggerClientEvent(player, "show_report_menu");
        }

        [Command("reports", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "View all current ingame reports made.", null)]
        public void reports_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;


            NAPI.Chat.SendChatMessageToPlayer(player, "=======REPORTS=======");
            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Type == 1)
                {
                    return;
                }

                if (i.Type == 2)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player,
                        "~b~" + i.Name + " reported ~r~" + i.Target + "~w~" + " | " + i.ReportMessage);
                    return;
                }
                NAPI.Chat.SendChatMessageToPlayer(player, "~b~" + i.Name + "~w~" + " | " + i.ReportMessage);
            }
        }

        [Command("acceptreport", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "For you to take on a report.",
             new[] {"Id: The id of target player."})]
        public void acceptreport_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 2)
                return;


            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveReport == false)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active reports.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.Nametag)
                {
                    AdminReports.Delete(i);
                }
            }

            receivercharacter.HasActiveReport = false;
            NAPI.Chat.SendChatMessageToPlayer(player, "Report accepted.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, "Your report has been taken by ~b~" + player.Nametag + ".");
            SendtoAllAdmins("[REPORT] " + player.Nametag + " has taken " + receiver.Nametag + "'s report.");
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has accepted report made by {GetLogName(receiver)}");
        }

        [Command("trashreport", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "For you to get rid of a report.",
             new[] {"Id: The id of target player."})]
        public void trashreport_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 1)
                return;


            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveReport == false || receivercharacter.HasActiveAsk)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active reports.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.Nametag)
                {
                    AdminReports.Delete(i);
                }
            }
            receivercharacter.HasActiveReport = false;
            NAPI.Chat.SendChatMessageToPlayer(player, "Request trashed.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, "Your admin/moderator request was trashed.");
            SendtoAllAdmins("[TRASHED] " + player.Nametag + " trashed " + receiver.Nametag + "'s report.");
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has trashed report by {GetLogName(receiver)}");
        }

        [Command("maccept", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "For you to take on an ask.",
             new[] {"Id: The id of target player."})]
        public void maccept_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character character = player.GetCharacter();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 1)
                return;

            if (character.IsOnAsk)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are already helping someone.");
                return;
            }

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.HasActiveAsk == false)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ This player has no active help requests.");
                return;
            }

            foreach (var i in AdminReports.Reports.ToList())
            {
                if (i.Name == receiver.Nametag)
                {
                    AdminReports.Delete(i);
                }
            }

            account.AdminDuty = true;
            character.IsOnAsk = true;
            character.LastPos = NAPI.Entity.GetEntityPosition(player);
            receivercharacter.HasActiveAsk = false;
            NAPI.Chat.SendChatMessageToPlayer(player, "Ask accepted.");
            NAPI.Chat.SendChatMessageToPlayer(receiver, "Your help request has been taken by ~b~" + player.Nametag + ".");
            SendtoAllAdmins("[REPORT] " + player.Nametag + " has taken " + receiver.Nametag + "'s ask request.");
            player.SendChatMessage(
                $"{receivercharacter.CharacterName}'s playing hours: ~g~{receivercharacter.GetPlayingHours()}.");
            NAPI.Entity.SetEntityPosition(player, NAPI.Entity.GetEntityPosition(receiver));
            API.SetPlayerNametagColor(player, 51, 102, 255);
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has accepted {GetLogName(receiver)}'s /ask.");
        }

        [Command("mfinish", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "When you're done with the player hit em with this.", null)]
        public void mfinish_cmd(Client player)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (account.AdminLevel < 1)
                return;

            if (character.IsOnAsk == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not helping anyone.");
                return;
            }

            account.AdminDuty = false;
            character.IsOnAsk = false;
            NAPI.Chat.SendChatMessageToPlayer(player, "Ask finished.");
            NAPI.Entity.SetEntityPosition(player, character.LastPos);
            API.ResetPlayerNametagColor(player);
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has finished their /ask.");
        }

        [Command("ask", GreedyArg = true),
         Help(HelpManager.CommandGroups.General, "To ask a mod/admin a question. Also useful for a checkpoint.",
             new[] {"Question for moderators"})]
        public void ask_cmd(Client player, string message)
        {
            Character character = player.GetCharacter();

            if (character.ReportMuteExpires > TimeManager.GetTimeStamp)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are muted from creating reports/ask requests.");
                return;
            }

            if (character.ReportCreated)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "Please wait 15 seconds before creating another request.");
                return;
            }

            character.HasActiveAsk = true;
            AdminReports.InsertReport(1, player.Nametag, message);
            SendtoAllAdmins("~g~[ASK]~w~ " + PlayerManager.GetName(player) + ": " + message);
            NAPI.Chat.SendChatMessageToPlayer(player,
                "~b~Ask request submitted. ~w~Moderators have been informed and will be with you soon.");
            startReportTimer(player);
        }

        [Command("nmute", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "Used to mute players in /n.",
             new[] {"Id: The id of target player."})]
        public void nmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.NMutedExpiration < TimeManager.GetTimeStamp)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have muted ~b~" + receiver.Nametag + "~w~ from newbie chat for 1 hour.");
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from newbie chat for 1 hour.");
                SendtoAllAdmins($"{account.AdminName} has muted {receiver.Nametag} from /n.");
                receivercharacter.NMutedExpiration = TimeManager.GetTimeStampPlus(TimeSpan.FromHours(1));
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from newbie chat.");
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have unmuted ~b~" + receiver.Nametag + "~w~ from newbie chat.");
                SendtoAllAdmins($"{account.AdminName} has unmuted {receiver.Nametag} from /n.");
                receivercharacter.NMutedExpiration = TimeManager.GetTimeStamp;
            }
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has nmuted {GetLogName(receiver)}");
        }

        [Command("vmute", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "Used to mute people in /v.",
             new[] {"Id: The id of target player."})]
        public void vmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.VMutedExpiration < TimeManager.GetTimeStamp)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have muted ~b~" + receiver.Nametag + "~w~ from VIP chat for 1 hour.");
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from VIP chat for 1 hour.");
                SendtoAllAdmins($"{account.AdminName} has muted {receiver.Nametag} from VIP chat.");
                receivercharacter.VMutedExpiration = TimeManager.GetTimeStampPlus(TimeSpan.FromHours(1));
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from VIP chat.");
                NAPI.Chat.SendChatMessageToPlayer(player, "You have unmuted ~b~" + receiver.Nametag + "~w~ from VIP chat.");
                SendtoAllAdmins($"{account.AdminName} has unmuted {receiver.Nametag} from VIP chat.");
                receivercharacter.VMutedExpiration = TimeManager.GetTimeStamp;
            }
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has vmuted {GetLogName(receiver)}");
        }

        [Command("reportmute", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "Used to stop people from making reports.",
             new[] {"Id: The id of target player."})]
        public void reportmute_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receivercharacter = receiver.GetCharacter();

            if (account.AdminLevel < 1)
                return;

            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receivercharacter.ReportMuteExpires < TimeManager.GetTimeStamp)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have muted ~b~" + receiver.Nametag + "~w~from creating reports.");
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~muted ~w~from making reports.");
                SendtoAllAdmins($"{account.AdminName} has muted {receiver.Nametag} from making reports.");
                receivercharacter.ReportMuteExpires = TimeManager.GetTimeStampPlus(TimeSpan.FromHours(1));
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(receiver, "You have been ~r~unmmuted ~w~from making reports.");
                SendtoAllAdmins($"{account.AdminName} has unmuted {receiver.Nametag} from making reports.");
                NAPI.Chat.SendChatMessageToPlayer(player,
                    "You have unmuted ~b~" + receiver.Nametag + "~w~from creating reports.");
                receivercharacter.ReportMuteExpires = TimeManager.GetTimeStamp;
            }
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has reportmuted {GetLogName(receiver)}");
        }

        //here toro
        [Command("mlist", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel1, "Used to list the active moderator reques.", null)]
        public void asklist_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 1)
                return;

            NAPI.Chat.SendChatMessageToPlayer(player, "=======ASK LIST=======");
            foreach (var i in AdminReports.Reports)
            {
                if (i.Type == 2 || i.Type == 3)
                {
                    return;
                }

                NAPI.Chat.SendChatMessageToPlayer(player, "~b~" + i.Name + "~w~" + " | " + i.ReportMessage);
            }
        }

        [Command("clearreports", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Clears all of the curren reports and asks", null)]
        public void clearreports_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 3)
                return;

            AdminReports.Reports.Clear();
            NAPI.Chat.SendChatMessageToPlayer(player, "All reports (including ask) have been cleared.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has trashed all reports and asks.");
        }

        //PLAYER-ADMIN STUFF

        [Command("prison", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "Places a player into prison for the specificed amount of time.",
             new[] {"ID of the target player", "Time in minutes.", "Reason for prison"})]
        public void prison_cmd(Client player, string id, string time, string reason)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            if (receiver.GetCharacter().isAJailed)
            {
                NAPI.Chat.SendChatMessageToPlayer(player,"That player is already in Admin Jail!");
                return;
            }

            receiver.GetCharacter().JailTimeLeft = int.Parse(time) * 1000 * 60;
            API.Shared.SetEntityDimension(receiver, (uint)receiver.GetCharacter().Id + 1000);
            aJailControl(receiver, int.Parse(time));
            NAPI.Chat.SendChatMessageToPlayer(player,
                "You have jailed " + receiver.Nametag + " for " + time + " minutes. Reason: " + reason);
            NAPI.Chat.SendChatMessageToPlayer(receiver,
                "You have been jailed by " + player.Nametag + " for " + time + " minutes. Reason: " + reason);
            SendtoAllAdmins(account.AdminName + " has jailed " + receiver.Nametag + " for " + time +
                            " minutes. Reason: " + reason);
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has jailed {GetLogName(receiver)} for {time} second(s).");
        }

        [Command("kickplayer", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "Kicks a player from the server",
             new[] {"ID of the target player", "The kick reason."})]
        public static void kick_cmd(Client player, string id, string reason)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has kicked {GetLogName(receiver)}.");
            KickPlayer(receiver, reason);
            SendtoAllAdmins(account.AdminName + " has kicked " + receiver.Nametag + " from the server. Reason: " +
                            reason);
        }

        [Command("remoteaw", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel2, "Admin warps a player remotely", new[] { "Character name of player"})]
        public void remoteaw_cmd(Client player, string charactername)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var receiver = DatabaseManager.CharacterTable.Find(x => x.CharacterName == charactername).FirstOrDefault();
            if (receiver == null)
                return;

            receiver.LastPos = new Vector3(429.8345, -672.5932, 29.05217);
            receiver.Save();
            player.SendChatMessage("You have remote admin warped " + receiver.CharacterName + " to newbie spawn.");
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remote warped {receiver.CharacterName}.");
        }

        [Command("remoteprison", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "Places a player into prison for the specificed amount of time.",
             new[] {"ID of the target player", "Time in minutes.", "Reason for prison"})]
        public void remoteprison_cmd(Client player, string charactername, string time, string reason)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 2)
                return;

            var receiver = DatabaseManager.CharacterTable.Find(x => x.CharacterName == charactername).SingleOrDefault();
            if (receiver == null)
                return;

            receiver.JailTimeLeft = int.Parse(time) * 1000 * 60;
            NAPI.Chat.SendChatMessageToPlayer(player,
                "You have remote jailed " + receiver.CharacterName + " for " + time + " minutes. Reason: " + reason);
            SendtoAllAdmins(account.AdminName + " has remote jailed " + receiver.CharacterName + " for " + time +
                            " minutes. Reason: " + reason);
            account.AdminActions++;
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remote jailed {receiver.CharacterName} for {time} second(s).");
        }

        [Command("remotewarn", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Applies a player warning to an offline player", new[]
             {"Account name of the target player", "The warning reason"})]
        public static void remotewarn_cmd(Client player, string accountname, string reason)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

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
                        API.Shared.SendChatMessageToPlayer(player, "This player is logged in. Use /warn.");
                        return;
                    }
                    var playerWarn = new PlayerWarns(c.AccountName, account.AccountName, reason);
                    c.PlayerWarns.Add(playerWarn);
                    account.AdminActions++;

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
                        API.Shared.SendChatMessageToPlayer(player, "Player has been tempbanned.");
                    }
                    c.Save();
                }
                API.Shared.SendChatMessageToPlayer(player,
                    "You remotewarned " + c.AccountName + " for '~r~" + reason + "~w~'.");
                Log(LogTypes.Warns,
                    $"Admin {account.AdminName}[{player.SocialClubName}] has remotewarned the player [{c.AccountName}]. Reason: '{reason}'");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remotewarned the player [{c.AccountName}]. Reason: '{reason}'");
                break;
            }
        }


        [Command("remoteban", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Bans an offline player",
             new[] {"Account name of the target player", "The ban reason"})]
        public static void remoteban_cmd(Client player, string accountname, string reason)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = player.GetAccount();

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
                API.Shared.SendChatMessageToPlayer(player,
                    "You have remote-banned " + c.AccountName + " from the server.");
                account.AdminActions++;
                Log(LogTypes.Bans,
                    $"Admin {account.AdminName}[{player.SocialClubName}] has remotebanned the player [{c.AccountName}]. Reason: '{reason}'");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has remotebanned the player [{c.AccountName}]. Reason: '{reason}'");
                break;
            }
        }

        [Command("setcp"),
         Help(HelpManager.CommandGroups.AdminLevel1, "Sends a checkpoint to a player.", "The target name or id")]
        public void sendwaypoint_cmd(Client player, string target)
        {
            var targetClient = PlayerManager.ParseClient(target);
            if (targetClient == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player isn't online.");
                return;
            }

            NAPI.ClientEvent.TriggerClientEvent(player, "GET_CP_TO_SEND", targetClient.Handle);
            NAPI.Chat.SendChatMessageToPlayer(player,
                "The checkpoint should be sent to " + targetClient.GetCharacter().CharacterName);
        }

        [Command("getcharacters", Alias = "getchars"),
         Help(HelpManager.CommandGroups.AdminLevel4, "Gets the characters of an account", new[] {"Soical club name"})]
        public void getcharacters_cmd(Client player, string accname)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            var foundAccount = DatabaseManager.AccountTable.Find(x => x.AccountName == accname).Project(x => x.Id)
                .FirstOrDefault();
            if (foundAccount == ObjectId.Empty)
            {
                player.SendChatMessage("Account not found.");
                return;
            }

            var foundCharacters = DatabaseManager.CharacterTable.Find(x => x.AccountId == foundAccount.ToString())
                .Project(x => x.CharacterName).ToList();
            if (foundCharacters.Count == 0)
            {
                player.SendChatMessage("No characters found for that account.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, $"***** Characters of {accname} *****");
            foreach (var chr in foundCharacters)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, $"** " + chr);
            }
            NAPI.Chat.SendChatMessageToPlayer(player, $"***********************************");
        }

        [Command("getaccountname", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel4, "Gets the account name of a character",
             new[] {"Character name for the account"})]
        public void getaccountname_cmd(Client player, string charactername)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            var foundCharacter = DatabaseManager.CharacterTable.Find(x => x.CharacterName == charactername)
                .FirstOrDefault();
            if (foundCharacter == null)
            {
                player.SendChatMessage("Character not found.");
                return;
            }

            var foundAccount = DatabaseManager.AccountTable.Find(x => x.Id == ObjectId.Parse(foundCharacter.AccountId))
                .FirstOrDefault();
            if (foundAccount == null)
            {
                player.SendChatMessage("Account not found.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player,
                charactername + "'s account name is '" + foundAccount.AccountName + "'.");
        }

        [Command("remotestats", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Gets the stats of a character", new[] {"Character name"})]
        public void remotestats_cmd(Client player, string charactername)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            var foundCharacter = DatabaseManager.CharacterTable.Find(x => x.CharacterName == charactername)
                .FirstOrDefault();
            if (foundCharacter == null)
            {
                player.SendChatMessage("Character not found.");
                return;
            }

            var foundAccount = DatabaseManager.AccountTable.Find(x => x.Id == ObjectId.Parse(foundCharacter.AccountId))
                .FirstOrDefault();
            if (foundAccount == null)
            {
                player.SendChatMessage("Account not found.");
                return;
            }

            PlayerManager.ShowStats(player, foundCharacter, foundAccount);
        }

        [Command("changename", GreedyArg = false),
         Help(HelpManager.CommandGroups.AdminLevel2, "Change a player's character name.",
             new[] {"ID of the target player", "New name"})]
        public static void forcechangename_cmd(Client player, string id, string name)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid target.");
                return;
            }

            Character receiverCharacter = receiver.GetCharacter();

            receiverCharacter.CharacterName = name;
            receiverCharacter.Save();
            receiverCharacter.update_nametag();
        }

        [Command("unban", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Unbans an account from the server",
             new[] {"Account name of the target player"})]
        public static void unban_cmd(Client player, string accountname)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = player.GetAccount();

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
                account.AdminActions++;
                API.Shared.SendChatMessageToPlayer(player, "You have unbanned " + c.AccountName + " from the server.");
                SendtoAllAdmins(account.AdminName + " has unbanned " + c.AccountName + " from the server.");
                Log(LogTypes.Unbans,
                    $"Admin {account.AdminName}[{player.SocialClubName}] has unbanned the player [{c.AccountName}].");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has unbanned {c.AccountName}");
                break;
            }
        }

        [Command("untempban", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Removes an accouns tempban", new[] {"ID of the target player"})]
        public static void untempban_cmd(Client player, string accountname)
        {
            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();
            Account account = player.GetAccount();

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
                API.Shared.SendChatMessageToPlayer(player,
                    "You have un-tempbanned " + c.AccountName + " from the server.");
                Log(LogTypes.Unbans,
                    $"Admin {account.AdminName}[{player.SocialClubName}] has untempbanned the player [{c.AccountName}].");
                Log(LogTypes.AdminActions,
                    $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has untempbanned {c.AccountName}");
                break;
            }
        }

        [Command("banplayer", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel2, "Bans a player from the server",
             new[] {"ID of the target player", "Ban reason"})]
        public static void ban_cmd(Client player, string id, string reason)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            account.AdminActions++;
            var receiver = PlayerManager.ParseClient(id);
            BanPlayer(receiver, reason);
            SendtoAllAdmins(account.AdminName + " has banned " + receiver.GetAccount().AccountName +
                            " from the server. Reason: " + reason);
            Log(LogTypes.Bans,
                $"Admin {account.AdminName}[{player.SocialClubName}] has banned the player {receiver.GetCharacter().CharacterName}[{receiver.SocialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has banned {GetLogName(receiver)} for <{reason}>");
        }


        [Command("warn", GreedyArg = false),
         Help(HelpManager.CommandGroups.AdminLevel2, "Applies a player warning to a player", new[]
             {"ID of the target player", "The reason for the warning"})]
        public static void warn_cmd(Client player, string id, string reason)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.Shared.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid target.");
                return;
            }
            Account receiverAccount = receiver.GetAccount();
            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            var playerWarn = new PlayerWarns(receiverAccount.AccountName, account.AccountName, reason);
            receiverAccount.PlayerWarns.Add(playerWarn);
            account.AdminActions++;

            API.Shared.SendChatMessageToPlayer(player,
                "You warned ~r~" + receiverCharacter.CharacterName + "~w~ for '~r~" + reason + "~w~'.");
            API.Shared.SendChatMessageToPlayer(receiver,
                "You were warned by ~r~" + character.CharacterName + "~w~ for '~r~" + reason + "~w~'.");
            SendtoAllAdmins(account.AdminName + " warned " + receiverCharacter.CharacterName + " for " + reason);
            Log(LogTypes.Warns,
                $"Admin {account.AdminName}[{player.SocialClubName}] has warned the player {receiver.GetCharacter().CharacterName}[{receiver.SocialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has warned {GetLogName(receiver)} for <{reason}>");

            if (receiverAccount.PlayerWarns.Count() >= 3)
            {
                API.Shared.SendNotificationToPlayer(player, "You were temporarily banned for reaching 3 player warns.");
                AddTempBanLevel(receiver);
                TempBanPlayer(receiver);
                receiverAccount.PlayerWarns.Clear();
                API.Shared.KickPlayer(receiver);
            }
        }

        [Command("removewarns", GreedyArg = false),
         Help(HelpManager.CommandGroups.AdminLevel2, "Removes all of a players warns", new[]
             {"ID of the target player", "The reason for removing them"})]
        public static void removewarns_cmd(Client player, string id, string reason)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
                return;

            Account receiverAccount = receiver.GetAccount();
            Character character = player.GetCharacter();
            Character receiverCharacter = receiver.GetCharacter();

            foreach (var w in receiverAccount.PlayerWarns)
            {
                receiverAccount.PlayerWarns.Remove(w);
            }

            account.AdminActions++;
            API.Shared.SendChatMessageToPlayer(player,
                "You removed all warns from ~r~" + receiverCharacter.CharacterName + "~w~.");
            API.Shared.SendChatMessageToPlayer(receiver,
                "Your warns were removed by ~r~" + character.CharacterName + "~w~.");
            Log(LogTypes.Bans,
                $"Admin {account.AdminName}[{player.SocialClubName}] has removed all the warns of the player {receiver.GetCharacter().CharacterName}[{receiver.SocialClubName}]. Reason: '{reason}'");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has removed all warns of {GetLogName(receiver)}");
        }

        [Command("remotesetadminlevel"),
         Help(HelpManager.CommandGroups.AdminLevel2, "Change an offline player's admin level.",
             new[] {"Account name of the player"})]
        public void remotesetadminlevel_cmd(Client player, string accountname, int level)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 7)
                return;

            var filter = Builders<Account>.Filter.Eq("AccountName", accountname);
            var foundAccount = DatabaseManager.AccountTable.Find(filter).ToList();

            foreach (var c in foundAccount)
            {
                if (c.AccountName == accountname)
                {
                    if (account.AdminLevel >= c.AdminLevel && c.AdminLevel > level)
                    {
                        var oldLevel = c.AdminLevel;
                        NAPI.Chat.SendChatMessageToPlayer(player,
                            "You have changed " + c.AccountName + "'s admin level to " + level + " (was " + oldLevel +
                            ").");
                        c.AdminLevel = level;
                        c.Save();

                    }
                    else
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, Color.White,
                            "You cannot set a higher admin level than yours or set someone to a level above yours.");
                    }

                }
                break;
            }
        }

        [Command("remoteplayerwarns"),
         Help(HelpManager.CommandGroups.AdminLevel2, "View an offline player's warnings",
             new[] {"Account name of the player"})]
        public static void remoteplayerwarns_cmd(Client player, string accountname)
        {
            Account account = player.GetAccount();

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
                        API.Shared.SendChatMessageToPlayer(player,
                            "Warning #" + j + " | ~r~Reason:~w~ " + warn.WarnReason + " | ~r~Given by:~w~ " +
                            warn.WarnSender);
                        j++;
                    }
                    if (c.PlayerWarns.Count == 0)
                    {
                        API.Shared.SendChatMessageToPlayer(player, "No warnings to show.");
                    }
                    API.Shared.SendChatMessageToPlayer(player, "~r~Tempban level:~w~ " + c.TempbanLevel);

                }
                break;
            }
        }


        [Command("playerwarns", GreedyArg = false),
         Help(HelpManager.CommandGroups.General, "View your player warnings",
             new[] {"ID of the target player <strong>[ADMIN ONLY]</strong>"})]
        public static void playerwarns_cmd(Client player, string id = null)
        {
            var receiver = PlayerManager.ParseClient(id);

            Account account = player.GetAccount();

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

        [Command("respawnveh", GreedyArg = false),
         Help(HelpManager.CommandGroups.AdminLevel4, "Respawns a vehicle.", "Id of vehicle to respawn. (OPTIONAL)")]
        public void respawnveh_cmd(Client player, int id = 0)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
            {
                return;
            }

            if (id == 0 && !player.IsInVehicle)
            {
                player.SendChatMessage("You must enter a vehicle ID.");
                return;
            }

            GameVehicle veh = null;

            if (player.IsInVehicle)
            {
                veh = VehicleManager.GetVehFromNetHandle(NAPI.Player.GetPlayerVehicle(player));
            }

            if (veh == null)
            {
                veh = VehicleManager.GetVehicleById(id);
                if (veh == null)
                {
                    player.SendChatMessage("Invalid vehicle ID.");
                    return;
                }
            }

            VehicleManager.respawn_vehicle(veh);
            player.SendChatMessage("Vehicle respawned.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has respawned vehicle id {veh.Id}");
        }


        public static void ShowWarns(Client player, Client receiver = null)
        {

            Account account = player.GetAccount();
            if (receiver != null)
            {
                account = receiver.GetAccount();
            }


            int j = 1;
            foreach (var warn in account.PlayerWarns)
            {
                API.Shared.SendChatMessageToPlayer(player,
                    "Warning #" + j + " | ~r~Reason:~w~ " + warn.WarnReason + " | ~r~Given by:~w~ " + warn.WarnSender);
                j++;
            }
            if (account.PlayerWarns.Count == 0)
            {
                API.Shared.SendChatMessageToPlayer(player, "No warnings to show.");
            }
            API.Shared.SendChatMessageToPlayer(player, "~r~Tempban level:~w~ " + account.TempbanLevel);
        }

        public static void AddTempBanLevel(Client player)
        {
            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            account.TempbanLevel += 1;
            if (account.TempbanLevel >= 3)
            {
                BanPlayer(player, "You reached the maximum tempban level (3)");
            }
        }

        public static void TempBanPlayer(Client player)
        {
            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            if (account.TempbanLevel == 1)
            {
                API.Shared.SendChatMessageToPlayer(player,
                    "3 player warns reached. You have been temporarily banned for 3 days");
                account.TempBanExpiration = DateTime.Now.AddDays(3);
                account.IsTempbanned = true;
                return;
            }
            if (account.TempbanLevel == 2)
            {
                API.Shared.SendChatMessageToPlayer(player,
                    "3 player warns reached. You have been temporarily banned for 7 days");
                account.TempBanExpiration = DateTime.Now.AddDays(7);
                account.IsTempbanned = true;
            }
        }

        public static void KickPlayer(Client player, string reason)
        {
            API.Shared.KickPlayer(player);
            API.Shared.SendNotificationToPlayer(player, "You were kicked from the server for ~r~'" + reason + "~w~'.");
            API.Shared.SendChatMessageToPlayer(player, "You were kicked from the server for ~r~'" + reason + "~w~'.");
        }

        public static void BanPlayer(Client player, string reason)
        {
            Account account = player.GetAccount();

            account.BanReason = reason;
            account.IsBanned = true;
            API.Shared.SendNotificationToPlayer(player, "You were banned from the server for ~r~'" + reason + "~w~'.");
            API.Shared.SendChatMessageToPlayer(player, "You were banned from the server for ~r~'" + reason + "~w~'.");
            API.Shared.KickPlayer(player);
        }

        public static void UnbanPlayer(Client player)
        {
            Account account = player.GetAccount();

            account.IsBanned = false;
        }

        [Command("setcharacterslots"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Set the amount of character slots a player may have", new[]
             {"ID of the target player", "The amount of character slots permitted"})]
        public void setcharacterslots(Client player, string id, int slots)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Account account = player.GetAccount();
            Account receiverAccount = receiver.GetAccount();

            if (account.AdminLevel < 3)
            {
                return;
            }

            receiverAccount.CharacterSlots = slots;
            receiverAccount.Save();
            player.SendChatMessage(
                $"You have set {receiver.GetCharacter().CharacterName}'s character slots to {slots}.");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} character slots to {slots}");
        }

        [Command("changeviplevel"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Change a players VIP level", new[]
             {"ID of the target player", "The VIP level to change to", "The VIP amount in days"})]
        public void changeviplevel_cmd(Client player, string id, int level, int days)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Account account = player.GetAccount();
            Account receiverAccount = receiver.GetAccount();

            if (account.AdminLevel < 3)
            {
                return;
            }

            if (receiverAccount.AdminLevel > 0)
            {
                receiverAccount.VipLevel = 3;
                receiverAccount.Save();
                receiver.SendChatMessage("Your ~y~VIP~y~ level was set to " + 3 + " by " + account.AdminName + ".");
                return;

            }

            if (receiverAccount.VipLevel == level)
            {
                player.SendChatMessage("This player is already this VIP level.");
                return;
            }

            if (level > 3)
            {
                player.SendChatMessage("Max VIP level is 3.");
                return;
            }
            receiverAccount.VipLevel = level;

            receiverAccount.VipExpirationDate = DateTime.Now.AddDays(days);
            receiverAccount.Save();

            account.AdminActions++;
            receiver.SendChatMessage("Your ~y~VIP~y~ level was set to " + level + " by " + account.AdminName + ".");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has set {GetLogName(receiver)} VIP level to {level} for {days} days.");

            if (level > 0)
            {
                foreach (var p in API.GetAllPlayers())
                {
                    if (p == null)
                        continue;

                    Account paccount = p.GetAccount();

                    if (paccount.VipLevel > 0)
                    {
                        p.SendChatMessage(receiver.GetCharacter().rp_name() + " has become a level " + level +
                                          " ~y~VIP~y~!");
                    }
                }
            }
        }

        [Command("addviptime", GreedyArg = true),
         Help(HelpManager.CommandGroups.AdminLevel3, "Adds VIP time to a specific players VIP", new[]
             {"ID of the target player", "The amount to add in days"})]
        public void addviptime_cmd(Client player, string id, string days)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            Account account = player.GetAccount();
            Account receiverAccount = receiver.GetAccount();

            if (account.AdminLevel < 3)
            {
                player.SendChatMessage("You cannot set the VIP time of an administrator.");
                return;
            }

            account.VipExpirationDate = account.VipExpirationDate.AddDays(int.Parse(days));
            account.Save();
            account.AdminActions++;
            receiver.SendChatMessage("Your ~y~VIP~y~ days were increased by " + int.Parse(days) + " days by " +
                                     account.AdminName + "!");
            Log(LogTypes.AdminActions,
                $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}] Admin {account.AdminName} has increased {GetLogName(receiver)} VIP time by {days} days.");
        }

        [Command("closestveh"), Help(HelpManager.CommandGroups.AdminLevel3, "Sets you into the closest vehicle.")]
        public void closestveh_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 2)
                return;

            var ClosestVeh = GetClosestVeh(player);
            if (ClosestVeh == null)
            {
                player.SendChatMessage("There are no nearby vehicles.");
                return;
            }

            NAPI.Player.SetPlayerIntoVehicle(player, ClosestVeh, -1);
        }

        public void startReportTimer(Client player)
        {
            Character senderchar = player.GetCharacter();
            senderchar.ReportCreated = true;
            senderchar.ReportTimer = new Timer {Interval = 15000};
            senderchar.ReportTimer.Elapsed += delegate { ReportTimer(player); };
            senderchar.ReportTimer.Start();
        }

        public static void SendtoAllAdmins(string text)
        {
            foreach (var c in API.Shared.GetAllPlayers())
            {
                if (c == null)
                    continue;

                Account receiverAccount = c.GetAccount();

                if (receiverAccount == null)
                    return;

                if (receiverAccount.AdminLevel > 0)
                {
                    API.Shared.SendChatMessageToPlayer(c, Color.AdminChat, text);
                }
            }
        }

        public void ReportTimer(Client player)
        {
            Character character = player.GetCharacter();

            character.ReportCreated = false;
            character.ReportTimer.Stop();
        }

        // TODO: convert to Vehicle instead of NetHandle
        public NetHandle GetClosestVeh(Client player)
        {
            var shortestDistance = 2000f;
            NetHandle closestveh = new NetHandle();
            foreach (var veh in NAPI.Pools.GetAllVehicles())
            {
                Vector3 Position = NAPI.Entity.GetEntityPosition(veh);
                var VehicleDistance = player.Position.DistanceTo(Position);
                if (VehicleDistance < shortestDistance)
                {
                    shortestDistance = VehicleDistance;
                    closestveh = veh;
                }
            }
            return closestveh;
        }

        [Command("forceredochar"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Force someone to redo character creation.",
             new[] {"Target player id."})]
        public void RedoCharacterSelection(Client player, string target)
        {
            if (player.GetAccount().AdminLevel >= 2)
            {
                var targetClient = PlayerManager.ParseClient(target);
                if (targetClient == null)
                    return;

                targetClient.SetData("REDOING_CHAR", true);
                NAPI.Player.FreezePlayer(targetClient, true);
                NAPI.Entity.SetEntityDimension(targetClient, (uint)targetClient.GetCharacter().Id + 1000);
                API.SetEntitySharedData(targetClient, "REG_DIMENSION", targetClient.GetCharacter().Id + 1000);
                targetClient.GetCharacter().Model.SetDefault();
                NAPI.ClientEvent.TriggerClientEvent(targetClient, "show_character_creation_menu");
            }
        }

        [Command("testtext"),
         Help(HelpManager.CommandGroups.AdminLevel3, "Goes into testing on-screen text position.",
             new[] {"Text to display."})]
        public void TestText(Client player, string text = "")
        {
            if (player.GetAccount().AdminLevel >= 3)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "texttest_settext", text);
            }
        }

        [Command("giveitem"),
         Help(HelpManager.CommandGroups.AdminLevel4,
             "Gives an inventory item to a player.<br/> <strong>This could cause problems with the player, use with caution.</strong>",
             new[] {"Target ID or name.", "Item name, you can get this from a dev.", "Amount to give."})]
        public void GiveItem(Client player, string target, string item, int amount)
        {
            if (player.GetAccount().AdminLevel < 4)
            {
                return;
            }

            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var type = InventoryManager.ParseInventoryItem(item);
            if (type == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This item doesn't exist. Ask a dev if you're stuck!");
                return;
            }

            var itema = InventoryManager.ItemTypeToNewObject(type);
            var x = InventoryManager.GiveInventoryItem(targetPlayer.GetCharacter(), itema, amount);
            switch (x)
            {
                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                    NAPI.Chat.SendChatMessageToPlayer(player,"Target player doesn't have space in their inventory for that.");
                    return;
                case InventoryManager.GiveItemErrors.MaxAmountReached:
                    NAPI.Chat.SendChatMessageToPlayer(player, "Target player will go past the max amount of " + item + "! Unable to give item.");
                    return;
                case InventoryManager.GiveItemErrors.HasSimilarItem:
                    NAPI.Chat.SendChatMessageToPlayer(player,"Target player alreadyy has one of these!");
                    return;
                case InventoryManager.GiveItemErrors.Success:
                    NAPI.Chat.SendChatMessageToPlayer(player, "Done.");
                    Log(LogTypes.AdminActions,
                        $"[/{MethodBase.GetCurrentMethod().GetCustomAttributes(typeof(CommandAttribute), false)[0].CastTo<CommandAttribute>().CommandString}]" +
                        $" Admin {player.GetAccount().AdminName} has given {GetLogName(targetPlayer)} an item {item}, amount: {amount}");
                    return;
                default:
                    NAPI.Chat.SendChatMessageToPlayer(player,"An unknown error occured.");
                    return;
            }
        }


        public static void aJailControl(Client target, int seconds)
        {
            Character targetChar = target.GetCharacter();
            WeaponManager.RemoveAllPlayerWeapons(target);
            API.Shared.SetEntityPosition(target, aJailLoc);
            targetChar.isAJailed = true;
            API.Shared.SendChatMessageToPlayer(target, "You have been Admin Jailed for " + (targetChar.aJailTimeLeft / 1000 / 60) + " minutes ");
            targetChar.aJailTimeLeftTimer = new Timer {Interval = 1000};
            targetChar.aJailTimeLeftTimer.Elapsed += delegate { aUpdateTimer(target); };
            targetChar.aJailTimeLeftTimer.Start();
            targetChar.aJailTimer = new Timer { Interval = targetChar.aJailTimeLeft };
            targetChar.aJailTimer.Elapsed += delegate { aSetFree(target); };
            targetChar.aJailTimer.Start();
            API.Shared.SetPlayerHealth(target, 100);
            API.Shared.SetEntityDimension(target, (uint)targetChar.Id+1000);

        }

        private static void aUpdateTimer(Client sender)
        {
            Character c = sender.GetCharacter();
            c.aJailTimeLeft -= 1000;
        }

        private static void aSetFree(Client sender)
        {
            Character c = sender.GetCharacter();
            if (!c.isAJailed) return;
            c.JailTimeLeft = 0;
            API.Shared.SendChatMessageToPlayer(sender, "You're able to leave. Read the rules in future to avoid admin jail.");
            c.isAJailed = false;
            API.Shared.SetEntityDimension(sender, 0);
            API.Shared.SetEntityPosition(sender, Lspd.FreeJail);
            c.aJailTimer.Stop();
            c.aJailTimeLeftTimer.Stop();

        }


    }
}
