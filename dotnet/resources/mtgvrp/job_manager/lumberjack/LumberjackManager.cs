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
using GameVehicle = mtgvrp.vehicle_manager.GameVehicle;
using System.Threading.Tasks;

namespace mtgvrp.job_manager.lumberjack
{
    public class LumberjackManager : Script
    {
        public LumberjackManager()
        {
            Tree.LoadTrees();

        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Client player, Vehicle vehicle, sbyte seat)
        {
            if(vehicle.GetVehicle() == null)
                return;

            if (API.GetEntityModel(vehicle) == (int)VehicleHash.Flatbed && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                GameVehicle veh = NAPI.Data.GetEntityData(vehicle, "Vehicle");
                if (veh.Job?.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = NAPI.Data.GetEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                if(NAPI.Data.HasEntityData(vehicle, "TREE_DRIVER"))
                {
                    int id = NAPI.Data.GetEntityData(vehicle, "TREE_DRIVER");
                    if (id != player.GetCharacter().Id)
                    {
                        //API.Delay(1000, true, () => API.WarpPlayerOutOfVehicle(player));
                        Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                        NAPI.Chat.SendChatMessageToPlayer(player, "This is not yours.");
                        return;
                    }
                }

                if (NAPI.Data.HasEntityData(vehicle, "Tree_Cancel_Timer"))
                {
                    System.Threading.Timer timer = NAPI.Data.GetEntityData(vehicle, "Tree_Cancel_Timer");
                    timer.Dispose();
                    NAPI.Data.ResetEntityData(vehicle, "Tree_Cancel_Timer");
                    NAPI.Chat.SendChatMessageToPlayer(player, "You've got back into your vehicle.");
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (API.GetEntityModel(vehicle) == (int) VehicleHash.Flatbed && player.GetCharacter().JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                GameVehicle veh = NAPI.Data.GetEntityData(vehicle, "Vehicle");
                if (veh.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    return;
                }

                Tree tree = NAPI.Data.GetEntityData(vehicle, "TREE_OBJ");
                if (tree == null)
                {
                    return;
                }

                NAPI.Data.SetEntityData(vehicle, "Tree_Cancel_Timer", new System.Threading.Timer(state =>
                {
                    GameVehicle vehN = NAPI.Data.GetEntityData(vehicle, "Vehicle");
                    Tree ttree = NAPI.Data.GetEntityData(vehicle, "TREE_OBJ");
                    if (ttree == null)
                    {
                        return;
                    }
                    ttree.Stage = Tree.Stages.Cutting;
                    ttree.UpdateAllTree();

                    NAPI.Data.ResetEntityData(vehicle, "TREE_OBJ");
                    vehN.Respawn();
                    NAPI.Data.ResetEntityData(vehicle, "Tree_Cancel_Timer");
                    NAPI.Data.ResetEntityData(vehicle, "TREE_DRIVER");
                    NAPI.Chat.SendChatMessageToPlayer(player, "Wood run cancelled.");
                    NAPI.ClientEvent.TriggerClientEvent(player, "update_beacon", new Vector3());

                }, null, 60000, Timeout.Infinite));
                NAPI.Chat.SendChatMessageToPlayer(player, "You've got 1 minute to get back to your vehicle or the wood will be reset.");
            }
        }

        [RemoteEvent("lumberjack_hittree")]
        public void LumberjackHittree(Client sender, params object[] arguments)
        {
            var character = sender.GetCharacter();
            if (character.JobOne.Type == JobManager.JobTypes.Lumberjack)
            {
                var tree = Tree.Trees.FirstOrDefault(x => x.TreeObj.Position.DistanceTo(sender.Position) <= 3.0f);
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
                        NAPI.Chat.SendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                        API.StopPlayerAnimation(sender);
                        //API.StopPedAnimation(sender);
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
                        NAPI.Chat.SendChatMessageToPlayer(sender, "~r~* Your axe would break.");
                        InventoryManager.DeleteInventoryItem<Weapon>(character, 1,
                            x => x.CommandFriendlyName == "Hatchet");
                        API.StopPlayerAnimation(sender);
                        //API.StopPedAnimation(sender);
                    }
                }
            }
        }

        [RemoteEvent("TreePlaced")]
        public void TreePlaced(Client sender, params object[] arguments)
        {
            var tree = Tree.Trees.First(x => x.TreeObj == (Entity)arguments[0]);
            tree.TreePos = tree.TreeObj.Position;
            tree.TreePos = tree.TreeObj.Position;
        }

        [Command("createtree"), Help(HelpManager.CommandGroups.LumberJob, "Creates a tree for lumberjack under you.")]
        public void CreateTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = new Tree {Id = ObjectId.GenerateNewId(DateTime.Now), TreePos = player.Position, TreeRot = new Vector3()};
            tree.CreateTree();
            tree.Insert();
            API.SetEntitySharedData(tree.TreeObj, "TargetObj", tree.Id.ToString());
            NAPI.ClientEvent.TriggerClientEvent(player, "PLACE_OBJECT_ON_GROUND_PROPERLY", tree.TreeObj, "TreePlaced");
        }

