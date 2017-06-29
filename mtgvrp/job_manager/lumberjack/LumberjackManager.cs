using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.core.Help;
using MongoDB.Bson;

namespace mtgvrp.job_manager.lumberjack
{
    public class LumberjackManager : Script
    {
        public LumberjackManager()
        {
            TreeItem.LoadTrees();

            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            var character = sender.GetCharacter();
            if (eventName == "lumberjack_hittree" && character.JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                var tree = TreeItem.Trees.SingleOrDefault(x => x.TreeText.position.DistanceTo(sender.position) <= 1.5);
                if (tree == null)
                    return;

                tree.CutPercentage += 10;
                tree.UpdateTreeText();

                if (tree.CutPercentage >= 100)
                {
                    API.setEntityRotation(tree.TreeObj, new Vector3(90, 0, 0));
                }
            }
            else if (eventName == "lumberjack_settreepositionadmin")
            {
                string id = (string) arguments[0];
                Vector3 pos = (Vector3) arguments[1];
                Vector3 rot = (Vector3) arguments[2];

                var tree = TreeItem.Trees.SingleOrDefault(x => x.Id.ToString() == id);
                if (tree == null)
                    return;

                API.resetEntitySyncedData(tree.TreeObj, "TargetObj");
                tree.TreePos = pos;
                tree.TreeRot = rot;
                tree.Save();
                tree.UpdateAllTree();
                API.sendChatMessageToPlayer(sender, "Success!");
            }
        }

        [Command("createtree"), Help(HelpManager.CommandGroups.AdminLevel4, "Creates a tree to be used in lumberjack.", null)]
        public void CreateTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = new TreeItem {Id = ObjectId.GenerateNewId(DateTime.Now), TreePos = player.position, TreeRot = new Vector3()};
            tree.CreateTree();
            tree.Insert();
            API.setEntitySyncedData(tree.TreeObj, "TargetObj", tree.Id.ToString());
            API.triggerClientEvent(player, "PLACE_OBJECT_ON_GROUND_PROPERLY", tree.Id.ToString(), "lumberjack_settreepositionadmin");
        }
    }
}
