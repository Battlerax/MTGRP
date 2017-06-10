using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.door_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.vehicle_manager;

namespace RoleplayServer.resources.property_system
{
    public class Property : IStorage
    {
        public int Id { get; set; }

        public int OwnerId { get; set; }
        public string PropertyName { get; set; }
        public PropertyManager.PropertyTypes Type { get; set; }
        public int Supplies { get; set; }
        public bool IsTeleportable { get; set; }
        public Vector3 TargetPos { get; set; }
        public Vector3 TargetRot { get; set; }

        [BsonIgnore]
        public int TargetDimension => Id + 1000;

        public int MainDoorId { get; set; }

        //Inventory System
        public List<IInventoryItem> Inventory { get; set; }
        public int MaxInvStorage => 1000; //TODO: to be changed.

        //EntranceInfo
        public string EntranceString { get; set; }
        public Vector3 EntrancePos { get; set; }
        public Vector3 EntranceRot { get; set; }

        public bool IsInteractable { get; set; }
        public Vector3 InteractionPos { get; set; }
        public Vector3 InteractionRot { get; set; }
        public int InteractionDimension { get; set; }

        public bool IsLocked { get; set; }

        public int PropertyPrice { get; set; }

        public Dictionary<string,int> ItemPrices { get; set; }
        public string[] RestaurantItems;

        [BsonIgnore]
        public MarkerZone EntranceMarker { get; set; }

        [BsonIgnore]
        public MarkerZone InteractionMarker { get; set; }

        [BsonIgnore]
        public MarkerZone ExitMarker { get; set; }

        public Property(PropertyManager.PropertyTypes type, Vector3 entrancePos, Vector3 entranceRot, string entranceString)
        {
            Type = type;
            EntranceString = entranceString;
            EntrancePos = entrancePos;
            EntranceRot = entranceRot;
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("properties");
            DatabaseManager.PropertyTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = MongoDB.Driver.Builders<Property>.Filter.Eq("_id", Id);
            DatabaseManager.PropertyTable.ReplaceOneAsync(filter, this);
        }

        public void Delete()
        {
            DestroyMarkers();
            var filter = MongoDB.Driver.Builders<Property>.Filter.Eq("_id", Id);
            DatabaseManager.PropertyTable.DeleteOne(filter);
            PropertyManager.Properties.Remove(this);
        }

        public void DestroyMarkers()
        {
            EntranceMarker?.Destroy();
            InteractionMarker?.Destroy();
            ExitMarker?.Destroy();
        }

        private int GetBlip()
        {
            switch (Type)
            {
                    case PropertyManager.PropertyTypes.TwentyFourSeven:
                        return 52;
                    case PropertyManager.PropertyTypes.Hardware:
                        return 446;
                    case PropertyManager.PropertyTypes.Bank:
                        return 207;
                    case PropertyManager.PropertyTypes.Clothing:
                        return 73;
                    case PropertyManager.PropertyTypes.Restaurant:
                        return 93;
                default:
                    return -1;
            }
        }

        public void CreateProperty()
        {
            EntranceString = OwnerId == 0 ? $"Unowned. /buyproperty to buy it.\nCosts ~g~${PropertyPrice}~w~" : PropertyName;

            EntranceMarker = new MarkerZone(EntrancePos, EntranceRot)
            {
                LabelText = EntranceString + "\n" + Type + "\n" + "ID: " + Id,
                BlipSprite = GetBlip()
            };
            EntranceMarker.Create();
            EntranceMarker.ColZone.setData("property_entrance", Id);
            if (API.shared.doesEntityExist(EntranceMarker.Blip))
            {
                API.shared.setBlipShortRange(EntranceMarker.Blip, true);
                API.shared.setBlipName(EntranceMarker.Blip, PropertyName);
            }

            if (IsInteractable)
            {
                InteractionMarker = new MarkerZone(InteractionPos, InteractionRot, InteractionDimension)
                {
                    LabelText = PropertyManager.GetInteractText(Type)
                };
                InteractionMarker.Create();
                InteractionMarker.ColZone.setData("property_interaction", Id);
            }

            if (IsTeleportable)
            {
                ExitMarker = new MarkerZone(TargetPos, TargetRot) { LabelText = "/exit" };
                ExitMarker.Create();
                ExitMarker.ColZone.setData("property_exit", Id);
            }
        }

        public void UpdateMarkers()
        {
            DestroyMarkers();
            CreateProperty();
        }

        public void UpdateLockStatus()
        {
            if (!IsTeleportable)
            {
                var door = Door.Doors.SingleOrDefault(x => x.Id == MainDoorId);
                if (door != null)
                {
                    door.Locked = IsLocked;
                    door.RefreshDoor();
                }
            }
        }
    }
}
