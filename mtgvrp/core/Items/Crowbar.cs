using System;
using System.Collections.Generic;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;

namespace mtgvrp.drugs_manager
{
    public class Crowbar : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => false;
        public bool IsBlocking => true;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var maxVal = new Dictionary<Type, int> { { typeof(Character), 1 } };
                return maxVal;
            }
        }

        public bool CanBeStored => true;
        public int AmountOfSlots => 25;
        public string CommandFriendlyName => "crowbar";
        public string LongName => "Crowbar";
        public int Object => -222444887;
        public int Amount { get; set; }
    }
}