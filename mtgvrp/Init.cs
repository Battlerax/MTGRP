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


using GTANetworkServer;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;

namespace mtgvrp
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
            //For Dealership.
            API.removeIpl("fakeint"); // remove the IPL "fakeint"
            API.requestIpl("shr_int"); // Request the IPL "shr_int"
            API.consoleOutput("[INIT] Unloaded fakeint IPL and loaded shr_int IPL.!");

            VehicleManager.load_all_unowned_vehicles();
            API.consoleOutput("[INIT] Script initalized!");
        }
    }
}
