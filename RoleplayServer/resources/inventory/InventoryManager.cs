using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using MongoDB.Bson.Serialization;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory
{
    class InventoryManager : Script
    {
        public InventoryManager()
        {
            BsonClassMap.RegisterClassMap<TestItem>();
        }

        public enum GiveItemErrors
        {
            NotEnoughSpace,
            HasBlockingItem,
            Success
        }
        public static GiveItemErrors GiveItemToPlayer(Character player, IInventoryItem item)
        {
            if (player.Inventory == null) player.Inventory = new List<IInventoryItem>();
            //Make sure he doesn't have blocking item.
            if(player.Inventory.FirstOrDefault(x => x.IsBlocking == true) != null)
                return GiveItemErrors.HasBlockingItem;

            //Check if player has simliar item.
            var oldItem = player.Inventory.FirstOrDefault(x => x.GetType() == item.GetType());
            if (oldItem == null || oldItem.CanBeStacked == false)
            {
                //Check if has enough space.
                if ((GetPlayerFilledSlots(player) + item.Amount * item.AmountOfSlots) <= player.MaxInvStorage)
                {
                    //Add.
                    player.Inventory.Add(item);
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
            else
            {
                //Make sure there is space again.
                if ((GetPlayerFilledSlots(player) + item.Amount * item.AmountOfSlots) <= player.MaxInvStorage)
                {
                    //Add.
                    oldItem.Amount += item.Amount;
                    return GiveItemErrors.Success;
                }
                else
                    return GiveItemErrors.NotEnoughSpace;
            }
        }

        public static IInventoryItem DoesHaveItem(Character player, Type item)
        {
            return player.Inventory.FirstOrDefault(x => x.GetType() == item);
        }

        public static Type ParseItem(string item)
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

        public static int GetPlayerFilledSlots(Character player)
        {
            int value = 0;
            player.Inventory.ForEach(x => value += x.AmountOfSlots * x.Amount);
            return value;
        }

        public static bool DeleteItem(Character player, Type item)
        {
            if (player.Inventory.RemoveAll(x => x.GetType() == item) > 0)
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
            var itemType = ParseItem(item);
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
            var sendersItem = DoesHaveItem(sender, itemType);
            if (sendersItem == null || sendersItem.Amount < amount)
            {
                API.sendNotificationToPlayer(player, "You don't have that item or you don't have that amount.");
                return;
            }

            //Give.
            var newItem = sendersItem.Copy();
            newItem.Amount = amount;
            switch (GiveItemToPlayer(target, newItem))
            {
                case GiveItemErrors.NotEnoughSpace:
                    API.sendNotificationToPlayer(player, "The target player doesn't have enough space in his inventory.");
                    API.sendNotificationToPlayer(targetClient, "Someone has tried to give you an item but failed due to insufficient inventory.");
                    break;

                case GiveItemErrors.HasBlockingItem:
                    API.sendNotificationToPlayer(player, "The target player has a blocking item in hand.");
                    API.sendNotificationToPlayer(targetClient, "You have a blocking item in-hand, place it somewhere first. /inv to find out what it is.");
                    break;

                case GiveItemErrors.Success:
                    API.sendNotificationToPlayer(player, $"You have sucessfully given ~g~{amount}~w~ ~g~{sendersItem.LongName}~w~ to ~g~{target.CharacterName}~w~.");
                    API.sendNotificationToPlayer(targetClient, $"You have receieved ~g~{amount}~w~ ~g~{sendersItem.LongName}~w~ from ~g~{sender.CharacterName}~w~.");

                    //Remove from their inv.
                    sendersItem.Amount -= amount;
                    break;
            }
    }

        //TODO: TEST COMMAND.
        [Command("givemeitem")]
        public void GiveMeItem(Client player, string item, int amount)
        {
            Character character = API.getEntityData(player, "Character");
            Type itemType = ParseItem(item);
            if (itemType != null)
            {
                var actualitem = ItemTypeToNewObject(itemType);
                actualitem.Amount = amount;
                switch (GiveItemToPlayer(character, actualitem))
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
