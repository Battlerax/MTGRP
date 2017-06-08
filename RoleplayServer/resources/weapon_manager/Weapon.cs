using System;
using System.Collections.Generic;
using System.Dynamic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.weapon_manager
{
    public interface Weapon
    {
        [BsonId]
        ObjectId Id { get; set; }

        WeaponHash WeaponHash { get; set; }
        string WeaponName { get; set; }

        int Ammo { get; set; }
        WeaponComponent WeaponAttachment { get; set; }

        bool IsAdminWeapon { get; set; }
    }
}
