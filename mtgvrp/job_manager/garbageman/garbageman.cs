using System;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.property_system;
using mtgvrp.inventory;

namespace mtgvrp.job_manager.garbageman
{
    public class Garbageman : Script
    {

        public Garbageman()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.IsOnGarbageRun)
            {
                character.GarbageTimeLeft = 0;
                character.GarbageTimeLeftTimer.Stop();
                character.GarbageBag.delete();
            }
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "garbage_throwbag":
                    {
                        Character character = API.getEntityData(player.handle, "Character");
                        API.deleteEntity(character.GarbageBag);
                        character.GarbageBag = null;

                        vehicle_manager.Vehicle closestVeh = VehicleManager.GetNearestVehicle(player, 10f);

                        if (closestVeh == null || closestVeh.Job.Type != JobManager.JobTypes.Garbageman)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag into the air.", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~You must throw the garbage bag into the back of the garbage truck!");
                            return;
                        }

                        if (player.rotation.Z > API.getEntityRotation(closestVeh.NetHandle).Z + 15 || player.rotation.Z < API.getEntityRotation(closestVeh.NetHandle).Z - 15)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and misses.", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~You failed to throw the garbage bag into the back of the garbage truck!");
                            return;
                        }

                        if (closestVeh.GarbageBags >= 10)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and the garbage goes everywhere!", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~Garbage trucks can only hold 10 bags!");
                            return;
                        }

                        ChatManager.RoleplayMessage(character, "successfully throws the garbage bag into the garbage truck.", ChatManager.RoleplayMe);
                        player.sendChatMessage("~b~Pick up another garbage bag if you have time or deliver the garbage bags to the depot!");
                        closestVeh.GarbageBags += 1;
                        closestVeh.UpdateMarkers();
                        break;
                    }
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player, "Character");
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if (veh.Job?.Type == JobManager.JobTypes.Garbageman)
            {

                Property TargetProperty = null;
                int maxGarbage = 0;
                foreach (var prop in PropertyManager.Properties)
                {
                    if (prop.HasGarbagePoint)
                    {
                        if (prop.GarbageBags > maxGarbage)
                        {
                            maxGarbage = prop.GarbageBags;
                            TargetProperty = prop;
                        }
                    }
                }

                if (maxGarbage == 0 && !character.IsOnGarbageRun)
                {
                    player.sendChatMessage("There is no garbage to pick up! Try again soon.");
                    API.warpPlayerOutOfVehicle(player);
                    return;
                }

                if (maxGarbage == 0)
                {
                    player.sendChatMessage("~r~There is currently no more garbage to pick up.");
                    return;
                }

                API.triggerClientEvent(player, "garbage_setwaypoint", TargetProperty.GarbagePoint);
                player.sendChatMessage("A garbage waypoint has been set on your map.");

                if (!character.IsOnGarbageRun)
                {
                    character.IsOnGarbageRun = true;
                    player.sendChatMessage("~r~You have 5 minutes to pick up the trash!");
                    character.GarbageTimeLeft = 1000 * 300;
                    character.GarbageTimeLeftTimer = new Timer { Interval = 1000 };
                    character.GarbageTimeLeftTimer.Elapsed += delegate { UpdateTimer(player); };
                    character.GarbageTimeLeftTimer.Start();
                    veh.RespawnTimer = new Timer { Interval = 1000 * 300 };
                    veh.RespawnTimer.Elapsed += delegate { RespawnGarbageTruck(player, veh); };
                    veh.RespawnTimer.Start();
                }

            }
        }

        public static void UpdateTimer(Client player)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            character.GarbageTimeLeft -= 1000;
        }

        public void RespawnGarbageTruck(Client player, vehicle_manager.Vehicle vehicle)
        {
            vehicle.RespawnTimer.Stop();
            vehicle.GarbageBags = 0;
            VehicleManager.respawn_vehicle(vehicle);
            vehicle.UpdateMarkers();
            player.GetCharacter().GarbageTimeLeft = 0;
            player.GetCharacter().GarbageTimeLeftTimer.Stop();
            player.GetCharacter().IsOnGarbageRun = false;
            player.sendChatMessage("~r~Your time to collect garbage is up. Your garbage truck was removed.");
        }

        public static void SendNotificationToGarbagemen(string message)
        {
            foreach(var player in PlayerManager.Players)
            {
                if (player.JobOne.Type == JobManager.JobTypes.Garbageman)
                {
                    API.shared.sendPictureNotificationToPlayer(player.Client, message, "CHAR_PROPERTY_CAR_SCRAP_YARD", 
                        0, 1, "Los Santos Sanitations", "Garbage Notification");
                }
            }
        }


        //Commands
        [Command("unloadtrash")]
        public void unloadtrash_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.sendChatMessage("You must be a garbageman to use this command!");
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.sendChatMessage("You must be on a garbage run to unload trash.");
                return;
            }

            if (player.position.DistanceTo(character.JobOne.MiscOne.Location) > 10)
            {
                player.sendChatMessage("You must be at the trash depot to unload the trash.");
                return;
            }

            vehicle_manager.Vehicle closestVeh = VehicleManager.GetNearestVehicle(player, 10f);

            if (closestVeh == null || closestVeh.Job.Type != JobManager.JobTypes.Garbageman)
            {
                player.sendChatMessage("~r~You must be at the back of your garbage truck to unload the trash.");
                return;
            }

            if (player.rotation.Z > API.getEntityRotation(closestVeh.NetHandle).Z + 20 || player.rotation.Z < API.getEntityRotation(closestVeh.NetHandle).Z - 20)
            {
                player.sendChatMessage("~r~You must be at the back of your garbage truck to unload the trash.");
                return;
            }

            if (closestVeh.GarbageBags == 0)
            {
                player.sendChatMessage("~r~Your garbage truck has no trash to unload!");
                return;
            }

            InventoryManager.GiveInventoryItem(character, new Money(), closestVeh.GarbageBags * 100);
            ChatManager.RoleplayMessage(character, "uses the garbage truck's control panel to unload the trash.", ChatManager.RoleplayMe);
            closestVeh.GarbageBags = 0;
            player.sendChatMessage($"You were paid ${closestVeh.GarbageBags * 100} for unloading {closestVeh.GarbageBags} trash bags.");
            API.shared.sendPictureNotificationToPlayer(player, $"Thanks for keeping Los Santos clean!", "CHAR_PROPERTY_CAR_SCRAP_YARD", 0, 1, "Los Santos Sanitations", "Garbage Notification");


        }

        [Command("pickuptrash")]
        public void pickuptrash_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.sendChatMessage("You must be a garbageman to use this command!");
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.sendChatMessage("You must be on a garbage run to pick up trash.");
                return;
            }

            var prop = PropertyManager.IsAtPropertyGarbagePoint(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a garbage point.");
                return;
            }

            if (prop.GarbageBags == 0)
            {
                player.sendChatMessage("There is no garbage to pick up.");
                return;
            }

            if (character.GarbageBag != null)
            {
                player.sendChatMessage("You are already carrying a garbage bag.");
                return;
            }

            prop.GarbageBags -= 1;
            prop.UpdateMarkers();
            character.GarbageBag = API.createObject(API.getHashKey("hei_prop_heist_binbag"), player.position, new Vector3());
            API.attachEntityToEntity(character.GarbageBag, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(360, 0, 0));
            API.triggerClientEvent(player, "garbage_holdbag");
            ChatManager.RoleplayMessage(character, "reaches into the trash and pulls out a garbage bag.", ChatManager.RoleplayMe);
            player.sendChatMessage("You are holding a garbage bag. Press LMB to throw it into the back of the garbage truck");
        }
    }
}
