using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;

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

        [Command("sellsupplies")]
        public void SellSupplies(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.Type == PropertyManager.PropertyTypes.Bank ||
                prop.Type == PropertyManager.PropertyTypes.Advertising ||
                prop.Type == PropertyManager.PropertyTypes.Housing ||
                prop.Type == PropertyManager.PropertyTypes.LSNN || prop.DoesAcceptSupplies == false
            )
            {
                API.sendChatMessageToPlayer(player, "This business doesnt buy supplies.");
                return;
            }

            if (InventoryManager.GetItemCount<SupplyItem>(player.GetCharacter()) < amount || amount <= 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount of supplies.");
                return;
            }

            if (Money.GetCharacterMoney(prop) < amount * prop.SupplyPrice)
            {
                API.sendChatMessageToPlayer(player, "The business doesn't have enough money.");
                return;
            }

            prop.Supplies += amount;
            InventoryManager.DeleteInventoryItem<SupplyItem>(player.GetCharacter(), amount);
            InventoryManager.DeleteInventoryItem<Money>(player.GetCharacter(), amount * prop.SupplyPrice);
            InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), amount * prop.SupplyPrice, true);

            API.sendChatMessageToPlayer(player, $"You've successfully sold {amount} supplies to the property for a total of ${amount * prop.SupplyPrice}");
        }
    }
}
