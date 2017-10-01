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
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core;
using mtgvrp.core.Discord;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.job_manager.gunrunner;

namespace mtgvrp
{
    public class Init : Script
    {
        public static string SERVER_NAME = "[EN][TEST] Arcadit Roleplay";
        public static string SERVER_VERSION = "v0.0.1682";
        public static string SERVER_WEBSITE = "www.arcadit.com";
        public static Random Random = new Random();

        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.setServerName(SERVER_NAME + " ~b~| ~g~" + SERVER_WEBSITE);
            API.setGamemodeName("Arcadit V-RP " + SERVER_VERSION);

            API.onResourceStart += OnResourceStartHandler;
            API.onResourceStop += API_onResourceStop;
            API.onClientEventTrigger += API_onClientEventTrigger;
            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            SettingsManager.Load();

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        public delegate void OnPlayerEnterVehicleExHandler(Client player, NetHandle vehicle, int seat);
        public static event OnPlayerEnterVehicleExHandler OnPlayerEnterVehicleEx;

        private void API_onClientEventTrigger(GrandTheftMultiplayer.Server.Elements.Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "OnPlayerEnterVehicleEx")
            {
                NetHandle veh = (NetHandle) arguments[0];
                int seat = (int) arguments[1];

                OnPlayerEnterVehicleEx?.Invoke(sender, veh, seat);
            }

            else if (eventName == "OBJECT_PLACED_PROPERLY")
            {
                NetHandle obj = (NetHandle) arguments[0];
                Vector3 pos = (Vector3) arguments[1];
                Vector3 rot = (Vector3) arguments[2];
                API.setEntityPosition(obj,pos);
                API.setEntityRotation(obj,rot);
            }

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

            //Gunrunning
            GunrunnerManager.load_all_containers();
            GunrunnerManager.load_all_container_zones();
            GunrunnerManager.MoveDealer();

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
