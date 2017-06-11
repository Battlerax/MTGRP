using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using RoleplayServer.inventory;

namespace RoleplayServer.core.Items
{
    class RagsItem : IInventoryItem
    {
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => true;
        public bool CanBeStacked => true;

        public bool IsBlocking => false;

        public int MaxAmount => -1;

        public int AmountOfSlots => 5;

        public string CommandFriendlyName => "rag";

        public string LongName => "Rag";

        public int Object => 1108364521;

        public int Amount { get; set; }
    }
}
