using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.hunting
{
    public class AnimalItem : IInventoryItem
    {

        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 50;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool IsBlocking => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>()
        {
            {typeof(Character), 2}
        };

        public string CommandFriendlyName => Type + "";
        public string LongName => Type + " Carcass";
        public int Object => 0;

        public HuntingManager.AnimalTypes Type { get; set; } = HuntingManager.AnimalTypes.Deer;
        public int Weight;
    }
}
