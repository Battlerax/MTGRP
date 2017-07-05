using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;

namespace mtgvrp.job_manager.delivery
{
    public class DeliveryManager : Script
    {
        [Command("getsupplies")]
        public void GetSupplies(Client player, int amount)
        {
            Character c = player.GetCharacter();
            if (c.JobOne?.Type != JobManager.JobTypes.DeliveryMan)
            {
                API.sendChatMessageToPlayer(player, "You must be a delivery man.");
                return;
            }

            if (JobManager.GetJobById(c.JobZone)?.Type != JobManager.JobTypes.Trucker || c.JobZoneType != 3)
            {
                API.sendChatMessageToPlayer(player, "You aren't at the /getsupplies spot.");
                return;
            }

            switch (InventoryManager.GiveInventoryItem(c, new SupplyItem(), amount))
            {
                case InventoryManager.GiveItemErrors.HasBlockingItem:
                    API.sendChatMessageToPlayer(player, "You have a blocking item.");
                    break;
                case InventoryManager.GiveItemErrors.MaxAmountReached:
                    API.sendChatMessageToPlayer(player, "You have reached the max amount of that item.");
                    break;
                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                    API.sendChatMessageToPlayer(player, "You don't have enough space in your inventory.");
                    break;
                case InventoryManager.GiveItemErrors.Success:
                    API.sendChatMessageToPlayer(player, $"You have sucessfully bought {amount} Supplies.");
                    break;
            }
        }
    }
}
