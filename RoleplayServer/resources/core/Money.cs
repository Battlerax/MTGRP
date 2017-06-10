using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.core
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

        public int MaxAmount => -1;
        public int AmountOfSlots => 0;

        public string CommandFriendlyName => "money";
        public string LongName => "Money";
        public int Object => 289396019;


        public int Amount { get; set; }

        public static int GetCharacterMoney(Character c)
        {
            return InventoryManager.DoesInventoryHaveItem<Money>(c)?.FirstOrDefault()?.Amount ?? 0;
        }
}
}
