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

        public string CommandFriendlyName => $"Bag{BagType}{BagDesign}";

        public string LongName => $"Bag, Type {BagType} Design {BagDesign}";

        public int Object
        {
            get
            {
                //If its the heist bag
                if (BagType == 40 || BagType == 41 || BagType == 44 || BagType == 45)
                    return -711724000;

                return 1269440357;
            }
        }

        //-------------------------

        private List<IInventoryItem> _inventory;
        public List<IInventoryItem> Inventory
        {
            get { return _inventory ?? (_inventory = new List<IInventoryItem>()); }
            set { _inventory = value; }
        }

        //TODO: to be changed, not sure how much it should be.
        public int MaxInvStorage
        {
            get
            {
                //If its the heist bag, its 1000 big and if its any other, 500.
                if (BagType == 40 || BagType == 41 || BagType == 44 || BagType == 45)
                    return 1000;

                return 500;
            }
        }

        public int BagType { get; set; }
        public int BagDesign { get; set; }

    }
}
