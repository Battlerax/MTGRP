using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using MongoDB.Bson;

namespace mtgvrp.core.Items
{
    public class RopeItem : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;
        public bool CanBeStored => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public int AmountOfSlots => 20;

        public string CommandFriendlyName => "rope";

        public string LongName => "Rope";

        public int Object => -1237359228;

        public int Amount { get; set; }
    }
}
