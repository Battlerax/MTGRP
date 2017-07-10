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
        public static string SERVER_NAME = "[EN] MT-Gaming V-RP Closed Beta";
        public static string SERVER_VERSION = "v0.0.1064";
        public static string SERVER_WEBSITE = "www.mt-gaming.com";
        public static Random Random = new Random();

        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.setServerName(SERVER_NAME + " ~r~[" + SERVER_VERSION + "] ~b~| ~g~" + SERVER_WEBSITE);

            API.onResourceStart += OnResourceStartHandler;
            API.onResourceStop += API_onResourceStop;
            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            SettingsManager.Load();

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

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public void OnResourceStartHandler()
        {
            //For Dealership.
            API.removeIpl("fakeint"); // remove the IPL "fakeint"
            API.requestIpl("shr_int"); // Request the IPL "shr_int"
            API.consoleOutput("[INIT] Unloaded fakeint IPL and loaded shr_int IPL.!");

            VehicleManager.load_all_unowned_vehicles();
            API.consoleOutput("[INIT] Script initalized!");

            LogManager.StartLogArchiveTimer();

            //Must be last to be called.
            if (IsRunningOnMono())
            {
                API.consoleOutput("[INIT] Starting Discord Bot!");
                DiscordManager.StartBot();
            }
        }

        private void API_onResourceStop()
        {
            SettingsManager.Save();
        }
    }
}