        [Command("deletetree"), Help(HelpManager.CommandGroups.LumberJob, "Delete the nearest lumberjack tree to you.")]
        public void DeleteTreeCmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 4)
                return;

            var tree = Tree.Trees.FirstOrDefault(x => x.TreeMarker?.Location.DistanceTo(player.Position) <= 1.5);
            if (tree == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't near a tree.");
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
                NAPI.Notification.SendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player) && API.GetEntityModel(NAPI.Player.GetPlayerVehicle(player)) == (int)VehicleHash.Flatbed)
            {
                GameVehicle vehicle = NAPI.Data.GetEntityData(NAPI.Player.GetPlayerVehicle(player), "Vehicle");
                if (vehicle.Job.Type != JobManager.JobTypes.Lumberjack)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "This is not a Lumberjack vehicle.");
                    return;
                }

                if (NAPI.Data.HasEntityData(vehicle.Entity, "TREE_OBJ"))
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "This vehicle is already holding some logs.");
                    return;
                }

                var tree = Tree.Trees.FirstOrDefault(x => x.TreeObj.Position.DistanceTo(player.Position) <= 10.0f && x.Stage == Tree.Stages.Waiting);
                if (tree == null || tree?.Stage != Tree.Stages.Waiting)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You aren't near a tree.");
                    return;
                }

                tree.Stage = Tree.Stages.Moving;
                tree.UpdateTreeText();
                API.AttachEntityToEntity(tree.TreeObj, NAPI.Player.GetPlayerVehicle(player), "bodyshell", new Vector3(0, -1.5, 0.3), new Vector3(0, 0, 0));

                ChatManager.RoleplayMessage(player, "picks up the woods into the truck.", ChatManager.RoleplayMe);

                NAPI.ClientEvent.TriggerClientEvent(player, "update_beacon", character.JobOne.MiscOne.Location);
                

                NAPI.Data.SetEntityData(vehicle.Entity, "TREE_OBJ", tree);
                NAPI.Data.SetEntityData(vehicle.Entity, "TREE_DRIVER", character.Id);
                NAPI.Chat.SendChatMessageToPlayer(player, "Go to the HQ to sell your wood.");
            }
            else
                NAPI.Chat.SendChatMessageToPlayer(player, "You have to be in a truck to pickup the wood.");
        }

        [Command("sellwood"), Help(HelpManager.CommandGroups.LumberJob, "Sells the wood you currently have on the truck.")]
        public void SellWoodCmd(Client player)
        {
            var character = player.GetCharacter();
            if (character.JobOne.Type != JobManager.JobTypes.Lumberjack)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You must be a lumberjack.");
                return;
            }

            if (character.JobZoneType != 2 || JobManager.GetJobById(character.JobZone).Type != JobManager.JobTypes.Lumberjack)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, Color.White, "You are not near the sell wood point!");
                return;
            }

            if (NAPI.Player.IsPlayerInAnyVehicle(player) && API.GetEntityModel(NAPI.Player.GetPlayerVehicle(player)) ==
                (int) VehicleHash.Flatbed)
            {
                Tree tree = NAPI.Data.GetEntityData(NAPI.Player.GetPlayerVehicle(player), "TREE_OBJ");
                if (tree == null)
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You dont have any wood on your vehicle.");
                    return;
                }

                tree.Stage = Tree.Stages.Hidden;
                tree.UpdateAllTree();
                tree.RespawnTimer = new Timer(1.8e+6);
                tree.RespawnTimer.Elapsed += tree.RespawnTimer_Elapsed;
                tree.RespawnTimer.Start();

                GameVehicle vehicle = NAPI.Data.GetEntityData(NAPI.Player.GetPlayerVehicle(player), "Vehicle");
                NAPI.Data.ResetEntityData(NAPI.Player.GetPlayerVehicle(player), "TREE_OBJ");
                Task.Delay(1000).ContinueWith(t => API.WarpPlayerOutOfVehicle(player)); // CONV NOTE: delay fixme
                VehicleManager.respawn_vehicle(vehicle);
                NAPI.Data.ResetEntityData(NAPI.Player.GetPlayerVehicle(player), "TREE_DRIVER");
                NAPI.ClientEvent.TriggerClientEvent(player, "update_beacon", new Vector3());

                InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), 350, true);
                NAPI.Chat.SendChatMessageToPlayer(player, "* You have sucessfully sold your wood for ~g~$350");
                LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {character.CharacterName}[{player.GetAccount().AccountName}] has earned $200 from selling wood.");

                SettingsManager.Settings.WoodSupplies += 50;
            }
        }
    }
}
