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
using GTANetworkAPI;
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
        public static string SERVER_NAME = "[EN][TEST] MT-Gaming Roleplay";
        public static string SERVER_VERSION = "v0.0.1682";
        public static string SERVER_WEBSITE = "www.mt-gaming.com";
        public static Random Random = new Random();

        public Init()
        {
            DebugManager.DebugMessage("[INIT] Initalizing script...");

            //API.SetServerName(SERVER_NAME + " ~b~| ~g~" + SERVER_WEBSITE);

            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            SettingsManager.Load();

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        [RemoteEvent("OBJECT_PLACED_PROPERLY")]
        public void ObjectPlacedProperly(Client sender, params object[] arguments)
        {
            Entity obj = (Entity) arguments[0];
            Vector3 pos = (Vector3) arguments[1];
            Vector3 rot = (Vector3) arguments[2];
            NAPI.Entity.SetEntityPosition(obj,pos);
            NAPI.Entity.SetEntityRotation(obj,rot);
        }

        private void InventoryManager_OnStorageItemUpdateAmount(IStorage sender,
            InventoryManager.OnItemAmountUpdatedEventArgs args)
        {
            if (sender.GetType() == typeof(Character) && args.Item == typeof(Money))
            {
                Character c = (Character) sender;
                API.Shared.TriggerClientEvent(c.Client, "update_money_display", args.Amount);
            }
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            //For Dealership.
            NAPI.World.RemoveIpl("fakeint"); // remove the IPL "fakeint"
            NAPI.World.RequestIpl("shr_int"); // Request the IPL "shr_int"

            NAPI.Util.ConsoleOutput("[INIT] Unloaded fakeint IPL and loaded shr_int IPL.!");

            VehicleManager.load_all_unowned_vehicles();

            //Gunrunning
            GunrunnerManager.load_all_containers();
            GunrunnerManager.load_all_container_zones();
            GunrunnerManager.MoveDealer();

            NAPI.Util.ConsoleOutput("[INIT] Script initalized!");

            LogManager.StartLogArchiveTimer();

            //Must be last to be called.
            /*if (IsRunningOnMono())
            {
                NAPI.Util.ConsoleOutput("[INIT] Starting Discord Bot!");
                DiscordManager.StartBot();
            }*/
        }

        [ServerEvent(Event.ResourceStop)]
        public void API_onResourceStop()
        {
            SettingsManager.Save();
        }
    }
}
