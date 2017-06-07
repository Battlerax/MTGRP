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

            if (character.FixcarPrevention)
            {
                API.sendChatMessageToPlayer(player, "You must wait 2 minutes before fixing another car.");
                return;
            }

            if (InventoryManager.DoesInventoryHaveItem(character, typeof(EngineParts)).Length == 0)
            {
                player.sendChatMessage("You don't have enough engine parts.");
            }

            API.setVehicleHealth(API.getPlayerVehicle(player), 1000);
            InventoryManager.DeleteInventoryItem(character, typeof(EngineParts), 1);
            player.sendChatMessage("Vehicle repaired.");
            character.FixcarPrevention = true;
            character.FixcarTimer = new Timer { Interval = 120000 };
            character.FixcarTimer.Elapsed += delegate { FixCarReset(player); };
            character.FixcarTimer.Start();
        }

        public void FixCarReset(Client player)
        {
            Character character = player.GetCharacter();

            character.FixcarPrevention = false;
            player.sendChatMessage("You can now fix another car.");
            character.FixcarTimer.Stop();
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
