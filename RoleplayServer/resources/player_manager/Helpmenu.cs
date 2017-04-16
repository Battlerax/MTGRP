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
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.group_manager;
using RoleplayServer.resources.job_manager;
using RoleplayServer.resources.AdminSystem;

namespace RoleplayServer.resources.player_manager
{
    public class Helpmenu : Script
    {
        public void HelpMenu()
        {
            API.onClientEventTrigger += onClientEvent;
        }
        [Command("help")]
        public void help_cmd(Client player)

        {
        }

        public void onClientEvent(Client player, string id, params object[] arguments)
        {
            if (id == "Commands1")
            {
                player.sendChatMessage("~h~Here is the list of commands availible to you:");
                player.sendChatMessage("/time, /stats");
            }
        }
    }
}
        /*
        public void OnClientEvent(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character character = API.getEntityData(player.handle, "Character");

            if (id == "clickeditem")
            {
                if ((string)args[0] == "Commands1")
                {
                    player.sendChatMessage("~h~Here is the list of commands availible to you:");
                    player.sendChatMessage("/time, /stats");
                }
            }

            if (id == "clickeditem")
            {
                if ((string)args[0] == "Police1")
                {
                    if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLspd)
                    {
                        API.sendChatMessageToPlayer(player, "You must be in the LSPD to use this command.");
                    }

                    else
                    {
                        player.sendChatMessage("~h~Here is the list of commands availible to you:");
                        player.sendChatMessage("");
                    }
                }
            }

            if (id == "clickeditem")
            {
                if ((string)args[0] == "Admin1")
                {
                    Account account = API.shared.getEntityData(player.handle, "Account");
                    if (account.AdminLevel == 0)
                    {
                        player.sendChatMessage("~h~Here is the list of commands availible to you:");
                        player.sendChatMessage("/spec, /agiveweapon, /goto, /gotopos, /sethealth, /setarmour, /slap, /spawnveh 'name'");
                    }

                    else
                    {
                        player.sendChatMessage("You're not an admin, if you'd like to apply for admin head to our forums.");
                        player.sendChatMessage("MT-gaming.com");
                    }
                }
            }

            if (id == "clickeditem")
            {
                if ((string)args[0] == "Rules1")
                {
                    player.sendChatMessage("To view all the rules head to ~h~MT-Gaming.com~h~ Here some basic server rules:");
                    player.sendChatMessage("IC = In character || OOC = Out of character");
                    player.sendChatMessage("MG = Metagaming - Using out of character info in character | Using in character info out of character.");
                    player.sendChatMessage("DM = Death matching - killing another player without any roleplay reason or any roleplay at all.");
                    player.sendChatMessage("PG = Powergaming - Roleplaying the impossible | forcing a player to roleplay situation without giving them a chance");
                }
            }

            if (id == "clickeditem")
            {
                if ((string)args[0] == "FAQ")//Need to set another menu to pop up once this is selected to see more
                {
                    player.sendChatMessage("~h~This still needs to be set up - Toro");
                }
            }
        }
    }
}
*/