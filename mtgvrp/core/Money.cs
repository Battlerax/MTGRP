using System;
using System.Collections.Generic;
using System.Linq;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.core
{
    public class Money : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;
        public bool IsBlocking => false;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();
        public int AmountOfSlots => 0;

        public string CommandFriendlyName => "money";
        public string LongName => "Money";
        public int Object => -994740387;


        public int Amount { get; set; }

        public static int GetCharacterMoney(Character c)
        {
            return InventoryManager.DoesInventoryHaveItem<Money>(c)?.FirstOrDefault()?.Amount ?? 0;
        }
}
}
