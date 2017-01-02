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

namespace RoleplayServer
{
    public class Init : Script
    { 
        public Init()
        {

            DebugManager.debugMessage("[INIT] Initalizing script...");

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
