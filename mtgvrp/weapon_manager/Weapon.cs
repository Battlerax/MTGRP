using System;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using RoleplayServer.group_manager;
using RoleplayServer.inventory;


namespace RoleplayServer.weapon_manager
{
    public class Weapon : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public WeaponHash WeaponHash { get; set; }
        public WeaponTint WeaponTint { get; set; }
        public WeaponComponent WeaponComponent { get; set; }
      

        public int Ammo { get; set; }

        public Group Group { get; set; }

        public bool IsPlayerWeapon { get; set; }
        public bool IsGroupWeapon { get; set; }
        public bool IsAdminWeapon { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => false;
        public bool IsBlocking => false;

        public int MaxAmount => -1;
        public int AmountOfSlots => 0;

        public string CommandFriendlyName => WeaponHash.ToString();
        public string LongName => WeaponHash.ToString();
        public int Object => 289396019;


        public int Amount { get; set; }

        public Weapon(WeaponHash weaponhash, WeaponTint weapontint, bool isplayerweapon, bool isadminweapon, bool isgroupweapon, Group group)
        {
            WeaponHash = weaponhash;
            WeaponTint = weapontint;
            IsPlayerWeapon = isplayerweapon;
            IsAdminWeapon = isadminweapon;
            IsGroupWeapon = isgroupweapon;
            Group = group;
        }
    }
}
