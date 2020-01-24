using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.job_manager.gunrunner
{
    public class WeaponCase : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 0;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool IsBlocking => false;
        public bool CanBeStored => false;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var itm = new Dictionary<Type, int> {{typeof(Character), 5}};
                return itm;
            }
        }

        public string CommandFriendlyName => "weaponcase_" + WeaponHash;
        public string LongName => "Weapon Case (" + WeaponHash + ")";
        public int Object => 0;


        public WeaponHash WeaponHash { get; set; }
        public Entity WeaponObject { get; set; }
        public int OwnerId { get; set; }

        public WeaponCase()
        {

        }

        public WeaponCase(WeaponHash weapon, Character owner)
        {
            WeaponHash = weapon;
            OwnerId = owner.Id;
        }

        public void CreateCase (Vector3 position, Vector3 rotation)
        {
            WeaponObject = API.Shared.CreateObject(API.Shared.GetHashKey("prop_gun_case_01"), position, rotation);
        }

        public void RemoveCase()
        {
            API.Shared.DeleteEntity(WeaponObject);
        }

    }
}
