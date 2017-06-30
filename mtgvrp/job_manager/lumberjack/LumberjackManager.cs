using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using MongoDB.Bson;
using Timer = System.Timers.Timer;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

namespace mtgvrp.job_manager.lumberjack
{
    public class LumberjackManager : Script
    {
        public LumberjackManager()
        {
            Tree.LoadTrees();

            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            if (API.getEntityModel(vehicle) == (int)VehicleHash.Forklift && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                Vehicle veh = API.getEntityData(vehicle, "Vehicle");
                if (veh.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = API.getEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                if(API.hasEntityData(vehicle, "TREE_DRIVER"))
                {
                    int id = API.getEntityData(vehicle, "TREE_DRIVER");
                    if (id != player.GetCharacter().Id)
                    {
                        API.warpPlayerOutOfVehicle(player);
                        API.sendChatMessageToPlayer(player, "This is not yours.");
                        return;
                    }
                }

                if (API.hasEntityData(vehicle, "Tree_Cancel_Timer"))
                {
                    System.Threading.Timer timer = API.getEntityData(vehicle, "Tree_Cancel_Timer");
                    timer.Dispose();
                    API.resetEntityData(vehicle, "Tree_Cancel_Timer");
                    API.sendChatMessageToPlayer(player, "You've got back into your forklift.");
                }
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (API.getEntityModel(vehicle) == (int) VehicleHash.Forklift && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                Vehicle veh = API.getEntityData(vehicle, "Vehicle");
                if (veh.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = API.getEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                API.setEntityData(vehicle, "Tree_Cancel_Timer", new System.Threading.Timer(state =>
                {
                    Vehicle vehN = API.getEntityData(vehicle, "Vehicle");
                    Tree ttree = API.getEntityData(vehicle, "TREE_OBJ");
                    if (ttree == null)
                    {
                        return;
                    }
                    ttree.Stage = Tree.Stages.Cutting;
                    ttree.UpdateAllTree();

                    API.resetEntityData(vehicle, "TREE_OBJ");
                    vehN.Respawn();
                    API.resetEntityData(vehicle, "Tree_Cancel_Timer");
                    API.resetEntityData(vehicle, "TREE_DRIVER");
                    API.sendChatMessageToPlayer(player, "Wood run cancelled.");
                    API.setBlipRouteVisible(player.GetCharacter().JobOne.MiscOne.Blip, false);

                }, null, 60000, Timeout.Infinite));
                API.sendChatMessageToPlayer(player, "You've got 1 minute to get back to your vehicle or the wood will be reset.");
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            var character = sender.GetCharacter();
            if (eventName == "lumberjack_hittree" && character.JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                var tree = Tree.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(sender.position) <= 1.5);
                if (tree == null)
                    return;
                if (tree.Stage == Tree.Stages.Cutting)
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
                        tree.Stage = Tree.Stages.Processing;
                    }

                    tree.UpdateTreeText();

                    var rnd = new Random();
                    if (rnd.Next(0, 1000) <= 0)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                    }
                }
                else if (tree.Stage == Tree.Stages.Processing)
                {
                    if (tree.ProcessPercentage >= 100)
                    {
                        API.sendChatMessageToPlayer(sender, "This tree is already cut, use /pickuptree to pick it up.");
                        return;
                    }

                    tree.ProcessPercentage += 10;

                    if (tree.ProcessPercentage >= 100)
                    {
                        tree.Stage = Tree.Stages.Waiting;
                        tree.UpdateAllTree();
                    }

                    tree.UpdateTreeText();

                    var rnd = new Random();
                    if (rnd.Next(0, 1000) <= 0)
                    {
                        API.sendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                    }
                }

            }
            else if (eventName == "lumberjack_settreepositionadmin")
            {
                string id = (string) arguments[0];
                Vector3 pos = (Vector3) arguments[1];
                Vector3 rot = (Vector3) arguments[2];

                var tree = Tree.Trees.SingleOrDefault(x => x.Id.ToString() == id);
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

            var tree = new Tree {Id = ObjectId.GenerateNewId(DateTime.Now), TreePos = player.position, TreeRot = new Vector3()};
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

            var tree = Tree.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(player.position) <= 1.5);
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

                if (API.hasEntityData(vehicle.NetHandle, "TREE_OBJ"))
                {
                    API.sendChatMessageToPlayer(player, "This forklift is already holding some logs.");
                    return;
                }

                var tree = Tree.Trees.SingleOrDefault(x => x.TreeText?.position.DistanceTo(player.position) <= 2);
                if (tree == null || tree?.Stage != Tree.Stages.Waiting)
                {
                    API.sendChatMessageToPlayer(player, "You aren't near a tree.");
                    return;
                }

                tree.Stage = Tree.Stages.Moving;
                tree.UpdateTreeText();
                API.attachEntityToEntity(tree.TreeObj, API.getPlayerVehicle(player), "forks_attach", new Vector3(), new Vector3(0, 0, 90));

                ChatManager.RoleplayMessage(player, "picks up the woods using the forklift.", ChatManager.RoleplayMe);

                API.setBlipRouteVisible(character.JobOne.MiscOne.Blip, true);
                API.setBlipRouteColor(character.JobOne.MiscOne.Blip, 59);

                API.setEntityData(vehicle.NetHandle, "TREE_OBJ", tree);
                API.setEntityData(vehicle.NetHandle, "TREE_DRIVER", character.Id);
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
                Tree tree = API.getEntityData(API.getPlayerVehicle(player), "TREE_OBJ");
                if (tree == null)
                {
                    API.sendChatMessageToPlayer(player, "You dont have any wood on your forklift.");
                    return;
                }

                tree.Stage = Tree.Stages.Hidden;
                tree.UpdateAllTree();
                tree.RespawnTimer = new Timer(1.8e+6);
                tree.RespawnTimer.Elapsed += tree.RespawnTimer_Elapsed;
                tree.RespawnTimer.Start();

                Vehicle vehicle = API.getEntityData(API.getPlayerVehicle(player), "Vehicle");
                API.resetEntityData(API.getPlayerVehicle(player), "TREE_OBJ");
                API.warpPlayerOutOfVehicle(player);
                vehicle.Respawn();
                API.resetEntityData(API.getPlayerVehicle(player), "TREE_DRIVER");
                API.setBlipRouteVisible(character.JobOne.MiscOne.Blip, false);

                InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), 500);
                API.sendChatMessageToPlayer(player, "* You have sucessfully sold your wood for ~g~$500");
            }
        }
    }
}
