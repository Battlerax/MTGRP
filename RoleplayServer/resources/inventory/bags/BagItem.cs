using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory.bags
{
    [BsonDiscriminator("BagItem")]
    class BagItem : IStorage, IInventoryItem
    {
        public BagItem()
        {
            Inventory = new List<IInventoryItem>();
        }

        [BsonId]
        public ObjectId Id { get; set; }

        public int Amount { get; set; }

        public int AmountOfSlots => 0; //Will return 0 cause it shouldn't be taking any slots but still have items inside.

        public bool CanBeDropped => true;
        public bool CanBeGiven => true;
        public bool CanBeStacked => false;
        public bool CanBeStashed => true;
        public bool IsBlocking => false;
        public int MaxAmount => 1;

        public string CommandFriendlyName
        {
            get
            {
                //TO
                throw new NotImplementedException();
            }
        }

        public string LongName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Object
        {
            get
            {
                //TODO: return 
                throw new NotImplementedException();
            }
        }

        //-------------------------

        private List<IInventoryItem> _inventory;
        public List<IInventoryItem> Inventory
        {
            get { return _inventory ?? (_inventory = new List<IInventoryItem>()); }
            set { _inventory = value; }
        }

        public int MaxInvStorage => 500; //To be calced dynamically depending on bag model.

    }
}
