using System.Linq;
using GTANetworkAPI;
using mtgvrp.core;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.core.Help;
using mtgvrp.job_manager.gunrunner;

namespace mtgvrp.weapon_manager
{
    class WeaponManager : Script
    {
        public WeaponManager()
        {
            //Event.OnPlayerWeaponAmmoChange += API_onPlayerWeaponAmmoChange;
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;
            InventoryManager.OnStorageGetItem += InventoryManager_OnStorageGetItem;
            InventoryManager.OnStorageLoseItem += InventoryManager_OnStorageLoseItem;

            DebugManager.DebugMessage("[WeaponM] Weapon Manager initalized!");
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            foreach (Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(e.Character))
            {
                API.GivePlayerWeapon(e.Character.Player, weapon.WeaponHash, 9999);
                API.SetPlayerWeaponTint(e.Character.Player, weapon.WeaponHash, weapon.WeaponTint);
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
                    API.RemovePlayerWeapon(chr.Player, item.WeaponHash);
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
                    API.Shared.GivePlayerWeapon(chr.Player, item.WeaponHash, 9999);
                }
            }
        }

        // TODO: re-assess since this event died. (it can be done client-side if necessary)
        private void API_onPlayerWeaponAmmoChange(Player player, WeaponHash weapon, int ammo)
        {
            if (ammo <= 1)
            {
                API.SetPlayerWeaponAmmo(player, weapon, 9999);
            }
        }

        [ServerEvent(Event.PlayerWeaponSwitch)]
        public void OnPlayerWeaponSwitch(Player player, WeaponHash weapon, WeaponHash newhash)
        {
            Character character = player.GetCharacter();
            Account playerAccount = player.GetAccount();

            WeaponHash currentPlayerWeapon = NAPI.Player.GetPlayerCurrentWeapon(player);

            if (character == null) { return; }
            if (currentPlayerWeapon == WeaponHash.Unarmed) { return; }

            if (!DoesPlayerHaveWeapon(player, currentPlayerWeapon) && currentPlayerWeapon != WeaponHash.Unarmed && character.JobOne.Type != job_manager.JobManager.JobTypes.Fisher)
            {
                API.RemovePlayerWeapon(player, currentPlayerWeapon);
                foreach (var p in API.GetAllPlayers())
                {
                    if (p == null)
                        continue;

                    Account account = p.GetAccount();
                    if (account.AdminLevel > 1) { p.SendChatMessage("~r~ [WARNING]: " + player.Nametag + " HAS A WEAPON THEY SHOULD NOT HAVE. TAKE ACTION."); }
                }
                return;
            }
            

            Weapon currentWeapon = GetCurrentWeapon(player);

            if (character.IsTied || character.IsCuffed)
            {
                API.GivePlayerWeapon(player, WeaponHash.Unarmed, 1);
                return;
            }
            if (currentWeapon.GroupId != character.GroupId && character.GroupId != 0 && currentWeapon.IsGroupWeapon == true)
            {
                RemoveAllPlayerWeapons(player);
                player.SendChatMessage("You must be a member of " + GroupManager.GetGroupById(currentWeapon.GroupId).Name + " to use this weapon. Your weapons were removed.");
                return;
            }
            if (playerAccount.VipLevel == 0)
            {
                API.Shared.SetPlayerWeaponTint(player, currentWeapon.WeaponHash, WeaponTint.Normal);
                return;
            }
            else { API.Shared.SetPlayerWeaponTint(player, currentWeapon.WeaponHash, currentWeapon.WeaponTint); }

        }

        public static bool DoesPlayerHaveAWeapon(Player player)
        {
            Character character = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<Weapon>(character).Length > 0) { return true; }
            else { return false; }
        }


        public static bool DoesPlayerHaveWeapon(Player player, WeaponHash weapon)
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

        public static void CreateWeapon(Player player, WeaponHash weaponhash, WeaponTint weapontint = WeaponTint.Normal, 
            bool isplayerweapon = false, bool isadminweapon = false, bool isgroupweapon = false)
        {
            Character character = player.GetCharacter();

            Weapon weapon = new Weapon(weaponhash, weapontint, isplayerweapon, 
                isadminweapon, isgroupweapon, character.Group);

            GivePlayerWeapon(player, weapon);

        }

        public static void SetWeaponTint(Player player, WeaponHash weaponhash, WeaponTint weapontint)
        {
            Character character = player.GetCharacter();

            foreach(Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(character))
            {
                if (weapon.WeaponHash == weaponhash)
                {
                    weapon.WeaponTint = weapontint;
                    API.Shared.SetPlayerWeaponTint(player, weaponhash, weapontint);
                }
            }
        }

        public static void SetWeaponComponent(Player player, WeaponHash weaponhash, WeaponComponent weaponcomponent)
        {
            Character character = player.GetCharacter();

            foreach (Weapon weapon in InventoryManager.DoesInventoryHaveItem<Weapon>(character))
            {
                if (weapon.WeaponHash == weaponhash)
                {
                    weapon.WeaponComponent = weaponcomponent;
                    API.Shared.SetPlayerWeaponComponent(player, weaponhash, weaponcomponent);
                }
            }
        }

        public static void RemoveAllPlayerWeapons(Player player)
        {
            Character character = player.GetCharacter();

            InventoryManager.DeleteInventoryItem(character, typeof(Weapon), -1);
            API.Shared.RemoveAllPlayerWeapons(player);
        }

        public static void GivePlayerWeapon(Player player, Weapon weapon)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (DoesPlayerHaveWeapon(player, weapon.WeaponHash)) { return; }

            if (account.VipLevel < 1) { API.Shared.SetPlayerWeaponTint(player, weapon.WeaponHash, WeaponTint.Normal); }
            else { API.Shared.SetPlayerWeaponTint(player, weapon.WeaponHash, weapon.WeaponTint); }
            //API.Shared.GivePlayerWeaponComponent(player, weapon.WeaponHash, weapon.WeaponComponent);

            InventoryManager.GiveInventoryItem(character, weapon, 1);
        }

        public static Weapon GetCurrentWeapon(Player player)
        {
            Character character = player.GetCharacter();

            WeaponHash currentWeapon = API.Shared.GetPlayerCurrentWeapon(player);

            Weapon[] weapon =
                InventoryManager.DoesInventoryHaveItem<Weapon>(character, x => x.WeaponHash == currentWeapon);

            if (weapon.Length > 0)
                return weapon[0];

            return new Weapon(WeaponHash.Unarmed, WeaponTint.Normal, true, false, false, Group.None);
        }

        [Command("listweapons"), Help(HelpManager.CommandGroups.AdminLevel3, "Used to see what weapons a player has on them.", new[] { "ID of target player." })]
        public void listweapons_cmd(Player player, string id)
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
         
                player.SendChatMessage("Weapon " + i + ": " + w.WeaponHash);
                i++;
            }
            player.SendChatMessage("This player owns " + i + " weapons.");

        }

        [Command("removeallweapons"), Help(HelpManager.CommandGroups.AdminLevel3, "Used to sremove all weapons from a player", new[] { "ID of target player." })]
        public void removeallweapons_cmd(Player player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = player.GetAccount();

            if (account.AdminLevel < 3)
            {
                return;
            }

            RemoveAllPlayerWeapons(receiver);

            player.SendChatMessage("Weapons removed.");
            receiver.SendChatMessage("All of your weapons were removed by " + account.AdminName);

        }
    }
}
