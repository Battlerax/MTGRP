using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.weapon_manager
{
    class WeaponManager : Script
    {
        public WeaponManager()
        {

        }

        public static bool DoesPlayerHaveAWeapon(Character player)
        {
            if (player.Weapons.Count() > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Character player, Weapon weapon)
        {
            if (player.Weapons.Contains(weapon)) { return true; }
            else { return false; }
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
