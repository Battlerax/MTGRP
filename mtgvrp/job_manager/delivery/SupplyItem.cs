using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.delivery
{
    public class SupplyItem : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 5;

        public bool CanBeDropped => true;
        public bool CanBeGiven => false;
        public bool CanBeStacked => true;
        public bool CanBeStashed => false;
        public bool IsBlocking => false;
        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>()
        {
            {typeof(Character), 10},
            {typeof(BagItem), 10},
            {typeof(Vehicle), 0},
            {typeof(Property), 0},
        };

        public string CommandFriendlyName => "supply";
        public string LongName => "Supply";
        public int Object => 0;
    }
}
