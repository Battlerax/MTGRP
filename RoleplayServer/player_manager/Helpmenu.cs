/*
 Help menu
 Created: 1/04/2017
 Author: Toro
 Todo: 
 - Add commands when they added to the server
 - Set up another menu within the menu for FAQ

 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.database_manager;
using RoleplayServer.group_manager;
using RoleplayServer.job_manager;
using RoleplayServer.AdminSystem;

namespace RoleplayServer.player_manager
{
    public class openHelpmenu : Script
    {
        public void openHelpMenu()
        {
            API.onClientEventTrigger += onClientEvent;
        }
        public void onClientEvent(Client player, string id, params object[] arguments)
        {
        }
        [Command("help")]
        public void Helpmenu(Client player)
        {
            API.triggerClientEvent(player, "openHelpMenu", "Help_Menu", "Select an option");
        }
    } 
}