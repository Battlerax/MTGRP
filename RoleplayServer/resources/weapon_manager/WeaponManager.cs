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
            API.onPlayerWeaponSwitch += API_onPlayerWeaponSwitch;
            API.onPlayerWeaponAmmoChange += API_onPlayerWeaponAmmoChange;

            DebugManager.DebugMessage("[WeaponM] Weapon Manager initalized!");
        }

        private void API_onPlayerWeaponAmmoChange(Client player, WeaponHash weapon, int ammo)
        {
            if (ammo <= 1)
            {
                API.setPlayerWeaponAmmo(player, weapon, 9999);
            }
        }

        private void API_onPlayerWeaponSwitch(Client player, WeaponHash weapon)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character == null) { return; }

            if (!DoesPlayerHaveWeapon(player, player.currentWeapon))
            {
                player.sendChatMessage("You are not supposed to have this weapon.."); //<--- TEST - REMOVE LATER
                                                                                      //BAN THE PLAYER
                return;
            }

            foreach (Weapon playerWeapon in character.Weapons)
            {
                if (playerWeapon.WeaponHash == weapon)
                {
                    if (playerWeapon.IsGroupWeapon == true && playerWeapon.Group != character.Group && character.Group != null)
                    {
                        RemovePlayerWeapon(player, weapon);
                        player.sendChatMessage("You must be a member of " + playerWeapon.Group.Name + " to use this weapon. It was removed.");
                    }
                }
                
            }

        }

        public static bool DoesPlayerHaveAWeapon(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character.Weapons.Count() > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Client player, WeaponHash weaponhash)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character.Weapons.Count() > 0)
            {
                foreach (Weapon i in character.Weapons)
                {
                    if (i.WeaponHash == weaponhash || (int)weaponhash == -1569615261) { return true; }
                }
            }

            return false;



        }

        public static void AddPlayerWeapon (Client player, WeaponHash weaponhash)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (DoesPlayerHaveWeapon(player, weaponhash)) { RemovePlayerWeapon(player, weaponhash); }
            Weapon weapon = new Weapon(weaponhash);
            weapon.IsPlayerWeapon = true;
        
            character.Weapons.Add(weapon);
            character.Save();
            API.shared.givePlayerWeapon(player, weaponhash, 9999, false, true);

        }

        public static void AddAdminWeapon(Client player,  WeaponHash weaponhash)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (DoesPlayerHaveWeapon(player, weaponhash)) { RemovePlayerWeapon(player, weaponhash); }
            Weapon weapon = new Weapon(weaponhash);
            weapon.IsAdminWeapon = true;

            character.Weapons.Add(weapon);
            character.Save();
            API.shared.givePlayerWeapon(player, weaponhash, 9999, false, true);
        }

        public static void AddGroupWeapon(Client player, WeaponHash weaponhash, Group group)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (DoesPlayerHaveWeapon(player, weaponhash)) { RemovePlayerWeapon(player, weaponhash); }
            Weapon weapon = new Weapon(weaponhash);
            weapon.IsGroupWeapon = true;
            weapon.Group = group;

            character.Weapons.Add(weapon);
            character.Save();
            API.shared.givePlayerWeapon(player, weaponhash, 9999, false, true);
        }

        public static void RemovePlayerWeapon(Client player, WeaponHash weaponhash)
        {
            if (DoesPlayerHaveWeapon(player, weaponhash))
            {
                Character character = API.shared.getEntityData(player.handle, "Character");

                foreach (Weapon weapon in character.Weapons.ToList())
                {
                    if (weapon.WeaponHash == weaponhash)
                    {
                        character.Weapons.Remove(weapon);
                        API.shared.removePlayerWeapon(player, weapon.WeaponHash);
                    }
                }
            }
        }
       

        public static void RemoveAllPlayerWeapons(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            foreach (Weapon weapon in character.Weapons)
            {
                RemovePlayerWeapon(player, weapon.WeaponHash);
            }
        }

        public void TradeWeapon(Client player, Client receiver, WeaponHash weapon)
        {
            RemovePlayerWeapon(player, weapon);
            AddPlayerWeapon(receiver, weapon);

        }



        [Command("giveweapon")]
        public void giveweapon_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");

            if (!DoesPlayerHaveAWeapon(player))
            {
                player.sendChatMessage("You have no weapons on you.");
                return;
            }

            if (player.position.DistanceTo(receiver.position) > 7)
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
            TradeWeapon(player, receiver, currentWeapon);
            player.sendChatMessage("You gave a weapon (" + currentWeapon + ") to " + receiverid.CharacterName + ".");
            receiver.sendChatMessage("You were given a weapon (" + currentWeapon + ") by " + playerid.CharacterName + ".");
            ChatManager.NearbyMessage(player, 10, "~p~" + playerid.CharacterName + " handed a " +  currentWeapon + " to " + receiverid.CharacterName + ".");
        }

        [Command("dropweapon")]
        public void drop_cmd(Client player)
        {
            WeaponHash currentWeapon = API.getPlayerCurrentWeapon(player);

            if ((int) currentWeapon == -1569615261)
            {
                player.sendChatMessage("You must be holding the weapon you want to drop.");
                return;
            }

            RemovePlayerWeapon(player, currentWeapon);
            ChatManager.NearbyMessage(player, 10, "~p~" + player.GetCharacter().CharacterName + " dropped their " + currentWeapon + ".");
        }

        [Command("listweapons")]
        public void listweapons_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");

            if (account.AdminLevel < 3)
            {
                return;
            }

            int i = 0;
            foreach (Weapon w in receiverid.Weapons)
            {
         
                player.sendChatMessage("Weapon " + i + ": " + w.WeaponHash);
                i++;
            }
            player.sendChatMessage("This player owns " + i + " weapons.");

        }
    }
}
