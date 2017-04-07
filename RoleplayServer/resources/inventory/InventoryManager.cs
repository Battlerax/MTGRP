using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.inventory
{
    class InventoryManager : Script
    {
        public enum GiveItemErrors
        {
            NotEnoughSpace,
            ItemNotStackable,
            Success
        }
        public static GiveItemErrors GiveItemToPlayer(Character player, IInventoryItem item)
        {
            //Check if player has simliar item.
            var oldItem = player.Inventory.FirstOrDefault(x => x.GetType() == item.GetType());
            if (oldItem == null)
            {
                //Check if has enough space.
                if ((GetPlayerFilledSlots(player) + item.Amount * item.AmountOfSlots) < player.MaxInvStorage)
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
                //Must be stackable.
                if (oldItem.CanBeStacked)
                {
                    //Make sure there is space again.
                    if ((GetPlayerFilledSlots(player) + item.Amount * item.AmountOfSlots) < player.MaxInvStorage)
                    {
                        //Add.
                        oldItem.Amount += item.Amount;
                        return GiveItemErrors.Success;
                    }
                    else
                        return GiveItemErrors.NotEnoughSpace;
                }
                else
                    return GiveItemErrors.ItemNotStackable;
            }
        }

        public IInventoryItem ParseItem(string item)
        {
            var allItems =
                Assembly.GetExecutingAssembly().GetTypes().Where(x => typeof(IInventoryItem).IsAssignableFrom(x) && x.IsClass).ToArray();

            // ReSharper disable once SuspiciousTypeConversion.Global
            foreach (IInventoryItem i in allItems)
            {
                if (i.CommandFriendlyName == item)
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

        //TODO: TEST COMMAND.
        [Command("givemeitem")]
        public void GiveMeItem(Client player, string item, int amount)
        {
            Character character = API.getEntityData(player, "Character");
            IInventoryItem actualitem = ParseItem(item);
            if (actualitem != null)
            {
                switch (GiveItemToPlayer(character, new TestItem() {Amount = amount}))
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
