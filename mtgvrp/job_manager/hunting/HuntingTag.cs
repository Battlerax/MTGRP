using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.hunting
{
    public class HuntingTag : IInventoryItem
    {
       
        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 5;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>()
        {
            {typeof(Character), 2}
        };

        public string CommandFriendlyName => Type + "_Tag";
        public string LongName => Type + " Tag (" + ValidDate.ToShortDateString() + ")";
        public int Object => 0;

        public HuntingManager.AnimalTypes Type { get; set; } = HuntingManager.AnimalTypes.Deer;
        public DateTime ValidDate { get; set; } = DateTime.Today.Date;
    }
}
