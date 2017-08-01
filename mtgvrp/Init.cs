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
using GrandTheftMultiplayer.Shared;
using mtgvrp.core;
using mtgvrp.core.Discord;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;

namespace mtgvrp
{
    public class Init : Script
    {
        public static string SERVER_NAME = "[EN] Moving Target Gaming Roleplay";
        public static string SERVER_VERSION = "v0.0.1457";
        public static string SERVER_WEBSITE = "www.mt-gaming.com";
        public static Random Random = new Random();

        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.setServerName(SERVER_NAME + " ~r~[" + SERVER_VERSION + "] ~b~| ~g~" + SERVER_WEBSITE);

            API.onResourceStart += OnResourceStartHandler;
            API.onResourceStop += API_onResourceStop;
            API.onClientEventTrigger += API_onClientEventTrigger;
            InventoryManager.OnStorageItemUpdateAmount += InventoryManager_OnStorageItemUpdateAmount;

            SettingsManager.Load();

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        /*public class OnPlayerEnterVehicleExEventArgs : EventArgs
        {
            public OnPlayerEnterVehicleExEventArgs(Client player, NetHandle vehicle, int seat)
            {
                Vehicle = vehicle;
                Player = player;
                Seat = seat;
            }

            public NetHandle Vehicle { get; }

            public Client Player { get; }

            public int Seat { get; }
        }*/
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
            else if (eventName == "PLAYER_STREAMED_IN")
            {
                var playerNet = (NetHandle) arguments[0];
                var playerClient = (Client) API.getPlayerFromHandle(playerNet);
                if (playerClient == null)
                    return;
                var playerChar = playerClient.GetCharacter();
                if(playerChar == null)
                    return;
                playerChar.update_ped(sender);
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
            API.requestIpl("ex_dt1_02_office_02b"); //Office.
            API.requestIpl("ex_dt1_02_office_02c"); //Office.
            API.requestIpl("ex_dt1_02_office_02a"); //Office.
            API.requestIpl("ex_dt1_02_office_01a"); //Office.
            API.requestIpl("ex_dt1_02_office_01b"); //Office.
            API.requestIpl("ex_dt1_02_office_01c"); //Office.
            API.requestIpl("ex_dt1_02_office_03b"); //Office.
            API.requestIpl("ex_dt1_02_office_03a"); //Office.
            API.requestIpl("ex_dt1_02_office_03c"); //Office.

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
