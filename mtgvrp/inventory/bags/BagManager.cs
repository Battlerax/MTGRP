

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.player_manager;

namespace mtgvrp.inventory.bags
{
    class BagManager : Script
    {
        public BagManager()
        {
            InventoryManager.OnStorageGetItem += InventoryManager_OnStorageGetItem;
            InventoryManager.OnStorageLoseItem += InventoryManager_OnStorageLoseItem;
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;
        }

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            var items = InventoryManager.DoesInventoryHaveItem(e.Character, typeof(BagItem));
            if (items.Length == 1)
            {
                BagItem item = (BagItem)items[0];
                NAPI.Player.SetPlayerClothes(e.Character.Client, 5, item.BagType, item.BagDesign);
            }
        }

        private void InventoryManager_OnStorageLoseItem(IStorage sender, InventoryManager.OnLoseItemEventArgs args)
        {
            if (sender.GetType() == typeof(Character))
            {
                if (args.Item.GetType() == typeof(BagItem))
                {
                    Character chr = (Character)sender;
                    NAPI.Player.SetPlayerClothes(chr.Client, 5, 0, 0);
                }
            }
        }

        private void InventoryManager_OnStorageGetItem(IStorage sender, InventoryManager.OnGetItemEventArgs args)
        {
            if (sender.GetType() == typeof(Character))
            {
                if (args.Item.GetType() == typeof(BagItem))
                {
                    Character chr = (Character) sender;
                    BagItem item = (BagItem) args.Item;
                    NAPI.Player.SetPlayerClothes(chr.Client, 5, item.BagType, item.BagDesign);
                }
            }
        }

        [Command("managebag"), Help(HelpManager.CommandGroups.Inventory, "Manages your backpack.")]
        public void Managebag(Client player)
        {
            Character character = player.GetCharacter();
            IInventoryItem[] bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You don't have a bag.");
                return;
            }

            //Show the window.
            InventoryManager.ShowInventoryManager(player, character, (BagItem)bag[0], "Inventory: ", "Bag: ");
        }

        [Command("bagname"), Help(HelpManager.CommandGroups.Inventory, "Changes your bag name.", "The name")]
        public void BagName(Client player, string name)
        {
            Character character = player.GetCharacter();
            IInventoryItem[] bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You don't have a bag.");
                return;
            }
            var bg = (BagItem) bag[0];
            bg.BagName = name;

            NAPI.Chat.SendChatMessageToPlayer(player, "Bag name was changed sucessfully.");
        }
    }
}
