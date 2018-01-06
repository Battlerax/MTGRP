using System;
using System.Collections.Generic;
using mtgvrp.player_manager;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.inventory.bags
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
        public bool CanBeStored => false;

        public Dictionary<Type, int> MaxAmount
        {
            get
            {
                var itm = new Dictionary<Type, int> { { typeof(Character), 1 } };
                return itm;
            }
        }

        public string CommandFriendlyName => $"bag_{BagName}";

        public string LongName => $"Bag, {BagName}";

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

        public int MaxInvStorage => 200;

        public int BagType { get; set; }
        public int BagDesign { get; set; }

        public string BagName { get; set; }

        public void Save()
        {
            //Ignored
        }
    }
}
