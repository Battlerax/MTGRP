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
            switch (eventName)
            {
                case "bag_moveFromInvToBag":
                    string id = (string)arguments[0];
                    string shortname = (string)arguments[1];
                    string amount = (string) arguments[2];

                    Character character = API.getEntityData(sender, "Character");

                    //See if has item.
                    var itemType = InventoryManager.ParseInventoryItem(shortname);
                    if (itemType == null)
                    {
                        API.sendNotificationToPlayer(sender, "That item type doesn't exist.");
                        return;
                    }
                    var playerItem = InventoryManager.DoesInventoryHaveItem(character, itemType).SingleOrDefault(x => x.Id.ToString() == id);
                    if (playerItem == null)
                    {
                        API.sendNotificationToPlayer(sender, "You don't have that item.");
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
                    InventoryManager.DeleteInventoryItem(character, itemType, 1,
                        x => x.Id.ToString() == id && x.CommandFriendlyName == shortname);

                    switch (InventoryManager.GiveInventoryItem(bag[0], playerItem, 1, true))
                    {
                        case InventoryManager.GiveItemErrors.NotEnoughSpace:
                            API.sendNotificationToPlayer(sender, "You don't have enough slots in bag.");
                            break;
                        case InventoryManager.GiveItemErrors.MaxAmountReached:
                            API.sendNotificationToPlayer(sender, "Reached max amount of that item in bag.");
                            break;
                        case InventoryManager.GiveItemErrors.Success:
                            //Send event done.
                            API.triggerClientEvent(sender, "moveItemFromInvToBag", id, shortname, amount); //Id should be same cause it was already set since it was in player inv.
                            break;
                    }
                    break;
            }
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
            string[][] bagItems = bag[0].Inventory.Select(x => new [] {x.Id.ToString(), x.LongName, x.CommandFriendlyName, x.Amount.ToString()}).ToArray();
            string[][] invItems = character.Inventory.Select(x => new[] {x.Id.ToString(), x.LongName, x.CommandFriendlyName, x.Amount.ToString()}).ToArray();
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
