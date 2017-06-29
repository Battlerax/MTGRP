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
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {

        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {

        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player, "Character");
            vehicle_manager.Vehicle playerveh = API.getEntityData(vehicle, "Vehicle");

            if (playerveh.Job.Type != JobManager.JobTypes.Garbageman)
            {
                player.sendChatMessage("You must be a garbageman to use this vehicle.");
                API.warpPlayerOutOfVehicle(player);
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
            player.sendChatMessage("A garbage waypoint has been set on your map.");

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
    }
}
