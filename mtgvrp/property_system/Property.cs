using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.door_manager;
using mtgvrp.inventory;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.property_system
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
        public int EntranceDimension { get; set; }

        public bool IsInteractable { get; set; }
        public Vector3 InteractionPos { get; set; }
        public Vector3 InteractionRot { get; set; }
        public int InteractionDimension { get; set; }

        public bool IsLocked { get; set; }

        public int PropertyPrice { get; set; }

        public Dictionary<string, int> ItemPrices { get; set; }
        public string[] RestaurantItems;

        [BsonIgnore]
        public MarkerZone EntranceMarker { get; set; }

        [BsonIgnore]
        public MarkerZone InteractionMarker { get; set; }

        [BsonIgnore]
        public MarkerZone ExitMarker { get; set; }

        public int AdvertisingPrice { get; set; }

        public Property(PropertyManager.PropertyTypes type, Vector3 entrancePos, Vector3 entranceRot,
            string entranceString)
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
                case PropertyManager.PropertyTypes.GasStation:
                    return 361;
                case PropertyManager.PropertyTypes.Ammunation:
                    return 110;
                case PropertyManager.PropertyTypes.LSNN:
                    return 184;
                case PropertyManager.PropertyTypes.Advertising:
                    return 133;
                default:
                    return -1;
            }
        }

        public void CreateProperty()
        {
            EntranceString = OwnerId == 0
                ? $"Unowned. /buyproperty to buy it.\nCosts ~g~${PropertyPrice}~w~"
                : PropertyName;

            EntranceMarker = new MarkerZone(EntrancePos, EntranceRot, EntranceDimension)
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
                if (Type != PropertyManager.PropertyTypes.GasStation)
                {
                    InteractionMarker = new MarkerZone(InteractionPos, InteractionRot, InteractionDimension)
                    {
                        LabelText = PropertyManager.GetInteractText(Type)
                    };
                }
                else
                {
                    InteractionMarker = new MarkerZone(InteractionPos, InteractionRot, InteractionDimension, 10)
                    {
                        LabelText = PropertyManager.GetInteractText(Type)
                    };
                }
                InteractionMarker.Create();
                InteractionMarker.ColZone.setData("property_interaction", Id);
            }

            if (IsTeleportable)
            {
                ExitMarker = new MarkerZone(TargetPos, TargetRot, TargetDimension) {LabelText = "/exit"};
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
