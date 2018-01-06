using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.job_manager.scuba
{
    public class ScubaItem : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 100;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => false;
        public bool CanBeStashed => false;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public string CommandFriendlyName => "scuba_kit";
        public string LongName => $"Scuba Diving Kit ({Math.Round((OxygenRemaining / MaxOxygen) * 100f)}%)";
        public int Object => 0;

        public float OxygenRemaining { get; set; } = MaxOxygen;

        public const float MaxOxygen = 3600f;
    }
}
