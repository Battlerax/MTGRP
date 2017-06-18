using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.player_manager;
using mtgvrp.group_manager;
using mtgvrp.inventory;


namespace mtgvrp.weapon_manager
{
    class WeaponManager : Script
    {
        public WeaponManager()
        {
            API.onPlayerWeaponSwitch += API_onPlayerWeaponSwitch;
            API.onPlayerWeaponAmmoChange += API_onPlayerWeaponAmmoChange;
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;
            InventoryManager.OnStorageGetItem += InventoryManager_OnStorageGetItem;
            InventoryManager.OnStorageLoseItem += InventoryManager_OnStorageLoseItem;
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;

            DebugManager.DebugMessage("[WeaponM] Weapon Manager initalized!");
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            foreach (Weapon weapon in e.Character.Weapons)
            {
                API.givePlayerWeapon(e.Character.Client, weapon.WeaponHash, 9999, true, true);
            }
        }

        private void InventoryManager_OnStorageLoseItem(IStorage sender, InventoryManager.OnLoseItemEventArgs args)
        {
            if (sender.GetType() == typeof(Character))
            {
                if (args.Item.GetType() == typeof(Weapon))
                {
                    Character chr = (Character)sender;
                    Weapon item = (Weapon) args.Item;
                    foreach(Weapon w in chr.Weapons.ToList())
                    {
                        if (w.WeaponHash == item.WeaponHash)
                        {
                            chr.Weapons.Remove(w);
                        }
                    }
                    API.removePlayerWeapon(chr.Client, item.WeaponHash);
                }
            }
        }

        private void InventoryManager_OnStorageGetItem(IStorage sender, InventoryManager.OnGetItemEventArgs args)
        {
            if (sender.GetType() == typeof(Character))
            {
                if (args.Item.GetType() == typeof(Weapon))
                {
                    Character chr = (Character)sender;
                    Weapon item = (Weapon)args.Item;
                    GivePlayerWeapon(chr.Client, item);
                }
            }
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
            if (weapon == WeaponHash.Unarmed) { return; }

            WeaponHash currentPlayerWeapon = API.getPlayerCurrentWeapon(player);


            if (!DoesPlayerHaveWeapon(player, currentPlayerWeapon) && currentPlayerWeapon != WeaponHash.Unarmed)
            {
                foreach (var p in API.getAllPlayers())
                {
                    Account account = API.shared.getEntityData(p, "Account");

                    API.removePlayerWeapon(player, currentPlayerWeapon);
                    if (account.AdminLevel > 1) { p.sendChatMessage("~r~ [WARNING]: " + player.nametag + " HAS A WEAPON THEY SHOULD NOT HAVE. TAKE ACTION."); }
                }
                return;
            }

            Weapon currentWeapon = GetCurrentWeapon(player);

            if (currentWeapon.Group != character.Group && currentWeapon.Group != Group.None)
            {
                RemoveAllPlayerWeapons(player);
                player.sendChatMessage("You must be a member of " + currentWeapon.Group.Name + " to use this weapon. Your weapons were removed.");
            }

        }

        public static bool DoesPlayerHaveAWeapon(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character.Weapons.Count() > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Client player, WeaponHash weapon)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (character.Weapons.Count() > 0)
            {
                foreach (Weapon i in character.Weapons)
                {
                    if (i.WeaponHash == weapon) { return true; }
                }
            }

            return false;
        }

        public static void CreateWeapon(Client player, WeaponHash weaponhash, WeaponTint weapontint = WeaponTint.Normal, 
            bool isplayerweapon = false, bool isadminweapon = false, bool isgroupweapon = false)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            Weapon weapon = new Weapon(weaponhash, weapontint, isplayerweapon, 
                isadminweapon, isgroupweapon, character.Group);

            GivePlayerWeapon(player, weapon);

        }

        public static void SetWeaponTint(Client player, WeaponHash weaponhash, WeaponTint weapontint)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            foreach(Weapon weapon in character.Weapons.ToList())
            {
                if (weapon.WeaponHash == weaponhash)
                {
                    weapon.WeaponTint = weapontint;
                    API.shared.setPlayerWeaponTint(player, weaponhash, weapontint);
                }
            }
        }

        public static void SetWeaponComponent(Client player, WeaponHash weaponhash, WeaponComponent weaponcomponent)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            foreach (Weapon weapon in character.Weapons.ToList())
            {
                if (weapon.WeaponHash == weaponhash)
                {
                    weapon.WeaponComponent = weaponcomponent;
                    API.shared.givePlayerWeaponComponent(player, weaponhash, weaponcomponent);
                }
            }
        }

        public static void RemoveAllPlayerWeapons(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            InventoryManager.DeleteInventoryItem(character, typeof(Weapon), -1);
            foreach (Weapon weapon in character.Weapons.ToList())
            {
                character.Weapons.Remove(weapon);
            }
            API.shared.removeAllPlayerWeapons(player);
        }

        public static void GivePlayerWeapon(Client player, Weapon weapon)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (DoesPlayerHaveWeapon(player, weapon.WeaponHash)) { return; }

            character.Weapons.Add(weapon);

            API.shared.givePlayerWeapon(player, weapon.WeaponHash, 9999, true, true);
            API.shared.setPlayerWeaponTint(player, weapon.WeaponHash, weapon.WeaponTint);
            //API.shared.givePlayerWeaponComponent(player, weapon.WeaponHash, weapon.WeaponComponent);

            InventoryManager.GiveInventoryItem(character, weapon, 1);
        }

        public static Weapon GetCurrentWeapon(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");

            WeaponHash currentWeapon = API.shared.getPlayerCurrentWeapon(player);

            foreach (Weapon weapon in character.Weapons)
            {
                if (weapon.WeaponHash == currentWeapon) { return weapon; }
            }
            return new Weapon(WeaponHash.Unarmed, WeaponTint.Normal, true, false, false, Group.None);
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

        [Command("removeallweapons")]
        public void removeallweapons_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");

            if (account.AdminLevel < 3)
            {
                return;
            }

            RemoveAllPlayerWeapons(player);

            player.sendChatMessage("Weapons removed.");
            receiver.sendChatMessage("All of your weapons were removed by " + account.AdminName);

        }
    }
}
