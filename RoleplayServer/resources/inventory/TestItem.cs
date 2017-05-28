using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.inventory
{
    class TestItem : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 25;

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => true;
        public bool CanBeStashed => true;
        public bool IsBlocking => false;
        public int MaxAmount => -1;

        public string CommandFriendlyName => "TestItem";
        public string LongName => "Test Item";
        public int Object => 0;
    }
}
