using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared.Math;
using MongoDB.Bson;

using mtgvrp.core;
using mtgvrp.database_manager;
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
        public Object TreeObj { get; set; }

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
                TreeMarker = new MarkerZone(TreePos.Add(new Vector3(1, 0, -0.5)), TreeRot, 0);
                TreeMarker.MarkerType = 1;
                TreeMarker.MarkerColor[2] = 0;
                TreeMarker.MarkerScale = new Vector3(1, 1, 1);
                TreeMarker.UseColZone = false;
                TreeMarker.UseBlip = false;
                TreeMarker.UseText = false;
                TreeMarker.Create();
            }

            switch (Stage)
            {
                case Stages.Processing:
                    TreeText = API.shared.createTextLabel("~g~Hit the tree to process it. ~n~" + ProcessPercentage + "%", TreePos, 10f, 1f, true);
                    TreeMarker.Location = TreePos;
                    TreeMarker.Refresh();
                    API.shared.attachEntityToEntity(TreeText, TreeObj, "0", new Vector3(1, 1, 1.5), new Vector3());
                    API.shared.attachEntityToEntity(TreeMarker.Marker, TreeObj, "0", new Vector3(1, 0, 1.5), new Vector3(-90, 0, 0));
                    break;
                case Stages.Cutting:
                    TreeText = API.shared.createTextLabel("~g~" + CutPercentage + "% Cut.~n~Tree", TreePos, 10f, 1f, true);
                    TreeMarker.Location = TreePos;
                    TreeMarker.Refresh();
                    API.shared.attachEntityToEntity(TreeText, TreeObj, "0", new Vector3(1, 0, 1), new Vector3());
                    API.shared.attachEntityToEntity(TreeMarker.Marker, TreeObj, "0", new Vector3(1, 0, 0), new Vector3());
                    break;
                case Stages.Waiting:
                    TreeText = API.shared.createTextLabel("~g~Waiting to be picked, use /pickupwood with a Flatbed.", TreePos, 10f, 1f, true);
                    TreeMarker.Location = TreePos;
                    TreeMarker.Refresh();      
                    API.shared.attachEntityToEntity(TreeText, TreeObj, "0", new Vector3(1, 0, 1), new Vector3());
                    API.shared.attachEntityToEntity(TreeMarker.Marker, TreeObj, "0", new Vector3(1, 0, 0), new Vector3());
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

            TreeObj?.setSyncedData("IS_TREE", true);
        }

        public void UpdateAllTree()
        {
            UpdateTreeObject();
            UpdateTreeText();
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
            CutPercentage = 0;
            ProcessPercentage = 0;
            UpdateAllTree();
        }
    }
}
