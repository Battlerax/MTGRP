/*
 Help menu
 Created: 1/04/2017
 Author: Toro
 Todo: 
 - Add commands when they added to the server
 - Set up another menu within the menu for FAQ

 */

using GTANetworkServer;

namespace mtgvrp.player_manager
{
    public class OpenHelpmenu : Script
    {
        public void OpenHelpMenu()
        {
            API.onClientEventTrigger += OnClientEvent;
        }
        public void OnClientEvent(Client player, string id, params object[] arguments)
        {
        }
        [Command("help")]
        public void Helpmenu(Client player)
        {
            API.triggerClientEvent(player, "openHelpMenu", "Help_Menu", "Select an option");
        }
    } 
}