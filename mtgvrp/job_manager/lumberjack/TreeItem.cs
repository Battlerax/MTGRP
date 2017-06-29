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
using mtgvrp.player_manager;
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

        [BsonIgnore]
        public int ProcessPercentage { get; set; }

        public enum Stages
        {
            Cutting,
            Processing,
            Waiting,
            Moving,
            Hidden,
        }

        [BsonIgnore]
        public Stages Stage = Stages.Cutting;

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
            UpdateAllTree();
        }

        public void UpdateTreeText()
        {
            if (TreeText != null && API.shared.doesEntityExist(TreeText))
                API.shared.deleteEntity(TreeText);

            switch (Stage)
            {
                case Stages.Processing:
                    TreeText = API.shared.createTextLabel("~g~Hit the tree to process it. ~n~" + ProcessPercentage + "%", TreePos.Add(new Vector3(0.5, -1, 1)), 5f, 1f, true);
                    break;
                case Stages.Cutting:
                    TreeText = API.shared.createTextLabel("~g~" + CutPercentage + "% Cut.~n~Tree", TreePos.Add(new Vector3(1, 0, 1)), 5f, 1f, true);
                    break;
                case Stages.Waiting:
                    TreeText = API.shared.createTextLabel("~g~Waiting to be picked, use /pickupwood with a Forklift.", TreePos.Add(new Vector3(1, 0, 1)), 5f, 1f, true);
                    break;

                default:
                    TreeText = null;
                    break;
            }
        }

        public void UpdateTreeObject()
        {
            if (TreeObj != null && API.shared.doesEntityExist(TreeObj))
                API.shared.deleteEntity(TreeObj);

            switch (Stage)
            {
                case Stages.Waiting:
                    TreeObj = API.shared.createObject(-1186441238, TreePos, TreeRot);
                    break;
                default:
                    TreeObj = API.shared.createObject(-1279773008, TreePos, TreeRot);
                    break;
            }
        }

        public void UpdateAllTree()
        {
            UpdateTreeText();
            UpdateTreeObject();
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
