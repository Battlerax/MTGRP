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
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;

namespace RoleplayServer.resources
{
    public class Init : Script
    { 
        public Init()
        {

            DebugManager.DebugMessage("[INIT] Initalizing script...");

            API.onResourceStart += OnResourceStartHandler;

            DebugManager.DebugManagerInit();
            DatabaseManager.DatabaseManagerInit();
        }

        public void OnResourceStartHandler()
        {
            API.consoleOutput("[INIT] Script initalized!");
        }
    }
}
