using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory.bags
{
    class BagManager : Script
    {

        [Command("managebag")]
        public void Managebag(Client player)
        {
            Character character = player.GetCharacter();
            IInventoryItem[] bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag.");
                return;
            }

            //Show the window.
            InventoryManager.ShowInventoryManager(player, character, (BagItem)bag[0]);
        }

        //TODO: test cmd.
        [Command("givemebag")]
        public void GiveMeBag(Client player)
        {
            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(player.GetCharacter(), new BagItem(), 1, true).ToString());
        }
    }
}
