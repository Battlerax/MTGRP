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
        public string WeaponName { get; set; }

        public int Ammo { get; set; }
        public WeaponComponent WeaponAttachment { get; set; }

        public Group Group { get; set; }

        public bool IsAdminWeapon { get; set; }

        public Weapon(WeaponHash weaponhash, WeaponComponent weaponattachment, string weaponname, int ammo, bool isadminweapon = false)
        {
            WeaponHash = weaponhash;
            WeaponAttachment = weaponattachment;
            WeaponName = WeaponName;
            Ammo = ammo;
            IsAdminWeapon = isadminweapon;
        }
    }
}
