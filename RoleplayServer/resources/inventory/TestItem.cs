using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer.resources.inventory
{
    [BsonDiscriminator("TestItem")]
    class TestItem : IInventoryItem
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public int AmountOfSlots => 25;
        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;
        public bool IsBlocking => false;

        public string CommandFriendlyName => "TestItem";
        public string LongName => "TestItem";

        public int Amount { get; set; }
    }
}
