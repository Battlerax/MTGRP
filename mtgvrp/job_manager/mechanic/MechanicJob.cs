using System;
using System.Collections;
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
            var veh = VehicleManager.GetClosestVehicle(player, 10);

            /*
            if (character.JobOne.Type != JobManager.MechanicJob)
            {
                API.sendPictureNotificationToPlayer(player, "You must be a mechanic to use this command.", "CHAR_BLOCKED", 0, 0, "Server", "~r~Command Error");
                return;
            }
            */

            if (veh == new NetHandle())
            {
                API.sendChatMessageToPlayer(player, "You are too far away from a vehicle.");
                return;
            }

            if (!IsPlayerInFrontOfVehicle(API.getEntityPosition(veh), API.getEntityPosition(veh).Z, 5))
            {
                API.sendChatMessageToPlayer(player, "You must be infront of the vehicle to fix it.");
                return;
            }

            if (character.FixcarPrevention)
            {
                API.sendChatMessageToPlayer(player, "You must wait 2 minutes before fixing another car.");
                return;
            }

            /*
            if (InventoryManager.DoesInventoryHaveItem(character, typeof(EngineParts)).Length == 0)
            {
                player.sendChatMessage("You don't have enough engine parts.");
            }
            */

            API.setVehicleHealth(veh, 1000);
            //InventoryManager.DeleteInventoryItem(character, typeof(EngineParts), 1);
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

        public static bool IsPlayerInFrontOfVehicle(Vector3 pos, float angle, float distance)
        {
            double X = pos.X;
            double Y = pos.Y;
            double Z = pos.Z;
            //Get The rotation
            double rotation = angle;

            //new Position Calc
            rotation = (rotation - 90) * (Math.PI / 180);
            X = pos.X - (distance * Math.Sin(rotation));
            Y = pos.Y - (distance * Math.Cos(rotation));
            Z += 0.5;

            if (pos.DistanceTo(new Vector3(X, Y, Z)) < 2)
            {
                API.shared.createMarker(1, new Vector3(X,Y,Z), new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 100, 51, 153, 255);
                return true;
            }
            API.shared.createMarker(1, new Vector3(X,Y,Z), new Vector3(), new Vector3(), new Vector3(1f, 1f, 1f), 100, 51, 153, 255);
            return false;

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
