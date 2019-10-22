using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;

namespace mtgvrp.job_manager.delivery
{
    public class DeliveryManager : Script
    {
        [Command("getsupplies"), Help(HelpManager.CommandGroups.DeliveryJob, "Used to get supplies which you can later sell at a business.", new []{"The amount of supplies you'd like to buy."})]
        public void GetSupplies(Client player, int amount)
        {
            Character c = player.GetCharacter();
            if (c.JobOne?.Type != JobManager.JobTypes.DeliveryMan)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be a delivery man.");
                return;
            }

            if (JobManager.GetJobById(c.JobZone)?.Type != JobManager.JobTypes.DeliveryMan || c.JobZoneType != 2)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at the /getsupplies spot.");
                return;
            }

            switch (InventoryManager.GiveInventoryItem(c, new SupplyItem(), amount))
            {
                case InventoryManager.GiveItemErrors.MaxAmountReached:
                    NAPI.Chat.SendChatMessageToPlayer(player, "You have reached the max amount of that item.");
                    break;
                case InventoryManager.GiveItemErrors.NotEnoughSpace:
                    NAPI.Chat.SendChatMessageToPlayer(player, "You don't have enough space in your inventory.");
                    break;
                case InventoryManager.GiveItemErrors.Success:
                    NAPI.Chat.SendChatMessageToPlayer(player, $"You have sucessfully bought {amount} Supplies.");
                    break;
            }
        }

        [Command("sellsupplies"), Help(HelpManager.CommandGroups.DeliveryJob, "Sell supplies to a business.", new []{"The amount of supplies you'd like to sell."})]
        public void SellSupplies(Client player, int amount)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You aren't at an interaction point or entrance.");
                return;
            }

            if (prop.Type == PropertyManager.PropertyTypes.Bank ||
                prop.Type == PropertyManager.PropertyTypes.Advertising ||
                prop.Type == PropertyManager.PropertyTypes.Housing ||
                prop.Type == PropertyManager.PropertyTypes.LSNN || 
                prop.Type == PropertyManager.PropertyTypes.VIPLounge || prop.DoesAcceptSupplies == false
            )
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "This business doesnt buy supplies.");
                return;
            }

            if (InventoryManager.GetItemCount<SupplyItem>(player.GetCharacter()) < amount || amount <= 0)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You don't have that amount of supplies.");
                return;
            }

            if (Money.GetCharacterMoney(prop) < amount * prop.SupplyPrice)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "The business doesn't have enough money.");
                return;
            }

            prop.Supplies += amount;
            InventoryManager.DeleteInventoryItem<SupplyItem>(player.GetCharacter(), amount);
            InventoryManager.DeleteInventoryItem<Money>(prop, amount * prop.SupplyPrice);
            InventoryManager.GiveInventoryItem(player.GetCharacter(), new Money(), amount * prop.SupplyPrice, true);
            LogManager.Log(LogManager.LogTypes.Stats, $"[Job] {player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}] has earned ${amount * prop.SupplyPrice} from delivering supplies. (Prop: {prop.Id}.");
            NAPI.Chat.SendChatMessageToPlayer(player, $"You've successfully sold {amount} supplies to the property for a total of ${amount * prop.SupplyPrice}");
        }
    }
}
