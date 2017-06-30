using System;
using System.Collections.Generic;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.property_system;

namespace mtgvrp.job_manager.garbageman
{
    public class Garbageman : Script
    {

        public Garbageman()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "garbage_throwbag":
                    {
                        Character character = API.getEntityData(player.handle, "Character");

                        API.playPlayerAnimation(player, (int)(Animations.AnimationFlags.StopOnLastFrame), "anim@heists@narcotics@trash", "throw_ranged_b_bin_bag");
                        API.deleteEntity(character.GarbageBag);
                        character.GarbageBag = null;

                        vehicle_manager.Vehicle closestVeh = VehicleManager.GetNearestVehicle(player, 5f);

                        if (closestVeh == null || closestVeh.Job.Type != JobManager.JobTypes.Garbageman)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag into the air.", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~You must throw the garbage bag into a garbage truck!");
                            return;
                        }

                        if (player.rotation.Y > API.getEntityRotation(closestVeh.NetHandle).Y + 20 || player.rotation.Y < API.getEntityRotation(closestVeh.NetHandle).Y - 20)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and misses.", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~You failed to throw the garbage bag into the truck!");
                            return;
                        }

                        if (closestVeh.GarbageBags >= 10)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and the garbage goes everywhere!", ChatManager.RoleplayMe);
                            player.sendChatMessage("~r~Garbage trucks can only hold 10 bags!");
                            return;
                        }

                        ChatManager.RoleplayMessage(character, "successfully throws the garbage into the garbage truck.", ChatManager.RoleplayMe);
                        player.sendChatMessage("~b~Pick up another garbage bag or deliver the garbage bags to the depot!");
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

                int result = DateTime.Compare(character.CanPickupTrash, DateTime.Now);

                if (result == 1 && !character.IsOnGarbageRun)
                {
                    player.sendChatMessage($"Please wait {character.CanPickupTrash.Subtract(DateTime.Now).Minutes} more minutes before picking up more trash.");
                    return;
                }

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

                if (maxGarbage == 0)
                {
                    player.sendChatMessage("There is no garbage to pick up! Try again soon.");
                    API.warpPlayerOutOfVehicle(player);
                    return;
                }

                API.triggerClientEvent(player, "garbage_setwaypoint", TargetProperty.GarbagePoint);
                character.IsOnGarbageRun = true;
                character.CanPickupTrash = DateTime.Now.AddMinutes(30);
                player.sendChatMessage("A garbage waypoint has been set on your map. You have 5 minutes to pick up the trash!");
                character.GarbageTimeLeftTimer = new Timer { Interval = 1000 };
                character.GarbageTimeLeftTimer.Elapsed += delegate { UpdateTimer(player); };
                character.GarbageTimeLeftTimer.Start();
                veh.RespawnTimer = new Timer { Interval = 1000 * 300 };
                veh.RespawnTimer.Elapsed += delegate { RespawnGarbageTruck(player, veh); };
                veh.RespawnTimer.Start();

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
            VehicleManager.respawn_vehicle(vehicle);
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

        [Command("pickuptrash")]
        public void pickuptrash_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.sendChatMessage("You must be a garbageman to use this command!");
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
            API.attachEntityToEntity(character.GarbageBag, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(180, 0, 0));
            API.triggerClientEvent(player, "garbage_holdbag");
            player.sendChatMessage("You are holding a garbage bag. Press LMB to throw it into the back of the garbage truck");
        }

        //Commands
    }
}
