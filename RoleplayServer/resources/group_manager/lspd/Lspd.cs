using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using System.Linq;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.group_manager.lspd
{
    class Lspd : Script
    {

        public Lspd()
        {
            API.onResourceStart += startLspd;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        //arrest locatio. TODO: Make it work with MarkerZone!!!!
        public static readonly Vector3 arrest_loc = new Vector3(468.7845f, -1015.69f, 26.38641f);

        //Jails
        public static readonly Vector3 jailOne = new Vector3(458.0021f, -1001.581f, 24.91485f);
        public static readonly Vector3 jailTwo = new Vector3(458.7058f, -998.1188f, 24.91487f);
        public static readonly Vector3 jailThree = new Vector3(459.6695f, -994.0704f, 24.91487f);

        //Jail Shapes
        public ColShape arrestShape;

        public static readonly Vector3 freeJail = new Vector3(427.7434f, -976.0182f, 30.70999f);


        public static Dictionary<Client, long> jailTimer = new Dictionary<Client, long>();


        public void startLspd()
        {

            //Bounds
            var jailShapeOne = API.createSphereColShape(jailOne, 3.7f);
            var jailShapeTwo = API.createSphereColShape(jailTwo, 3.7f);
            var jailShapeThree = API.createSphereColShape(jailThree, 3.7f);

            arrestShape = API.createSphereColShape(arrest_loc, 3.7f);

            API.createMarker(2, arrest_loc, new Vector3(), new Vector3(), new Vector3(0.5, 0.5, 0.5), 255, 255, 255, 255);
        }
        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "LSPD_Menu_Change_Clothes":
                { 
                    Character c = API.getEntityData(player.handle, "Character");

                    if (c.Group.CommandType != Group.CommandTypeLspd)
                    {
                        return;
                    }

                    c.IsInPoliceUniform = !c.IsInPoliceUniform;
                    ChatManager.AmeLabelMessage(player,
                        "changes into their " + (c.IsInPoliceUniform ? "police uniform." : "civilian clothing."), 8000);
                    c.update_ped();
                    c.Save();

                    break;
                }
                case "LSPD_Menu_Toggle_Duty":
                {
                    Character c = API.getEntityData(player.handle, "Character");

                    if (c.Group.CommandType != Group.CommandTypeLspd)
                    {
                        return;
                    }

                    c.IsOnPoliceDuty = !c.IsOnPoliceDuty;
                    GroupManager.SendGroupMessage(player,
                        c.CharacterName + " is now " + (c.IsOnPoliceDuty ? "on" : "off") + " police duty.");
                    c.Save();

                    break;
                }
                case "LSPD_Menu_Equip_Standard_Equipment":
                {
                    Character c = API.getEntityData(player.handle, "Character");

                    if (c.Group.CommandType != Group.CommandTypeLspd)
                    {
                        return;
                    }

                    GiveLspdEquipment(player);
                    API.sendChatMessageToPlayer(player, Color.White, "You have been given the standard LSPD equipment.");

                    break;
                }
                case "LSPD_Menu_Equip_SWAT_Equipment":
                {
                    Character c = API.getEntityData(player.handle, "Character");

                    if (c.Group.CommandType != Group.CommandTypeLspd)
                    {
                        return;
                    }

                    GiveLspdEquipment(player, 1);
                    API.sendChatMessageToPlayer(player, Color.White, "You have been given the standard SWAT equipment.");
                    break;
                }
                case "toggle_megaphone_key":
                    {
                        megaphonetog_cmd(player);
                        break;
                    }
            }
        }

        [Command("recordcrime", GreedyArg = true)]
        public void recordcrimes_cmd(Client player, string id, string crime)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.getEntityData(receiver.handle, "Character");
            CriminalRecord criminalrecord = API.getEntityData(receiver.handle, "CriminalRecord");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.sendNotificationToPlayer(player, "You have recorded " + receiver.name + " for committing a crime.");
            API.sendNotificationToPlayer(receiver, player.name + " has recorded a crime you committed: ~r~" + crime + "~w~.");
            criminalrecord.ActiveCrime = true;
            criminalrecord.Crime = crime;
            criminalrecord.CharacterId = id;
            criminalrecord.OfficerId = player.name;
            criminalrecord.Insert();

            
        }
        [Command("arrest", GreedyArg = true)] // arrest command
        public void arrest_cmd(Client player, string id, int time)
        {

            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.getEntityData(receiver.handle, "Character");
            CriminalRecord criminalrecord = API.getEntityData(receiver.handle, "CriminalRecord");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            if (criminalrecord.ActiveCrime == false)
            {
                API.sendChatMessageToPlayer(player, "You must record this player's crimes before they can be arrested.");
            }
            if (character.GroupId == receiverCharacter.GroupId)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You cannot arrest a member of the LSPD.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (arrestPointCheck(player))
            {
                API.sendNotificationToPlayer(player, "~r~You are too far away from the arrest point.");
                return;
            }

            API.sendNotificationToPlayer(player, "You have arrested ~b~" + receiver.name + "~w~.");
            API.sendNotificationToPlayer(receiver, "You have been arrested by ~b~" + player.name + "~w~.");
            criminalrecord.ActiveCrime = false;
            jailControl(receiver, time);

        }

        [Command("frisk", GreedyArg = true)]
        public void frisk_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't frisk yourself!");
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            //TODO: check items on player (first implement inv system).
        }

        [Command("backupbeacon", GreedyArg = true)]
        public void backupbeacon_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }
            API.sendNotificationToPlayer(player, "~b~Backup beacon deployed~w~. Available officers have been notified.");

            //Update beacon 5 seconds for 60 seconds. Reset beacon to false after 60.
            character.BeaconSet = true;
            character.BeaconCreator = player;
            character.BeaconResetTimer = new System.Timers.Timer { Interval = 60 };
            character.BeaconResetTimer.Elapsed += delegate { resetBeacon(player); };
            character.BeaconResetTimer.Start();

        }
 
        [Command("acceptbeacon", GreedyArg = true)]
        public void acceptbeacon_cmd(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            var beaconCreator = player;
            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            foreach (var c in PlayerManager.Players)
            {
                if (c.GroupId == character.GroupId && c.BeaconSet == true)
                {
                    beaconCreator = c.BeaconCreator;
                }

            }

            if (beaconCreator == player)
            {
                return;
            }

            API.sendNotificationToPlayer(player, "~b~Beacon accepted~w~. A marker has been added to your map.");
            character.BeaconTimer = new System.Timers.Timer { Interval = 5 };
            character.BeaconTimer.Elapsed += delegate { beaconDeployer(player, beaconCreator); };
            character.BeaconTimer.Start();
        }

        [Command("megaphonetoggle", Alias ="mp", GreedyArg = true)]
        public void megaphonetog_cmd(Client player)
        {
            var playerPos = API.getEntityPosition(player);
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }
            if (API.getEntityData(player, "MegaphoneStatus") != true)
            {
                API.sendNotificationToPlayer(player, "You are speaking through a megaphone", true);
                API.setEntityData(player, "MegaphoneStatus", true);
                var megaphone = API.createObject(API.getHashKey("prop_megaphone_01"), playerPos, new Vector3());
                API.attachEntityToEntity(megaphone, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(0, 0, 0));

            }
            API.sendNotificationToPlayer(player, "You are no longer speaking through a megaphone.");
            API.setEntityData(player, "MegaphoneStatus", false);
            API.deleteObject(player, playerPos, API.getHashKey("prop_megaphone_01"));
        }


        [Command("locker")]
        public void locker_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be in the LSPD to use this command.");
                return;
            }

            if (player.position.DistanceTo(character.Group.Locker.Location) > 8)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not in the LSPD locker room.");
                return;
            }


            API.triggerClientEvent(player, "show_lspd_locker");
        }
  
        public void GiveLspdEquipment(Client player, int type = 0)
        {
            API.removeAllPlayerWeapons(player);

            switch (type)
            {
                case 0:
                    API.givePlayerWeapon(player, WeaponHash.StunGun, 1, false, true);
                    API.givePlayerWeapon(player, WeaponHash.Nightstick, 1, false, true);
                    API.givePlayerWeapon(player, WeaponHash.Pistol, 250, false, true);
                    API.givePlayerWeapon(player, WeaponHash.Flashlight, 1, false, true);

                    API.setPlayerWeaponTint(player, WeaponHash.Pistol, WeaponTint.LSPD);
                    break;
                case 1:
                    API.givePlayerWeapon(player, WeaponHash.CombatPistol, 250, false, true);
                    API.givePlayerWeapon(player, WeaponHash.CombatPDW, 300, false, true);
                    API.givePlayerWeapon(player, WeaponHash.SmokeGrenade, 3, false, true);
                    API.givePlayerWeapon(player, WeaponHash.BZGas, 3, false, true);

                    API.setPlayerWeaponTint(player, WeaponHash.CombatPistol, WeaponTint.LSPD);
                    API.setPlayerWeaponTint(player, WeaponHash.CombatPDW, WeaponTint.LSPD);
                    break;
            }
            API.setPlayerHealth(player, 100);
            API.setPlayerArmor(player, 100);
        }

        public void jailControl(Client player, int seconds)
        {
            Character character = API.getEntityData(player.handle, "Character");

            int jailOnePlayers = API.getPlayersInRadiusOfPosition(3.7f, jailOne).Count;
            int jailTwoPlayers = API.getPlayersInRadiusOfPosition(3.7f, jailTwo).Count;
            int jailThreePlayers = API.getPlayersInRadiusOfPosition(3.7f, jailThree).Count;
            int smallest = API.getAllPlayers().Count;
            int chosenCell = 0;
            List<int> list = new List<int>(new int[] { jailOnePlayers, jailTwoPlayers, jailThreePlayers });


            //Choose correct cell for player
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < smallest)
                {
                    smallest = list[i];
                    chosenCell = i;
                }
            }

            if (chosenCell == 0)
                API.setEntityPosition(player, jailOne);
            else if (chosenCell == 1)
                API.setEntityPosition(player, jailTwo);
            else
                API.setEntityPosition(player, jailThree);

            API.removeAllPlayerWeapons(player);
            character.isJailed = true;
            API.sendChatMessageToPlayer(player, "You have been jailed for " + seconds / 60 + " minutes.");


            character.jailTimer = new System.Timers.Timer { Interval = seconds };
            character.jailTimer.Elapsed += delegate { setFree(player); };
            character.jailTimer.Start();
        }

        public void beaconDeployer(Client player, Client beaconSender)
        {
            Character character = API.getEntityData(beaconSender.handle, "Character");
            if (character.BeaconSet == false)
            {
                character.BeaconTimer.Stop();
            }

            var senderPosition = API.getEntityPosition(beaconSender);
            API.triggerClientEvent(player, "update_beacon", senderPosition);

        }

        public void resetBeacon(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            character.BeaconSet = false;
            character.BeaconResetTimer.Stop();
        }

        public void setFree(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");
            API.sendChatMessageToPlayer(player, "You have done time and are free to go.");
            character.isJailed = false;
            API.setEntityPosition(player, freeJail);
            lock (jailTimer) jailTimer.Remove(player);
            character.jailTimer.Stop();

        }

        public bool arrestPointCheck(NetHandle entity)
        {
            return arrestShape.containsEntity(entity);
        }
    }
}
