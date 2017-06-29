using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using GTANetworkShared;
using mtgvrp.database_manager;
using mtgvrp.door_manager;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.job_manager.lumberjack
{
    public class TreeItem : IInventoryItem
    {
        public static List<TreeItem> Trees = new List<TreeItem>();

        [BsonId]
        public ObjectId Id { get; set; }

        public bool CanBeGiven => true;
        public bool CanBeDropped => true;
        public bool CanBeStashed => false;
        public bool CanBeStacked => true;

        public bool IsBlocking => true;

        public Dictionary<Type, int> MaxAmount => new Dictionary<Type, int>();

        public int AmountOfSlots => 100;

        public string CommandFriendlyName => "treelog";

        public string LongName => "Tree Log";

        public int Object => -1279773008;

        public int Amount { get; set; }


        public Vector3 TreePos { get; set; }
        public Vector3 TreeRot { get; set; }

        [BsonIgnore]
        public GTANetworkServer.Object TreeObj { get; set; }

        [BsonIgnore]
        public TextLabel TreeText { get; set; }

        [BsonIgnore]
        public int CutPercentage { get; set; }

        public void Insert()
        {
            DatabaseManager.TreesTable.InsertOne(this);
            Trees.Add(this);
        }

        public void Save()
        {
            var filter = MongoDB.Driver.Builders<TreeItem>.Filter.Eq("_id", Id);
            DatabaseManager.TreesTable.ReplaceOne(filter, this);
        }

        public void Delete()
        {
            if(API.shared.doesEntityExist(TreeObj))
                API.shared.deleteEntity(TreeObj);

            if (API.shared.doesEntityExist(TreeText))
                API.shared.deleteEntity(TreeText);

            var filter = MongoDB.Driver.Builders<TreeItem>.Filter.Eq("_id", Id);
            DatabaseManager.TreesTable.DeleteOne(filter);
            Trees.Remove(this);
        }

        public void CreateTree()
        {
            TreeObj = API.shared.createObject(-1279773008, TreePos, TreeRot);
            TreeText = API.shared.createTextLabel("~g~" + CutPercentage + "% Cut.~n~Tree", TreePos.Add(new Vector3(1, 0, 1)), 5f, 1f, true);
        }

        public void UpdateTreeText()
        {
            if (API.shared.doesEntityExist(TreeText))
                API.shared.deleteEntity(TreeText);

            TreeText = API.shared.createTextLabel("~g~" + CutPercentage + "% Cut.~n~Tree", TreePos.Add(new Vector3(1, 0, 1)), 5f, 1f, true);
        }

        public void UpdateAllTree()
        {
            UpdateTreeText();

            if (API.shared.doesEntityExist(TreeObj))
                API.shared.deleteEntity(TreeObj);

            TreeObj = API.shared.createObject(-1279773008, TreePos, TreeRot);
        }

        public static void LoadTrees()
        {
            Trees = new List<TreeItem>();
            foreach (var tree in DatabaseManager.TreesTable.Find(FilterDefinition<TreeItem>.Empty).ToList())
            {
                tree.CreateTree();
                Trees.Add(tree);
            }
        }
    }
}
