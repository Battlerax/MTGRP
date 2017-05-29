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
        public bool IsEnterable { get; set; }
        public Vector3 TargetPos { get; set; }
        public Vector3 TargetRot { get; set; }

        public List<IInventoryItem> Inventory { get; set; }

        public int MaxInvStorage => 1000; //TODO: to be changed.

        //EnteranceInfo
        public string EnteranceString { get; set; }
        public Vector3 EnterancePos { get; set; }
        public Vector3 EnteranceRot { get; set; }

        public string InteractionString { get; set; }
        public Vector3 InteractionPos { get; set; }
        public Vector3 InteractionRot { get; set; }
        public Vector3 InteractionDimension { get; set; }


        [BsonIgnore]
        public MarkerZone EnteranceMarker { get; set; }

        [BsonIgnore]
        public MarkerZone InteractionMarker { get; set; }

        public Property(PropertyManager.PropertyTypes type, Vector3 enterancePos, Vector3 enteranceRot, string enteranceString)
        {
            Type = type;
            EnteranceString = enteranceString;
            EnterancePos = enterancePos;
            EnteranceRot = enteranceRot;
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
            var filter = MongoDB.Driver.Builders<Property>.Filter.Eq("_id", Id);
            DatabaseManager.PropertyTable.DeleteOne(filter);
        }

        public void CreateProperty()
        {
            EnteranceMarker = new MarkerZone(EnterancePos, EnteranceRot) {LabelText = EnteranceString};
            EnteranceMarker.Create();
            EnteranceMarker.ColZone.setData("property_enterance", Id);

            InteractionMarker = new MarkerZone(InteractionPos, InteractionRot) { LabelText = InteractionString };
            InteractionMarker.Create();
            InteractionMarker.ColZone.setData("property_interaction", Id);
        }
    }
}
