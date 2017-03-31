using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.group_manager.lspd
{
    class Lspd : Script
    {

        public Lspd()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
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
            }
        }

        [Command("recordcrimes", GreedyArg = true)]
        public void recordcrimes_cmd(Client player, string id, Crime crime)
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
            API.setEntityData(receiver, "crimesRecorded", true);
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

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            if (API.getEntityData(receiver, "crimesRecorded") == false)
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

            if (!(bool)API.call("Lspd", "arrestPointCheck", player))
            {
                API.sendNotificationToPlayer(player, "~r~You are too far away from the arrest point.");
                return;
            }

            API.sendNotificationToPlayer(player, "You have arrested ~b~" + receiver.name + "~w~.");
            API.sendNotificationToPlayer(receiver, "You have been arrested by ~b~" + player.name + "~w~.");
            GroupManager.SendGroupMessage(player, player.name + " has placed " + receiver.name + " under arrest.");
            API.setEntityData(receiver, "crimesRecorded", false);
            API.call("Lspd", "jailControl", time);
        }

        [Command("frisk", GreedyArg = true)]
        public void frisk_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
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

            //check items on player (first implement inv system.
        }

        [Command("megaphone", Alias = "mp", GreedyArg = true)]
        public void megaphone_cmd(Client player, string text)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            ChatManager.NearbyMessage(player, 30, PlayerManager.GetName(player) + " [MEGAPHONE]: " + text);
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

    }
}
