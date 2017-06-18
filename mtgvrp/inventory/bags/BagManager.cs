using GTANetworkServer;
using mtgvrp.core;
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
                API.setPlayerClothes(e.Character.Client, 5, item.BagType, item.BagDesign);
            }
        }

        private void InventoryManager_OnStorageLoseItem(IStorage sender, InventoryManager.OnLoseItemEventArgs args)
        {
            if (sender.GetType() == typeof(Character))
            {
                if (args.Item.GetType() == typeof(BagItem))
                {
                    Character chr = (Character)sender;
                    API.setPlayerClothes(chr.Client, 5, 0, 0);
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
                    API.setPlayerClothes(chr.Client, 5, item.BagType, item.BagDesign);
                }
            }
        }

        [Command("managebag")]
        public void Managebag(Client player)
        {
            Character character = player.GetCharacter();
            IInventoryItem[] bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag.");
                return;
            }

            //Show the window.
            InventoryManager.ShowInventoryManager(player, character, (BagItem)bag[0], "Inventory: ", "Bag: ");
        }

        [Command("bagname")]
        public void BagName(Client player, string name)
        {
            Character character = player.GetCharacter();
            IInventoryItem[] bag = InventoryManager.DoesInventoryHaveItem(character, typeof(BagItem));
            if (bag.Length != 1)
            {
                API.sendNotificationToPlayer(player, "You don't have a bag.");
                return;
            }
            var bg = (BagItem) bag[0];
            bg.BagName = name;

            API.sendChatMessageToPlayer(player, "Bag name was changed sucessfully.");
        }
    }
}
