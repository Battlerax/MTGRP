using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Shared;


using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;


public class Weapon : IInventoryItem
    {

        public WeaponHash WeaponHash { get; set; }
        public WeaponTint WeaponTint { get; set; }
        public WeaponComponent WeaponComponent { get; set; }

        public int Ammo { get; set; }

        public int GroupId { get; set; }

        public bool IsPlayerWeapon { get; set; }
        public bool IsGroupWeapon { get; set; }
        public bool IsAdminWeapon { get; set; }

        #region inv-related
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => false;
        public bool IsBlocking => false;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>()
        {
            {typeof(Character), 1},
        };
        public int AmountOfSlots => 0;

        public string CommandFriendlyName => WeaponHash.ToString();
        public string LongName => WeaponHash.ToString();
        public int Object => 289396019;

        public int Amount { get; set; }

        #endregion

        public Weapon()
        {

        }

        public Weapon(WeaponHash weaponhash, WeaponTint weapontint, bool isplayerweapon, bool isadminweapon, bool isgroupweapon, Group group)
        {
            WeaponHash = weaponhash;
            WeaponTint = weapontint;
            IsPlayerWeapon = isplayerweapon;
            IsAdminWeapon = isadminweapon;
            IsGroupWeapon = isgroupweapon;
            GroupId = group.Id;
        }

    }
