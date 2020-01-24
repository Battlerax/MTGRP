using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

//Change class name
namespace mtgvrp.core.Items
{
    public class SprayPaint : IInventoryItem
    {

        [BsonId]
        public ObjectId Id { get; set; }
        public int Amount { get; set; }


        public int AmountOfSlots => 20;


        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var itm = new Dictionary<Type, int> { { typeof(Character), 5 } };
                return itm;
            }
        }

        public string CommandFriendlyName => $"spraypaint";

        public string LongName => $"Spray Paint";

        public int Object => 0;
    }
}
