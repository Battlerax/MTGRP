using System;
using System.Collections.Generic;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.core;

namespace RoleplayServer.resources.job_manager.taxi
{
    public class MechanicJob : Script
    {

        public MechanicJob()
        {
            API.onPlayerEnterVehicle += API_onPlayerEnterVehicle;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
        }

        [Command("fixcar")]
        public void fixcar_cmd(Client player)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(API.getPlayerVehicle(player));

            if (character.JobOne.Type != JobManager.MechanicJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a mechanic to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem(character, typeof(EngineParts)).Length == 0)
            {
                player.sendChatMessage("You don't have enough engine parts.");
            }

            API.setVehicleHealth(API.getPlayerVehicle(player), 1000);
            InventoryManager.DeleteInventoryItem(character, typeof(EngineParts), 1);
            player.sendChatMessage("Vehicle repaired.");
        }

        [Command("paintcar")]
        public void paintcar_cmd(Client player, string col1, string col2)
        {
            Character character = player.GetCharacter();
            var veh = VehicleManager.GetVehFromNetHandle(API.getPlayerVehicle(player));

            if (character.JobOne.Type != JobManager.MechanicJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a mechanic to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem(character, typeof(SprayPaint)).Length == 0)
            {
                player.sendChatMessage("You don't have spray paint.");
            }

            API.setVehiclePrimaryColor(API.getPlayerVehicle(player), int.Parse(col1));
            API.setVehicleSecondaryColor(API.getPlayerVehicle(player), int.Parse(col2));
            veh.SpawnColors[0] = int.Parse(col1);
            veh.SpawnColors[1] = int.Parse(col2);
            veh.Save();
            InventoryManager.DeleteInventoryItem(character, typeof(SprayPaint), 1);
            player.sendChatMessage("Vehicle painted.");
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player.handle, "Character");
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

        }

        private void API_onPlayerEnterVehicle(Client player, NetHandle vehicle)
        {
            Character character = API.getEntityData(player.handle, "Character");
            var veh = VehicleManager.GetVehFromNetHandle(vehicle);

        }
    }
}
