using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory.bags
{
    class BagManager : Script
    {
        public BagManager()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        [Command("putinbag")]
        public void PutInBag(Client player, string itemname, int amount)
        {
            Character character = API.getEntityData(player, "Character");
            var bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag == null)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag right now.");
                return;
            }

            //Parse for that item.
            Type itemType = InventoryManager.ParseInventoryItem(itemname);
            if (itemType == null)
            {
                API.sendNotificationToPlayer(player, "That item doesn't exist.");
                return;
            }
            var item = InventoryManager.DoesInventoryHaveItem(character, itemType);
            if (item == null)
            {
                API.sendNotificationToPlayer(player, "You don't have that item.");
                return;
            }

            //Check of has enough amount.
            if (item.Amount >= amount)
            {
                //Add to bag first.

            }
            else
            {
                API.sendNotificationToPlayer(player, "You don't have that amount.");
            }
        }

        //TODO: test cmd.
        [Command("givemebag")]
        public void GiveMeBag(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(character, new BagItem()).ToString());
        }
    }
}
