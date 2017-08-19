using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.hunting
{
    public class AmmoItem : IInventoryItem
    {

        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 1;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>()
        {
            {typeof(Character), 5}
        };

        public string CommandFriendlyName => "5.56_Bullet";
        public string LongName => "5.56 Bullet";
        public int Object => 0;
        
    }
}