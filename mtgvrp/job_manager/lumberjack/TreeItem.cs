using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkServer;
using mtgvrp.inventory;
using mtgvrp.inventory.bags;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.door_manager;
using mtgvrp.player_manager;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.job_manager.lumberjack
{
    public class Tree
    {
        public static List<Tree> Trees = new List<Tree>();

        [BsonId]
        public ObjectId Id { get; set; }

        public Vector3 TreePos { get; set; }
        public Vector3 TreeRot { get; set; }

        [BsonIgnore]
        public GTANetworkServer.Object TreeObj { get; set; }

        [BsonIgnore]
        public TextLabel TreeText { get; set; }
        [BsonIgnore]
        public MarkerZone TreeMarker { get; set; }

        [BsonIgnore]
        public int CutPercentage { get; set; }

        [BsonIgnore]
        public int ProcessPercentage { get; set; }

        [BsonIgnore]
        public Timer RespawnTimer { get; set; }

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
            var filter = MongoDB.Driver.Builders<Tree>.Filter.Eq("_id", Id);
            DatabaseManager.TreesTable.ReplaceOne(filter, this);
        }

        public void Delete()
        {
            if(API.shared.doesEntityExist(TreeObj))
                API.shared.deleteEntity(TreeObj);

            if (API.shared.doesEntityExist(TreeText))
                API.shared.deleteEntity(TreeText);

            TreeMarker.Destroy();

            var filter = MongoDB.Driver.Builders<Tree>.Filter.Eq("_id", Id);
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

            if (TreeMarker == null)
            {
                TreeMarker = new MarkerZone(TreePos.Add(new Vector3(1, 0, 0)), new Vector3(0, 0, 0), 0, 5);
                TreeMarker.MarkerType = 1;
                TreeMarker.Green = 0;
                TreeMarker.Scale = new Vector3(0.75, 0.75, 0.75);
                TreeMarker.Create();
            }

            switch (Stage)
            {
                case Stages.Processing:
                    TreeText = API.shared.createTextLabel("~g~Hit the tree to process it. ~n~" + ProcessPercentage + "%", TreePos.Add(new Vector3(0.5, -1, 1)), 10f, 1f, true);
                    TreeMarker.Location = TreePos.Add(new Vector3(1.0, -1, -0.5));
                    TreeMarker.Refresh();
                    break;
                case Stages.Cutting:
                    TreeText = API.shared.createTextLabel("~g~" + CutPercentage + "% Cut.~n~Tree", TreePos.Add(new Vector3(1, 0, 1)), 10f, 1f, true);
                    TreeMarker.Location = TreePos.Add(new Vector3(1.5, 0, -0.5));
                    TreeMarker.Refresh();
                    break;
                case Stages.Waiting:
                    TreeText = API.shared.createTextLabel("~g~Waiting to be picked, use /pickupwood with a Forklift.", TreePos.Add(new Vector3(1, 0, 1)), 10f, 1f, true);
                    TreeMarker.Location = TreePos.Add(new Vector3(1.5, 0, -0.5));
                    TreeMarker.Refresh();             
                    break;

                default:
                    TreeText = null;
                    TreeMarker.Destroy();
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
                case Stages.Hidden:
                    TreeObj = null;
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
            Trees = new List<Tree>();
            foreach (var tree in DatabaseManager.TreesTable.Find(FilterDefinition<Tree>.Empty).ToList())
            {
                tree.CreateTree();
                Trees.Add(tree);
            }
        }

        public void RespawnTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RespawnTimer.Stop();
            Stage = Stages.Cutting;
            UpdateAllTree();
        }
    }
}
