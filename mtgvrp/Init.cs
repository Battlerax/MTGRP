/*
 *  File: Init.cs
 *  Author: Chenko
 *  Date: 12/24/2016
 * 
 * 
 *  Purpose: Initalizes the server
 * 
 * 
 * */


using System;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using RoleplayServer.core;
using RoleplayServer.database_manager;
using RoleplayServer.inventory;
using RoleplayServer.player_manager;
using RoleplayServer.vehicle_manager;

namespace RoleplayServer
{
    public class Init : Script
    { 
        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.onResourceStart += OnResourceStartHandler;
            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        private void InventoryManager_OnStorageItemUpdateAmount(IStorage sender,
            InventoryManager.OnItemAmountUpdatedEventArgs args)
        {
            if (sender.GetType() == typeof(Character) && args.Item == typeof(Money))
            {
                Character c = (Character) sender;
                API.shared.triggerClientEvent(c.Client, "update_money_display", args.Amount);
            }
        }

        public void OnResourceStartHandler()
        {
            VehicleManager.load_all_unowned_vehicles();
            API.consoleOutput("[INIT] Script initalized!");
        }

        private Marker targetmrk = null;
        private Marker playermrk = null;
        private Marker vehmark = null;

        [Command("near")]
        public void amiinfrontofvehicle(Client player, int angtoadd)
        {
            vehicle_manager.Vehicle vh = VehicleManager.Vehicles.First() ?? null;

            if (vh == null)
            {
                API.sendChatMessageToPlayer(player, "Not near vehicle.");
                return;
            }

            var playerpos = API.getEntityPosition(player);
            var vehiclepos = API.getEntityPosition(vh.NetHandle);
            var distance = playerpos.DistanceTo(vehiclepos);

            if (targetmrk != null)
                API.deleteEntity(targetmrk);
            if (playermrk != null)
                API.deleteEntity(playermrk);
            if (vehmark != null)
                API.deleteEntity(vehmark);

            playermrk = API.createMarker(1, playerpos, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 0, 225, 0, 0);
            vehmark = API.createMarker(1, vehiclepos, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 0, 0, 225, 0);

            if (distance < 6.0)
            {
                var angle = API.getEntityRotation(vh.NetHandle).Z;
                    angle += angtoadd;

                var newPos = new Vector3(vehiclepos.X + (float)(distance * Math.Sin(-angle)), vehiclepos.Y + (float)(distance * Math.Cos(-angle)), vehiclepos.Z);

                targetmrk = API.createMarker(1, newPos, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 0, 0, 0);

                distance = playerpos.DistanceTo(newPos);

                if (distance < 1.0)
                {
                    API.sendChatMessageToPlayer(player, "OHH YEAH");
                    return;
                }
            }
            API.sendChatMessageToPlayer(player, "NOPE");
        }
    }
}
