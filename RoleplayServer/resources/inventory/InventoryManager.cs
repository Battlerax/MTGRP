using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory
{
    class InventoryManager : Script
    {
        public InventoryManager()
        {
			//TODO: Not sure if items still need to be registered here, but do so if you ever see a related exception.
            //BsonClassMap.RegisterClassMap<TestItem>();
        }

        public enum GiveItemErrors
        {
            NotEnoughSpace,
            HasBlockingItem,
            MaxAmountReached,
            Success
        }
        public static GiveItemErrors GiveInventoryItem(IStorage storage, IInventoryItem item, bool ignoreBlocking = false)
        {
            if (storage.Inventory == null) storage.Inventory = new List<IInventoryItem>();
            //Make sure he doesn't have blocking item.
            if(storage.Inventory.FirstOrDefault(x => x.IsBlocking == true) != null && ignoreBlocking == false)
                return GiveItemErrors.HasBlockingItem;

            //Check if player has simliar item.
            var oldItem = storage.Inventory.FirstOrDefault(x => x.GetType() == item.GetType());
            if (oldItem == null || oldItem.CanBeStacked == false)
            {
                //Check if has enough space.
                if ((GetInventoryFilledSlots(storage) + item.Amount * item.AmountOfSlots) <= storage.MaxInvStorage)
                {
                    //Add.
                    storage.Inventory.Add(item);
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
            else
            {
                if (item.MaxAmount != -1 && oldItem.Amount >= item.MaxAmount)
                {
                    return GiveItemErrors.MaxAmountReached;
                }

                //Make sure there is space again.
                if ((GetInventoryFilledSlots(storage) + item.Amount * item.AmountOfSlots) <= storage.MaxInvStorage)
                {
                    //Add.
                    oldItem.Amount += item.Amount;
                    if (oldItem.Amount == 0)
                    {
                        storage.Inventory.Remove(oldItem);
                    }
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
        }

        public static T DoesInventoryHaveItem<T>(IStorage storage)
        {
            return (T)storage.Inventory.FirstOrDefault(x => x.GetType() == typeof(T));
        }

        public static Type ParseInventoryItem(string item)
        {
            var allItems =
                Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IInventoryItem).IsAssignableFrom(x) && x.IsClass).ToArray();

            foreach (var i in allItems)
            {
                IInventoryItem instance = (IInventoryItem)Activator.CreateInstance(i);
                if (instance.CommandFriendlyName == item)
                    return i;
            }
            return null;
        }

        public static int GetInventoryFilledSlots(IStorage storage)
        {
            int value = 0;
            storage.Inventory.ForEach(x => value += x.AmountOfSlots * x.Amount);
            return value;
        }

        public static bool DeleteInventoryItem(IStorage storage, Type item)
        {
            if (storage.Inventory.RemoveAll(x => x.GetType() == item) > 0)
            return true;

            return false;
        }

        public static IInventoryItem ItemTypeToNewObject(Type item)
        {
            return (IInventoryItem)Activator.CreateInstance(item);
        }

        [Command("give")]
        public void give_cmd(Client player, string id, string item, int amount)
        {
            var targetClient = PlayerManager.ParseClient(id);
            if (targetClient == null)
            {
                API.sendNotificationToPlayer(player, "Target player not found.");
                return;
            }
            Character sender = API.getEntityData(player, "Character");
            Character target = API.getEntityData(targetClient, "Character");
            if (player.position.DistanceTo(targetClient.position) > 5f)
            {
                API.sendNotificationToPlayer(player, "You must be near the target player to give him an item.");
                return;
            }

            //Get the item.
            var itemType = ParseInventoryItem(item);
            if (itemType == null)
            {
                API.sendNotificationToPlayer(player, "That item doesn't exist.");
                return;
            }
            var itemObj = ItemTypeToNewObject(itemType);
            if (itemObj.CanBeGiven == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be given.");
                return;
            }

            //Make sure he does have such amount.
            var sendersItem = DoesInventoryHaveItem(sender, itemType);
            if (sendersItem == null || sendersItem.Amount < amount)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount.");
                return;
            }

            //Give.
            var newItem = sendersItem.Copy();
            newItem.Amount = amount;
            switch (GiveInventoryItem(target, newItem))
            {
                case GiveItemErrors.NotEnoughSpace:
                    API.sendNotificationToPlayer(player, "The target player doesn't have enough space in his inventory.");
                    API.sendNotificationToPlayer(targetClient,
                        "Someone has tried to give you an item but failed due to insufficient inventory.");
                    break;

                case GiveItemErrors.HasBlockingItem:
                    API.sendNotificationToPlayer(player, "The target player has a blocking item in hand.");
                    API.sendNotificationToPlayer(targetClient,
                        "You have a blocking item in-hand, place it somewhere first. /inv to find out what it is.");
                    break;

                case GiveItemErrors.Success:
                    API.sendNotificationToPlayer(player,
                        $"You have sucessfully given ~g~{amount}~w~ ~g~{sendersItem.LongName}~w~ to ~g~{target.CharacterName}~w~.");
                    API.sendNotificationToPlayer(targetClient,
                        $"You have receieved ~g~{amount}~w~ ~g~{sendersItem.LongName}~w~ from ~g~{sender.CharacterName}~w~.");

                    //Remove from their inv.
                    sendersItem.Amount -= amount;
                    break;
            }
        }

        [Command("drop")]
        public void drop_cmd(Client player, string item, int amount)
        {
            Character character = API.getEntityData(player, "Character");

            //Get the item.
            var itemType = ParseInventoryItem(item);
            if (itemType == null)
            {
                API.sendNotificationToPlayer(player, "That item doesn't exist.");
                return;
            }
            var itemObj = ItemTypeToNewObject(itemType);
            if (itemObj.CanBeDropped == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be dropped.");
                return;
            }

            //Get in inv.
            var sendersItem = DoesInventoryHaveItem(character, itemType);
            if (sendersItem == null || sendersItem.Amount < amount)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount.");
                return;
            }

            if (amount == sendersItem.Amount)
                DeleteInventoryItem(character, itemType);
            else
                sendersItem.Amount -= amount;

            API.sendNotificationToPlayer(player, "Item was sucessfully dropped.");
        }

        #region Stashing System: 
        private Dictionary<NetHandle, IInventoryItem> stashedItems = new Dictionary<NetHandle, IInventoryItem>();

        [Command("stash")]
        public void stash_cmd(Client player, string item, int amount)
        {
            Character character = API.getEntityData(player, "Character");

            //Get the item.
            var itemType = ParseInventoryItem(item);
            if (itemType == null)
            {
                API.sendNotificationToPlayer(player, "That item doesn't exist.");
                return;
            }
            var itemObj = ItemTypeToNewObject(itemType);
            if (itemObj.CanBeStashed == false)
            {
                API.sendNotificationToPlayer(player, "That item cannot be stashed.");
                return;
            }

            //Get in inv.
            var sendersItem = DoesInventoryHaveItem(character, itemType);
            if (sendersItem == null || sendersItem.Amount < amount)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount.");
                return;
            }

            //Create object and add to list.
            var droppedObject = API.createObject(sendersItem.Object, player.position.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, 0));
            stashedItems.Add(droppedObject, sendersItem.Copy());

            //Decrease.
            if (amount == sendersItem.Amount)
                DeleteInventoryItem(character, itemType);
            else
                sendersItem.Amount -= amount;

            //Send message.
            API.sendNotificationToPlayer(player, $"You have sucessfully stashed ~g~{amount} {sendersItem.LongName}~w~. Use /pickupstash to take it.");
        }

        [Command("pickupstash")]
        public void pickupstash_cmd(Client player)
        {
            //Check if near any stash.
            var items = stashedItems.Where(x => API.getEntityPosition(x.Key).DistanceTo(player.position) <= 3).ToArray();
            if (!items.Any())
            {
                API.sendNotificationToPlayer(player, "You aren't near any stash.");
                return;
            }

            //Just get the first one and take it.
            Character character = API.getEntityData(player, "Character");
            switch (GiveInventoryItem(character, items.First().Value))
            {
                case GiveItemErrors.NotEnoughSpace:
                    API.sendNotificationToPlayer(player, "You don't have enough space in his inventory.");
                    break;

                case GiveItemErrors.HasBlockingItem:
                    API.sendNotificationToPlayer(player, "You have a blocking item in hand.");
                    break;

                case GiveItemErrors.Success:
                    API.sendNotificationToPlayer(player,
                        $"You have sucessfully taken ~g~{items.First().Value.Amount}~w~ ~g~{items.First().Value.LongName}~w~ from the stash.");

                    //Remove object and item from list.
                    API.deleteEntity(items.First().Key);
                    stashedItems.Remove(items.First().Key);
                    break;
            }
        }

        #endregion

        [Command("inventory", Alias = "inv")]
        public void showinventory_cmd(Client player)
        {
            //TODO: For now can be just text-based even though I'd recommend it to be a CEF.
            Character character = API.getEntityData(player, "Character");

            //First the main thing.
            API.sendChatMessageToPlayer(player, "-------------------------------------------------------------");
            API.sendChatMessageToPlayer(player, $"[INVENTORY] {GetInventoryFilledSlots(character)}/{character.MaxInvStorage} Slots [INVENTORY]");
            
            //For Each item.
            foreach (var item in character.Inventory)
            {
                API.sendChatMessageToPlayer(player, $"* ~r~{item.LongName}~w~[{item.CommandFriendlyName}] ({item.Amount}) Weights {item.AmountOfSlots} Slots" + (item.IsBlocking ? " [BLOCKING]" : ""));
            }

            //Ending
            API.sendChatMessageToPlayer(player, "-------------------------------------------------------------");
        }

        //TODO: TEST COMMAND.
        [Command("givemeitem")]
        public void GiveMeItem(Client player, string item, int amount)
        {
            Character character = API.getEntityData(player, "Character");
            Type itemType = ParseInventoryItem(item);
            if (itemType != null)
            {
                var actualitem = ItemTypeToNewObject(itemType);
                actualitem.Amount = amount;
                switch (GiveInventoryItem(character, actualitem))
                {
                    case GiveItemErrors.NotEnoughSpace:
                        API.sendChatMessageToPlayer(player, "You can't hold anymore items in your inventory.");
                        break;
                    case GiveItemErrors.Success:
                        API.sendChatMessageToPlayer(player, "DONE!");
                        break;
                }
            }
            else
                API.sendChatMessageToPlayer(player, "Invalid item name.");
        }
    }
}
