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
