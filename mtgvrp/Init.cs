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
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.inventory;
using mtgvrp.job_manager.hunting;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;

namespace mtgvrp
{
    public class Init : Script
    {
        public static string SERVER_NAME = "[EN] MT-Gaming V-RP Test Server";
        public static string SERVER_VERSION = "v0.0.779";
        public static string SERVER_WEBSITE = "www.mt-gaming.com";
        public static Random Random = new Random();

        [Command("underwater")]
        public void SetMyWater(Client player, float duration)
        {
            API.sendNativeToPlayer(player, Hash.SET_PED_MAX_TIME_UNDERWATER, player.handle, duration);
        }

        [Command("getunder")]
        public void GetMyWater(Client player)
        {
            API.sendChatMessageToPlayer(player, "Amount: " + API.fetchNativeFromPlayer<float>(player, Hash.GET_PLAYER_UNDERWATER_TIME_REMAINING, player.handle));
        }

        [Command("scuba")]
        public void Scuba(Client player, bool state)
        {
            API.sendNativeToPlayer(player, Hash.SET_ENABLE_SCUBA, player.handle, state);
        }

        [Command("scubakit")]
        public void ScubaKit(Client player)
        {

            var head = API.createObject(239157435, player.position, new Vector3());
            API.attachEntityToEntity(head, player, "SKEL_Head", new Vector3(0, 0, 0), new Vector3(180, 90, 0));

            var back = API.createObject(1593773001, player.position, new Vector3());
            API.attachEntityToEntity(back, player, "SKEL_Spine3", new Vector3(-0.3, -0.23, 0), new Vector3(180, 90, 0));

        }

        [Command("getobjs")]
        public void GetAmountOfText(Client player, int id)
        {
            API.sendChatMessageToPlayer(player, "Amount: " + API.fetchNativeFromPlayer<int>(player, Hash.GET_NUMBER_OF_PED_DRAWABLE_VARIATIONS, player.handle, id));
        }

        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.setServerName(SERVER_NAME + " ~r~[" + SERVER_VERSION + "] ~b~| ~g~" + SERVER_WEBSITE);

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

        [Command("unfreezeanimals")]
        public void unfreezeanimals(Client player)
        {
            foreach (var a in HuntingManager.SpawnedAnimals)
            {
                API.setEntityPositionFrozen(a.handle, false);
            }
        }
    }
}
