using System;
using System.Linq;
using System.Threading;
using GTANetworkAPI;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.vehicle_manager;
using MongoDB.Bson;
using Color = mtgvrp.core.Color;
using Timer = System.Timers.Timer;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

namespace mtgvrp.job_manager.lumberjack
{
    public class LumberjackManager : Script
    {
        public LumberjackManager()
        {
            Tree.LoadTrees();

            Event.OnClientEventTrigger += API_onClientEventTrigger;
            Event.OnPlayerExitVehicle += API_onPlayerExitVehicle;
            Event.OnPlayerEnterVehicle += API_onPlayerEnterVehicle;
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle, int seat)
        {
            if(vehicle.GetVehicle() == null)
                return;

            if (API.GetEntityModel(vehicle) == (int)VehicleHash.Flatbed && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                Vehicle veh = API.GetEntityData(vehicle, "Vehicle");
                if (veh.Job?.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = API.GetEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                if(API.HasEntityData(vehicle, "TREE_DRIVER"))
                {
                    int id = API.GetEntityData(vehicle, "TREE_DRIVER");
                    if (id != player.GetCharacter().Id)
                    {
                        API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));;
                        API.SendChatMessageToPlayer(player, "This is not yours.");
                        return;
                    }
                }

                if (API.HasEntityData(vehicle, "Tree_Cancel_Timer"))
                {
                    System.Threading.Timer timer = API.GetEntityData(vehicle, "Tree_Cancel_Timer");
                    timer.Dispose();
                    API.ResetEntityData(vehicle, "Tree_Cancel_Timer");
                    API.SendChatMessageToPlayer(player, "You've got back into your vehicle.");
                }
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle, int seat)
        {
            if (API.GetEntityModel(vehicle) == (int) VehicleHash.Flatbed && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                Vehicle veh = API.GetEntityData(vehicle, "Vehicle");
                if (veh.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = API.GetEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                API.SetEntityData(vehicle, "Tree_Cancel_Timer", new System.Threading.Timer(state =>
                {
                    Vehicle vehN = API.GetEntityData(vehicle, "Vehicle");
                    Tree ttree = API.GetEntityData(vehicle, "TREE_OBJ");
                    if (ttree == null)
                    {
                        return;
                    }
                    ttree.Stage = Tree.Stages.Cutting;
                    ttree.UpdateAllTree();

                    API.ResetEntityData(vehicle, "TREE_OBJ");
                    vehN.Respawn();
                    API.ResetEntityData(vehicle, "Tree_Cancel_Timer");
                    API.ResetEntityData(vehicle, "TREE_DRIVER");
                    API.SendChatMessageToPlayer(player, "Wood run cancelled.");
                    API.TriggerClientEvent(player, "update_beacon", new Vector3());

                }, null, 60000, Timeout.Infinite));
                API.SendChatMessageToPlayer(player, "You've got 1 minute to get back to your vehicle or the wood will be reset.");
            }
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            var character = sender.GetCharacter();
            if (eventName == "lumberjack_hittree" && character.JobOne.Type == JobManager.JobTypes.Lumberjack)
            {

                var tree = Tree.Trees.FirstOrDefault(x => x.TreeObj?.position?.DistanceTo(sender.Position) <= 3.0f);
                if (tree == null)
                    return;
                if (tree.Stage == Tree.Stages.Cutting)
                {
                    tree.CutPercentage += 10;

                    if (tree.CutPercentage >= 100)
                    {
                        API.SetEntityRotation(tree.TreeObj, new Vector3(90 + tree.TreeRot.X, tree.TreeRot.Y, tree.TreeRot.Z));
                        ChatManager.RoleplayMessage(sender, "A tree would fall over on the ground.",
                            ChatManager.RoleplayDo);
                        tree.Stage = Tree.Stages.Processing;
                    }

                    API.PlayPlayerAnimation(sender, (int)(Animations.AnimationFlags.StopOnLastFrame | Animations.AnimationFlags.OnlyAnimateUpperBody), "melee@large_wpn@streamed_core", "ground_attack_0");
                    tree.UpdateTreeText();

                    var rnd = new Random();
                    if (rnd.Next(0, 1000) <= 0)
                    {
                        API.SendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                        API.StopPlayerAnimation(sender);
                        API.StopPedAnimation(sender);
                    }
                }
                else if (tree.Stage == Tree.Stages.Processing)
                {
                    tree.ProcessPercentage += 10;

                    if (tree.ProcessPercentage >= 100)
                    {
                        tree.Stage = Tree.Stages.Waiting;
                        tree.UpdateAllTree();
                    }

                    API.PlayPlayerAnimation(sender, (int)(Animations.AnimationFlags.StopOnLastFrame | Animations.AnimationFlags.OnlyAnimateUpperBody), "melee@large_wpn@streamed_core", "ground_attack_0");
                    tree.UpdateTreeText();

                    var rnd = new Random();
                    if (rnd.Next(0, 1000) <= 0)
                    {
                        API.SendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                        API.StopPlayerAnimation(sender);
                        API.StopPedAnimation(sender);
                    }
                }

            }
            else if (eventName == "TreePlaced")
            {
                var tree = Tree.Trees.First(x => x.TreeObj == (NetHandle) arguments[0]);
                tree.TreePos = tree.TreeObj.position;
                tree.TreePos = tree.TreeObj.position;
            }
        }

        [Command("createtree"), Help(HelpManager.CommandGroups.LumberJob, "Creates a tree for lumberjack under you.")]
        public void CreateTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = new Tree {Id = ObjectId.GenerateNewId(DateTime.Now), TreePos = player.Position, TreeRot = new Vector3()};
            tree.CreateTree();
            tree.Insert();
            API.SetEntitySyncedData(tree.TreeObj, "TargetObj", tree.Id.ToString());
            API.TriggerClientEvent(player, "PLACE_OBJECT_ON_GROUND_PROPERLY", tree.TreeObj.handle, "TreePlaced");
        }

