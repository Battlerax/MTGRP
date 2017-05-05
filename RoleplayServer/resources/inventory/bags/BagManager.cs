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
        public BagManager()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "bag_moveFromInvToBag":
                    string id = (string)arguments[0];
                    string shortname = (string)arguments[1];
                    int amount;
                    if (!int.TryParse((string)arguments[2], out amount))
                    {
                        API.sendChatMessageToPlayer(sender, "Invalid amount entered.");
                        return;
                    }
                    if (amount <= 0)
                    {
                        API.sendChatMessageToPlayer(sender, "Amount must not be zero or negative.");
                        return;
                    }

                    Character character = sender.GetCharacter();

                    //See if has item.
                    var itemType = InventoryManager.ParseInventoryItem(shortname);
                    if (itemType == null)
                    {
                        API.sendNotificationToPlayer(sender, "That item type doesn't exist.");
                        return;
                    }
                    var playerItem = InventoryManager.DoesInventoryHaveItem(character, itemType).SingleOrDefault(x => x.Id.ToString() == id);
                    if (playerItem == null || playerItem.Amount < amount)
                    {
                        API.sendNotificationToPlayer(sender, "You don't have that item or don't have that amount.");
                        return;
                    }

                    //Get the bag.
                    BagItem[] bag = (BagItem[])InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
                    if (bag.Length != 1)
                    {
                        API.sendNotificationToPlayer(sender, "You don't have a bag right now.");
                        return;
                    }

                    //Add to bag and remove from player
                    InventoryManager.DeleteInventoryItem(character, itemType, amount,
                        x => x.Id.ToString() == id && x.CommandFriendlyName == shortname);

                    switch (InventoryManager.GiveInventoryItem(bag[0], playerItem, amount, true))
                    {
                        case InventoryManager.GiveItemErrors.NotEnoughSpace:
                            API.sendNotificationToPlayer(sender, "You don't have enough slots in bag.");
                            break;
                        case InventoryManager.GiveItemErrors.MaxAmountReached:
                            API.sendNotificationToPlayer(sender, "Reached max amount of that item in bag.");
                            break;
                        case InventoryManager.GiveItemErrors.Success:
                            //Send event done.
                            API.triggerClientEvent(sender, "moveItemFromInvToBag", id, shortname, amount.ToString()); //Id should be same cause it was already set since it was in player inv.
                            break;
                    }
                    break;
            }
        }

        [Command("managebag")]
        public void Managebag(Client player)
        {
            Character character = player.GetCharacter();
            BagItem[] bag = (BagItem[])InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag right now.");
                return;
            }

            //Get the current bag items.
            
        }

        //TODO: test cmd.
        [Command("givemebag")]
        public void GiveMeBag(Client player)
        {
            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(player.GetCharacter(), new BagItem(), 1, true).ToString());
        }
    }
}
