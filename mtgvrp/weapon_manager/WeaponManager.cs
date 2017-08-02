using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;


using mtgvrp.core;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.core.Help;


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

            DebugManager.DebugMessage("[WeaponM] Weapon Manager initalized!");
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            foreach (Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(e.Character))
            {
                API.givePlayerWeapon(e.Character.Client, weapon.WeaponHash, 9999, true, true);
                API.setPlayerWeaponTint(e.Character.Client, weapon.WeaponHash, weapon.WeaponTint);
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
                    foreach(Weapon w in InventoryManager.DoesInventoryHaveItem<Weapon>(chr))
                    {
                        if (w.WeaponHash == item.WeaponHash)
                        {
                            InventoryManager.DeleteInventoryItem<Weapon>(chr, 1, x => x == w);
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
                    API.shared.givePlayerWeapon(chr.Client, item.WeaponHash, 9999, true, true);
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
            Character character = player.GetCharacter();
            Account playerAccount = player.GetAccount();

            WeaponHash currentPlayerWeapon = API.getPlayerCurrentWeapon(player);

            if (character == null) { return; }
            if (currentPlayerWeapon == WeaponHash.Unarmed) { return; }

            if (!DoesPlayerHaveWeapon(player, currentPlayerWeapon) && currentPlayerWeapon != WeaponHash.Unarmed && character.JobOne.Type != job_manager.JobManager.JobTypes.Fisher)
            {
                API.removePlayerWeapon(player, currentPlayerWeapon);
                foreach (var p in API.getAllPlayers())
                {
                    if (p == null)
                        continue;

                    Account account = p.GetAccount();
                    if (account.AdminLevel > 1) { p.sendChatMessage("~r~ [WARNING]: " + player.nametag + " HAS A WEAPON THEY SHOULD NOT HAVE. TAKE ACTION."); }
                }
                return;
            }
            

            Weapon currentWeapon = GetCurrentWeapon(player);

            if (character.IsTied || character.IsCuffed)
            {
                API.givePlayerWeapon(player, WeaponHash.Unarmed, 1, true, true);
                return;
            }
            if (currentWeapon.GroupId != character.GroupId && character.GroupId != 0 && currentWeapon.IsGroupWeapon == true)
            {
                RemoveAllPlayerWeapons(player);
                player.sendChatMessage("You must be a member of " + GroupManager.GetGroupById(currentWeapon.GroupId).Name + " to use this weapon. Your weapons were removed.");
                return;
            }
            if (playerAccount.VipLevel == 0)
            {
                API.shared.setPlayerWeaponTint(player, currentWeapon.WeaponHash, WeaponTint.Normal);
                return;
            }
            else { API.shared.setPlayerWeaponTint(player, currentWeapon.WeaponHash, currentWeapon.WeaponTint); }

        }

        public static bool DoesPlayerHaveAWeapon(Client player)
        {
            Character character = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<Weapon>(character).Length > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Client player, WeaponHash weapon)
        {
            Character character = player.GetCharacter();

            foreach (Weapon i in InventoryManager.DoesInventoryHaveItem<Weapon>(character))
            {
                if (i.WeaponHash == weapon)
                {
                    return true;
                }
            }

            return false;
        }

        public static void CreateWeapon(Client player, WeaponHash weaponhash, WeaponTint weapontint = WeaponTint.Normal, 
            bool isplayerweapon = false, bool isadminweapon = false, bool isgroupweapon = false)
        {
            Character character = player.GetCharacter();

            Weapon weapon = new Weapon(weaponhash, weapontint, isplayerweapon, 
                isadminweapon, isgroupweapon, character.Group);

            GivePlayerWeapon(player, weapon);

        }

        public static void SetWeaponTint(Client player, WeaponHash weaponhash, WeaponTint weapontint)
        {
            Character character = player.GetCharacter();

            foreach(Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(character))
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
            Character character = player.GetCharacter();

            foreach (Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(character))
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
            Character character = player.GetCharacter();

            InventoryManager.DeleteInventoryItem(character, typeof(Weapon), -1);
            API.shared.removeAllPlayerWeapons(player);
        }

        public static void GivePlayerWeapon(Client player, Weapon weapon)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (DoesPlayerHaveWeapon(player, weapon.WeaponHash)) { return; }

            if (account.VipLevel < 1) { API.shared.setPlayerWeaponTint(player, weapon.WeaponHash, WeaponTint.Normal); }
            else { API.shared.setPlayerWeaponTint(player, weapon.WeaponHash, weapon.WeaponTint); }
            //API.shared.givePlayerWeaponComponent(player, weapon.WeaponHash, weapon.WeaponComponent);

            InventoryManager.GiveInventoryItem(character, weapon, 1);
        }

        public static Weapon GetCurrentWeapon(Client player)
        {
            Character character = player.GetCharacter();

            WeaponHash currentWeapon = API.shared.getPlayerCurrentWeapon(player);

            Weapon[] weapon =
                InventoryManager.DoesInventoryHaveItem<Weapon>(character, x => x.WeaponHash == currentWeapon);

            if (weapon.Length > 0)
                return weapon[0];

            return new Weapon(WeaponHash.Unarmed, WeaponTint.Normal, true, false, false, Group.None);
        }

        [Command("listweapons"), Help(HelpManager.CommandGroups.AdminLevel3, "Used to see what weapons a player has on them.", new[] { "ID of target player." })]
        public void listweapons_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receiverid = receiver.GetCharacter();

            if (account.AdminLevel < 3)
            {
                return;
            }

            int i = 0;
            foreach (Weapon w in InventoryManager.DoesInventoryHaveItem<Weapon>(receiverid))
            {
         
                player.sendChatMessage("Weapon " + i + ": " + w.WeaponHash);
                i++;
            }
            player.sendChatMessage("This player owns " + i + " weapons.");

        }

        [Command("removeallweapons"), Help(HelpManager.CommandGroups.AdminLevel3, "Used to sremove all weapons from a player", new[] { "ID of target player." })]
        public void removeallweapons_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();
            Character receiverid = receiver.GetCharacter();

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
