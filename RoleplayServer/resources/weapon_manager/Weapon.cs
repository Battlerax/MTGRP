using System;
using System.Collections.Generic;
using System.Dynamic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using RoleplayServer.resources.group_manager;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.weapon_manager
{
    public class Weapon
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
