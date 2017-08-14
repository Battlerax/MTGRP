using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;

namespace mtgvrp.drugs_manager
{
    public class Speed : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;
        public bool IsBlocking => false;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var maxVal = new Dictionary<Type, int> { { typeof(Character), 100 } };
                return maxVal;
            }
        }

        public bool CanBeStored => true;
        public int AmountOfSlots => 5;
        public string CommandFriendlyName => "speed";
        public string LongName => "Speed";
        public int Object => -1384898827;
        public int Amount { get; set; }
    }
}
