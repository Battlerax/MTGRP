using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.group_manager;

namespace RoleplayServer.resources.weapon_manager
{
    class WeaponManager : Script
    {
        public WeaponManager()
        {

        }

        public static bool DoesPlayerHaveAWeapon(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character.Weapons.Count() > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Client player, string weaponname)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            foreach (Weapon i in character.Weapons)
            {
                if (i.WeaponName == weaponname) { return true; }
            }

            return false;
        }

        public void AddPlayerWeapon (Client player, string weaponname, int ammo)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            WeaponHash weaponhash = API.weaponNameToModel(weaponname);

            Weapon weapon = new Weapon(weaponhash, weaponcomponent, weaponname, ammo);
            character.Weapons.Add(weapon);
        }

        public void AddPlayerAdminWeapon(Character player,  string weaponname, int ammo)
        {
            Weapon weapon = new Weapon(weaponhash, weaponcomponent, weaponname, ammo, true);
            player.Weapons.Add(weapon);
        }

        public void AddPlayerGroupWeapon(Character player, string weaponname, int ammo, string groupname)
        {
            Weapon weapon = new Weapon(weaponhash, weaponcomponent, weaponname, ammo);
            weapon.Group = group;
            player.Weapons.Add(weapon);
        }

        public void RemovePlayerWeapon(Character player, Weapon weapon)
        {
            if (DoesPlayerHaveWeapon(player, weapon))
            {
                player.Weapons.Remove(weapon);
            }
        }

        public void RemoveAllPlayerWeapons(Character player)
        {
            foreach (Weapon weapon in player.Weapons)
            {
                player.Weapons.Remove(weapon);
            }
        }


        [Command("giveweapon")]
        public void giveweapon_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");

            if (API.getPlayerWeapons(player).Length == 0)
            {
                player.sendChatMessage("You have no weapons on you.");
                return;
            }

            if (GetDistanceBetweenPlayers(player, receiver) > 7)
            {
                API.sendChatMessageToPlayer(player, "That player is too far from you.");
                return;
            }

            if ((int)API.getPlayerCurrentWeapon(player) == -1569615261)
            {
                player.sendChatMessage("You must be holding the weapon you want to hand over.");
                return;
            }

            WeaponHash currentWeapon = API.getPlayerCurrentWeapon(player);
            API.removePlayerWeapon(player, currentWeapon);
            receiverid.Weapons.Remove(currentWeapon);
            playerid.Weapons.Add(currentWeapon);
            API.givePlayerWeapon(receiver, currentWeapon, 500, true, true);
            NearbyMessage(player, 10, "~p~" + playerid.CharacterName + " handed a " + currentWeapon + " to " + receiverid.CharacterName + ".");
            player.sendChatMessage("You gave a weapon (" + currentWeapon + ") to " + receiverid.CharacterName + ".");
            receiver.sendChatMessage("You were given a weapon (" + currentWeapon + ") by " + playerid.CharacterName + ".");

        }

        [Command("dropweapon")]
        public void drop_cmd(Client player, string item)
        {
          
        }
    }
}
