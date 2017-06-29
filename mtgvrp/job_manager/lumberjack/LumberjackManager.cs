using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using MongoDB.Bson;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

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
                var tree = TreeItem.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(sender.position) <= 1.5);
                if (tree == null)
                    return;
                if (tree.Stage == TreeItem.Stages.Cutting)
                {
                    if (tree.CutPercentage >= 100)
                    {
                        API.sendChatMessageToPlayer(sender, "This tree is already cut, use /pickuptree to pick it up.");
                        return;
                    }

                    tree.CutPercentage += 10;

                    if (tree.CutPercentage >= 100)
                    {
                        API.setEntityRotation(tree.TreeObj, new Vector3(90, 0, 0));
                        ChatManager.RoleplayMessage(sender, "A tree would fall over on the ground.",
                            ChatManager.RoleplayDo);
                        tree.Stage = TreeItem.Stages.Processing;
                    }

                    tree.UpdateTreeText();
                }
                else if (tree.Stage == TreeItem.Stages.Processing)
                {
                    if (tree.ProcessPercentage >= 100)
                    {
                        API.sendChatMessageToPlayer(sender, "This tree is already cut, use /pickuptree to pick it up.");
                        return;
                    }

                    tree.ProcessPercentage += 10;

                    if (tree.ProcessPercentage >= 100)
                    {
                        tree.Stage = TreeItem.Stages.Waiting;
                        tree.UpdateAllTree();
                    }

                    tree.UpdateTreeText();
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

        [Command("createtree")]
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

        [Command("deletetree")]
        public void DeleteTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = TreeItem.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(player.position) <= 1.5);
            if (tree == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't near a tree.");
                return;
            }

            tree.Delete();
        }

        [Command("pickupwood")]
        public void PickupWood(Client player)
        {
            var character = player.GetCharacter();
            if (character.JobOne.Type != JobManager.JobTypes.Lumberjack)
            {
                API.sendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (API.isPlayerInAnyVehicle(player) && API.getEntityModel(API.getPlayerVehicle(player)) == (int)VehicleHash.Forklift)
            {
                Vehicle vehicle = API.getEntityData(API.getPlayerVehicle(player), "Vehicle");
                if (vehicle.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    API.sendChatMessageToPlayer(player, "This is not a Lumberjack vehicle.");
                    return;
                }

                var tree = TreeItem.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(player.position) <= 2);
                if (tree == null || tree?.Stage != TreeItem.Stages.Waiting)
                {
                    API.sendChatMessageToPlayer(player, "You aren't near a tree.");
                    return;
                }

                tree.Stage = TreeItem.Stages.Moving;
                tree.UpdateTreeText();
                API.attachEntityToEntity(tree.TreeObj, API.getPlayerVehicle(player), "forks_attach", new Vector3(), new Vector3(0, 0, 90));

                ChatManager.RoleplayMessage(player, "picks up the woods using the forklift.", ChatManager.RoleplayMe);

                API.setBlipRouteVisible(character.JobOne.MiscOne.Blip, true);
                API.setBlipRouteColor(character.JobOne.MiscOne.Blip, 59);

                API.setEntityData(vehicle.NetHandle, "TREE_OBJ", tree);
                API.sendChatMessageToPlayer(player, "Goto the HQ to sell your wood.");
            }
            else
                API.sendChatMessageToPlayer(player, "You have to be in a forklift to pickup the wood.");
        }

        [Command("sellwood")]
        public void SellWoodCmd(Client player)
        {
            var character = player.GetCharacter();
            if (character.JobOne.Type != JobManager.JobTypes.Lumberjack)
            {
                API.sendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (character.JobZoneType != 2)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not near the sell wood point!");
                return;
            }

            var job = JobManager.GetJobById(character.JobZone);

            if (job == null)
            {
                API.sendChatMessageToPlayer(player, "null job");
                return;
            }

            if (job.Type != JobManager.JobTypes.Lumberjack)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You are not near the sell wood point!");
                return;
            }

            if (API.isPlayerInAnyVehicle(player) && API.getEntityModel(API.getPlayerVehicle(player)) ==
                (int) VehicleHash.Forklift)
            {
                TreeItem tree = API.getEntityData(API.getPlayerVehicle(player), "TREE_OBJ");
                if (tree == null)
                {
                    API.sendChatMessageToPlayer(player, "You dont have any wood on your forklift.");
                    return;
                }

                tree.Stage = TreeItem.Stages.Hidden;
                tree.UpdateAllTree();
                tree.RespawnTimer = new Timer(1.8e+6);
                tree.RespawnTimer.Elapsed += tree.RespawnTimer_Elapsed;
                tree.RespawnTimer.Start();

                Vehicle vehicle = API.getEntityData(API.getPlayerVehicle(player), "Vehicle");
                API.resetEntityData(API.getPlayerVehicle(player), "TREE_OBJ");
                API.warpPlayerOutOfVehicle(player);
                vehicle.Respawn();

                InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), 500);
                API.sendChatMessageToPlayer(player, "* You have sucessfully sold your woof for ~g~$500");
            }
        }
    }
}
