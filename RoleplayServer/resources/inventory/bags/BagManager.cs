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

        [Command("managebag")]
        public void Managebag(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            BagItem[] bag = (BagItem[])InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag right now.");
                return;
            }

            //Get the current bag items.
            string[][] bagItems = bag[0].Inventory.Select(x => new [] {x.Id.ToString(), x.LongName}).ToArray();
            string[][] invItems = character.Inventory.Select(x => new[] { x.Id.ToString(), x.LongName }).ToArray();
            API.triggerClientEvent(player, "bag_showmanager", API.toJson(invItems), API.toJson(bagItems));
        }

        //TODO: test cmd.
        [Command("givemebag")]
        public void GiveMeBag(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(character, new BagItem(), 1, true).ToString());
        }
    }
}
