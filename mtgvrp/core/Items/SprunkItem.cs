using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using MongoDB.Bson;

namespace mtgvrp.core.Items
{
    public class SprunkItem : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public int AmountOfSlots => 20;

        public string CommandFriendlyName => "sprunk";

        public string LongName => "Sprunk";

        public int Object => 1020618269;

        public int Amount { get; set; }
    }
}
