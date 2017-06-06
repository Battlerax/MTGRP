using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.core;
using RoleplayServer.resources.inventory;
using RoleplayServer.resources.phone_manager;

namespace RoleplayServer.resources.property_system.businesses
{
    class GeneralBuying : Script
    {
        public GeneralBuying()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "property_exitbuy")
            {
                API.freezePlayer(sender, false);
            }
            else if (eventName == "property_buyitem")
            {
                var prop = PropertyManager.IsAtPropertyInteraction(sender);
                if (prop == null)
                {
                    API.sendChatMessageToPlayer(sender, "You aren't at a property or you moved away.");
                    return;
                }

                var itemName = (string) arguments[0];

                string name = "";
                int price = prop.ItemPrices.SingleOrDefault(x => x.Key == itemName).Value;

                //Make sure has enough money.
                if (sender.GetCharacter().Money < price)
                {
                    API.sendChatMessageToPlayer(sender, "Not Enough Money");
                    return;
                }

                IInventoryItem item = null;
                if (prop.Type == PropertyManager.PropertyTypes.TwentyFourSeven)
                {
                    name = ItemManager.TwentyFourSevenItems.Single(x => x[0] == itemName)[1];
                    switch (itemName)
                    {
                        case "phone":
                            var number = PhoneManager.GetNewNumber();
                            var phone = new Phone()
                            {
                                Number = number,
                                PhoneName = "default"
                            };
                            item = phone;
                            break;
                    }
                }

                if (item == null)
                {
                    API.sendChatMessageToPlayer(sender,
                        "Error finding the item you bought, report this as a bug report.");
                    return;
                }

                //Send message.
                switch (InventoryManager.GiveInventoryItem(sender.GetCharacter(), item))
                {
                    case InventoryManager.GiveItemErrors.Success:
                        sender.GetCharacter().Money -= price;
                        API.sendChatMessageToPlayer(sender, $"[BUSINESS] You have sucessfully bought a ~g~{name}~w~ for ~g~${price}.");
                        break;

                    case InventoryManager.GiveItemErrors.NotEnoughSpace:
                        API.sendChatMessageToPlayer(sender, $"[BUSINESS] You dont have enough space for that item. Need {item.AmountOfSlots} Slots.");
                        break;

                    case InventoryManager.GiveItemErrors.MaxAmountReached:
                        API.sendChatMessageToPlayer(sender, $"[BUSINESS] You have reached the maximum allowed ammount of that item.");
                        break;
                }
            }
        }

        [Command("buy")]
        public void Buy(Client player)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop == null)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a property.");
                return;
            }

            if (prop.Type == PropertyManager.PropertyTypes.Hardware)
            {
                API.freezePlayer(player, true);
                List<string[]> itemsWithPrices = new List<string[]>();
                foreach (var itm in ItemManager.HardwareItems)
                {
                    itemsWithPrices.Add(new[] { itm[0], itm[1], itm[2], prop.ItemPrices.Single(x => x.Key == itm[0]).Value.ToString() });
                }
                API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "Hardware", prop.PropertyName);
            }
            else if (prop.Type == PropertyManager.PropertyTypes.TwentyFourSeven)
            {
                API.freezePlayer(player, true);
                List<string[]> itemsWithPrices = new List<string[]>();
                foreach (var itm in ItemManager.TwentyFourSevenItems)
                {
                    itemsWithPrices.Add(new[] { itm[0], itm[1], itm[2], prop.ItemPrices.Single(x => x.Key == itm[0]).Value.ToString() });
                }
                API.triggerClientEvent(player, "property_buy", API.toJson(itemsWithPrices.ToArray()), "24/7", prop.PropertyName);
            }
            else
            {
                API.sendChatMessageToPlayer(player, "This property doesn't sell anything.");
            }
        }
    }
}