        [Command("deletetree"), Help(HelpManager.CommandGroups.LumberJob, "Delete the nearest lumberjack tree to you.")]
        public void DeleteTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = Tree.Trees.FirstOrDefault(x => x.TreeMarker?.Location.DistanceTo(player.position) <= 1.5);
            if (tree == null)
            {
                API.SendChatMessageToPlayer(player, "You aren't near a tree.");
                return;
            }

            tree.Delete();
        }

        [Command("pickupwood"), Help(HelpManager.CommandGroups.LumberJob, "Pick up the nearest processed wood to you from the ground to your truck.")]
        public void PickupWood(Client player)
        {
            var character = player.GetCharacter();
            if (character.JobOne.Type != JobManager.JobTypes.Lumberjack)
            {
                API.SendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (API.IsPlayerInAnyVehicle(player) && API.GetEntityModel(API.GetPlayerVehicle(player)) == (int)VehicleHash.Flatbed)
            {
                Vehicle vehicle = API.GetEntityData(API.GetPlayerVehicle(player), "Vehicle");
                if (vehicle.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    API.SendChatMessageToPlayer(player, "This is not a Lumberjack vehicle.");
                    return;
                }

                if (API.HasEntityData(vehicle.NetHandle, "TREE_OBJ"))
                {
                    API.SendChatMessageToPlayer(player, "This vehicle is already holding some logs.");
                    return;
                }

                var tree = Tree.Trees.FirstOrDefault(x => x.TreeObj?.position?.DistanceTo(player.position) <= 10.0f && x.Stage == Tree.Stages.Waiting);
                if (tree == null || tree?.Stage != Tree.Stages.Waiting)
                {
                    API.SendChatMessageToPlayer(player, "You aren't near a tree.");
                    return;
                }

                tree.Stage = Tree.Stages.Moving;
                tree.UpdateTreeText();
                API.AttachEntityToEntity(tree.TreeObj, API.GetPlayerVehicle(player), "bodyshell", new Vector3(0, -1.5, 0.3), new Vector3(0, 0, 0));

                ChatManager.RoleplayMessage(player, "picks up the woods into the truck.", ChatManager.RoleplayMe);

                API.TriggerClientEvent(player, "update_beacon", character.JobOne.MiscOne.Location);
                

                API.SetEntityData(vehicle.NetHandle, "TREE_OBJ", tree);
                API.SetEntityData(vehicle.NetHandle, "TREE_DRIVER", character.Id);
                API.SendChatMessageToPlayer(player, "Go to the HQ to sell your wood.");
            }
            else
                API.SendChatMessageToPlayer(player, "You have to be in a truck to pickup the wood.");
        }

        [Command("sellwood"), Help(HelpManager.CommandGroups.LumberJob, "Sells the wood you currently have on the truck.")]
        public void SellWoodCmd(Client player)
        {
            var character = player.GetCharacter();
            if (character.JobOne.Type != JobManager.JobTypes.Lumberjack)
            {
                API.SendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (character.JobZoneType != 2 || JobManager.GetJobById(character.JobZone).Type != JobManager.JobTypes.Lumberjack)
            {
                API.SendChatMessageToPlayer(player, Color.White, "You are not near the sell wood point!");
                return;
            }

            if (API.IsPlayerInAnyVehicle(player) && API.GetEntityModel(API.GetPlayerVehicle(player)) ==
                (int) VehicleHash.Flatbed)
            {
                Tree tree = API.GetEntityData(API.GetPlayerVehicle(player), "TREE_OBJ");
                if (tree == null)
                {
                    API.SendChatMessageToPlayer(player, "You dont have any wood on your vehicle.");
                    return;
                }

                tree.Stage = Tree.Stages.Hidden;
                tree.UpdateAllTree();
                tree.RespawnTimer = new Timer(1.8e+6);
                tree.RespawnTimer.Elapsed += tree.RespawnTimer_Elapsed;
                tree.RespawnTimer.Start();

                Vehicle vehicle = API.GetEntityData(API.GetPlayerVehicle(player), "Vehicle");
                API.ResetEntityData(API.GetPlayerVehicle(player), "TREE_OBJ");
                API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));;
                VehicleManager.respawn_vehicle(vehicle);
                API.ResetEntityData(API.GetPlayerVehicle(player), "TREE_DRIVER");
                API.TriggerClientEvent(player, "update_beacon", new Vector3());

                InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), 350, true);
                API.SendChatMessageToPlayer(player, "* You have sucessfully sold your wood for ~g~$350");
                LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {character.CharacterName}[{player.GetAccount().AccountName}] has earned $200 from selling wood.");

                SettingsManager.Settings.WoodSupplies += 50;
            }
        }
    }
}
