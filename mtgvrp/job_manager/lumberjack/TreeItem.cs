using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using GTANetworkShared;

namespace mtgvrp.job_manager.lumberjack
{
    public class TreeItem : IInventoryItem
    {
        public static List<TreeItem> Trees = new List<TreeItem>();

        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => false;
        public bool CanBeStacked => true;

        public bool IsBlocking => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public int AmountOfSlots => 100;

        public string CommandFriendlyName => "rag";

        public string LongName => "Rag";

        public int Object => -1279773008;

        public int Amount { get; set; }


        public Vector3 TreePos { get; set; } 

    }
}
