using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

//Change class name
namespace mtgvrp.core.Items
{
    public class FishingRod : IInventoryItem
    {

        [BsonId]
        public ObjectId Id { get; set; }
        public int Amount { get; set; }


        public int AmountOfSlots => 25;


        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool IsBlocking => false;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var itm = new Dictionary<Type, int> { { typeof(Character), 1 } };
                return itm;
            }
        }

        public string CommandFriendlyName => $"fishingrod";

        public string LongName => $"Fishing Rod";

        public int Object => 0;
    }
}