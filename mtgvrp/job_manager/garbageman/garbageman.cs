using System;
using System.Timers;

using GTANetworkAPI;



using mtgvrp.core;
using mtgvrp.core.Help;
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
            Event.OnPlayerEnterVehicle += API_onPlayerEnterVehicle;
            Event.OnClientEventTrigger += API_onClientEventTrigger;
            Event.OnPlayerDisconnected += API_onPlayerDisconnected;
        }

        private void API_onPlayerDisconnected(Client player, byte type, string reason)
        {
            var c = player.GetCharacter();
            if (c?.GarbageBag != null)
            {
                API.DeleteEntity(c.GarbageBag);
            }
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "garbage_throwbag":
                    {
                        Character character = player.GetCharacter();
                        API.DeleteEntity(character.GarbageBag);
                        character.GarbageBag = null;

                        vehicle_manager.Vehicle closestVeh = VehicleManager.GetClosestVehicle(player, 10f).GetVehicle();

                        if (API.IsPlayerInAnyVehicle(player))
                        {
                            player.SendChatMessage("You cannot be in a vehicle while doing this.");
                            return;
                        }
                        if (closestVeh == null || closestVeh.Job.Type != JobManager.JobTypes.Garbageman)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag into the air.", ChatManager.RoleplayMe);
                            player.SendChatMessage("~r~You must throw the garbage bag into the back of the garbage truck!");
                            return;
                        }
                        if (player.Rotation.Z > API.GetEntityRotation(closestVeh.NetHandle).Z + 30 || player.Rotation.Z < API.GetEntityRotation(closestVeh.NetHandle).Z - 30)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and misses.", ChatManager.RoleplayMe);
                            player.SendChatMessage("~r~You failed to throw the garbage bag into the back of the garbage truck!");
                            return;
                        }
                        if (closestVeh.GarbageBags >= 10)
                        {
                            ChatManager.RoleplayMessage(character, "throws the garbage bag at the garbage truck and the garbage goes everywhere!", ChatManager.RoleplayMe);
                            player.SendChatMessage("~r~Garbage trucks can only hold 10 bags!");
                            return;
                        }

                        ChatManager.RoleplayMessage(character, "successfully throws the garbage bag into the garbage truck.", ChatManager.RoleplayMe);
                        player.SendChatMessage("~b~Pick up another garbage bag if you have time or deliver the garbage bags to the depot!");
                        closestVeh.GarbageBags += 1;
                        closestVeh.UpdateMarkers();
                        break;
                    }
                case "garbage_pickupbag":
                    {
                        Character character = player.GetCharacter();

                        if (character == null)
                            return;

                        if (character.JobOne?.Type != JobManager.JobTypes.Garbageman)
                        {
                            return;
                        }

                        pickuptrash_e(character.Client);
                        break;
                    }
            }
        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle, byte seat)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

            if (veh?.Job?.Type == JobManager.JobTypes.Garbageman && character.JobOne?.Type == JobManager.JobTypes.Garbageman)
            {
                Property targetProperty = null;
                int maxGarbage = 0;
                foreach (var prop in PropertyManager.Properties)
                {
                    if (prop.HasGarbagePoint)
                    {
                        if (prop.GarbageBags > maxGarbage)
                        {
                            maxGarbage = prop.GarbageBags;
                            targetProperty = prop;
                        }
                    }
                }

                if (maxGarbage == 0)
                {

                    int GarbageProperties = 0;
                    foreach(var p in PropertyManager.Properties)
                    {
                        if (p.HasGarbagePoint) { GarbageProperties++; }
                    }

                    if (GarbageProperties == PropertyManager.Properties.Count)
                    {
                        player.SendChatMessage("There is no garbage to pick up!");
                        player.WarpOutOfVehicle();
                        return;
                    }

                    targetProperty = ChooseRandomProperty();

                    while (!targetProperty.HasGarbagePoint)
                    {
                        targetProperty = ChooseRandomProperty();
                    }

                    targetProperty.GarbageBags = 5;
                }

                if(veh.GarbageBags == 10)
                {
                    API.TriggerClientEvent(player, "garbage_setwaypoint", character.JobOne.MiscOne.Location);
                    player.SendChatMessage("You've got 10 bags of garbage in this truck. Unload it at the waypoint on your map.");
                }
                else
                {
                    API.TriggerClientEvent(player, "garbage_setwaypoint", targetProperty.GarbagePoint);
                    player.SendChatMessage("A garbage waypoint has been set on your map.");
                }

                if (!character.IsOnGarbageRun)
                {
                    character.IsOnGarbageRun = true;
                    player.SendChatMessage("~r~You have 15 minutes to pick up the trash!");
                    character.GarbageTimeLeft = 900000;
                    character.GarbageTimeLeftTimer = new Timer { Interval = 1000 };
                    character.GarbageTimeLeftTimer.Elapsed += delegate { UpdateTimer(player); };
                    character.GarbageTimeLeftTimer.Start();
                    character.update_ped();
                    veh.CustomRespawnTimer = new Timer { Interval = 900000  };
                    veh.CustomRespawnTimer.Elapsed += delegate { RespawnGarbageTruck(player, veh); };
                    veh.CustomRespawnTimer.Start();
                }

            }
        }
        
        public static Property ChooseRandomProperty()
        {
            Property randomProp;
            Random rand = new Random();
            int r = rand.Next(PropertyManager.Properties.Count);
            randomProp = PropertyManager.Properties[r];
            return randomProp;
        }

        public static void UpdateTimer(Client player)
        {
            Character character = player.GetCharacter();
            character.GarbageTimeLeft -= 1000;
        }

        public void RespawnGarbageTruck(Client player, vehicle_manager.Vehicle vehicle)
        {
            vehicle.CustomRespawnTimer.Stop();
            vehicle.GarbageBags = 0;
            VehicleManager.respawn_vehicle(vehicle);
            vehicle.UpdateMarkers();
            player.GetCharacter().GarbageTimeLeft = 0;
            player.GetCharacter().GarbageTimeLeftTimer.Stop();
            player.GetCharacter().IsOnGarbageRun = false;
            player.GetCharacter().update_ped();
            player.SendChatMessage("~r~The garbage run has ended. Your garbage truck was removed.");
            API.TriggerClientEvent(player, "garbage_removewaypoint");
        }

        public static void SendNotificationToGarbagemen(string message)
        {
            foreach(var player in PlayerManager.Players)
            {
                if (player?.JobOne?.Type == JobManager.JobTypes.Garbageman)
                {
                    API.Shared.SendPictureNotificationToPlayer(player.Client, message, "CHAR_PROPERTY_CAR_SCRAP_YARD", 
                        0, 1, "Los Santos Sanitations", "Garbage Notification");
                }
            }
        }


        //Commands

        [Command("unloadtrash"), Help(HelpManager.CommandGroups.GarbageJob, "Unloads your truck's garabge and get your payment!")]
        public void unloadtrash_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.SendChatMessage("You must be a garbageman to use this command!");
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.SendChatMessage("You must be on a garbage run to unload trash.");
                return;
            }

            if (player.Position.DistanceTo(character.JobOne.MiscOne.Location) > 10)
            {
                player.SendChatMessage("You must be at the trash depot to unload the trash.");
                return;
            }

            vehicle_manager.Vehicle closestVeh = VehicleManager.GetClosestVehicle(player, 10f).GetVehicle();

            if (closestVeh == null || closestVeh.Job.Type != JobManager.JobTypes.Garbageman)
            {
                player.SendChatMessage("~r~You must be at the back of your garbage truck to unload the trash.");
                return;
            }

            if (player.Rotation.Z > API.GetEntityRotation(closestVeh.NetHandle).Z + 20 || player.Rotation.Z < API.GetEntityRotation(closestVeh.NetHandle).Z - 20)
            {
                player.SendChatMessage("~r~You must be at the back of your garbage truck to unload the trash.");
                return;
            }

            if (closestVeh.GarbageBags == 0)
            {
                player.SendChatMessage("~r~Your garbage truck has no trash to unload!");
                return;
            }

            LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {character.CharacterName}[{player.GetAccount().AccountName}] has earned ${closestVeh.GarbageBags * 100} from a garbage run.");
            InventoryManager.GiveInventoryItem(character, new Money(), closestVeh.GarbageBags * 100);
            ChatManager.RoleplayMessage(character, "uses the garbage truck's control panel to unload the trash.", ChatManager.RoleplayMe);
            player.SendChatMessage($"You were paid ${closestVeh.GarbageBags * 100} for unloading {closestVeh.GarbageBags} trash bags.");
            closestVeh.GarbageBags = 0;
            closestVeh.UpdateMarkers();
            API.Shared.SendPictureNotificationToPlayer(player, $"Thanks for keeping Los Santos clean!", "CHAR_PROPERTY_CAR_SCRAP_YARD", 0, 1, "Los Santos Sanitations", "Garbage Notification"); 
        }

        [Command("endtrash"), Help(HelpManager.CommandGroups.GarbageJob, "Ends your current trash job.")]
        public void endtrash_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.SendChatMessage("You must be a garbageman to use this command!");
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.SendChatMessage("You aren't on a garbage run!");
                return;
            }

            var veh = VehicleManager.GetVehFromNetHandle(API.GetPlayerVehicle(player));

            if (veh?.Job?.Type != JobManager.JobTypes.Garbageman)
            {
                player.SendChatMessage("You must be in your garbage truck to end the garbage run.");
                return;
            }

            character.IsOnGarbageRun = false;
            RespawnGarbageTruck(player, veh);
            character.update_ped();

        }

        [Command("pickuptrash"), Help(HelpManager.CommandGroups.GarbageJob, "Picks up trash from a garbage bin.")]
        public void pickuptrash_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                player.SendChatMessage("You must be a garbageman to use this command!");
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.SendChatMessage("You must be on a garbage run to pick up trash.");
                return;
            }

            var prop = PropertyManager.IsAtPropertyGarbagePoint(player);
            if (prop == null)
            {
                API.SendChatMessageToPlayer(player, "You aren't at a garbage point.");
                return;
            }

            if (API.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You cannot do this while in a vehicle.");
                return;
            }
            if (prop.GarbageBags == 0)
            {
                player.SendChatMessage("There is no garbage to pick up.");
                return;
            }

            if (character.GarbageBag != null)
            {
                player.SendChatMessage("You are already carrying a garbage bag.");
                return;
            }

            prop.GarbageBags -= 1;
            prop.UpdateMarkers();
            character.GarbageBag = API.CreateObject(API.GetHashKey("hei_prop_heist_binbag"), player.Position, new Vector3());
            API.AttachEntityToEntity(character.GarbageBag, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(360, 0, 0));
            API.TriggerClientEvent(player, "garbage_holdbag");
            ChatManager.RoleplayMessage(character, "reaches into the trash and pulls out a garbage bag.", ChatManager.RoleplayMe);
            player.SendChatMessage("You are holding a garbage bag. Press LMB to throw it into the back of the garbage truck");
        }

        public void pickuptrash_e(Client player)
        {
            Character character = player.GetCharacter();

            if (character.JobOne.Type != JobManager.JobTypes.Garbageman)
            {
                return;
            }

            var prop = PropertyManager.IsAtPropertyGarbagePoint(player);
            if (prop == null)
            {
                return;
            }

            if (!character.IsOnGarbageRun)
            {
                player.SendChatMessage("You must be on a garbage run to pick up trash.");
                return;
            }

            if (API.IsPlayerInAnyVehicle(player))
            {
                player.SendChatMessage("You cannot do this while in a vehicle.");
                return;
            }
            if (prop.GarbageBags == 0)
            {
                player.SendChatMessage("There is no garbage to pick up.");
                return;
            }

            if (character.GarbageBag != null)
            {
                player.SendChatMessage("You are already carrying a garbage bag.");
                return;
            }

            prop.GarbageBags -= 1;
            prop.UpdateMarkers();
            character.GarbageBag = API.CreateObject(API.GetHashKey("hei_prop_heist_binbag"), player.Position, new Vector3());
            API.AttachEntityToEntity(character.GarbageBag, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(360, 0, 0));
            API.TriggerClientEvent(player, "garbage_holdbag");
            ChatManager.RoleplayMessage(character, "reaches into the trash and pulls out a garbage bag.", ChatManager.RoleplayMe);
            player.SendChatMessage("You are holding a garbage bag. Press LMB to throw it into the back of the garbage truck");
        }
    }
}
