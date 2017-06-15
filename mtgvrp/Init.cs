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
    }
}
