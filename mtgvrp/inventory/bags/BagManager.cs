using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using RoleplayServer.core;
using RoleplayServer.player_manager;

namespace RoleplayServer.inventory.bags
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
            var items = InventoryManager.DoesInventoryHaveItem(e.character, typeof(BagItem));
            if (items.Length == 1)
            {
                BagItem item = (BagItem)items[0];
                API.setPlayerClothes(e.character.Client, 5, item.BagType, item.BagDesign);
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

        //TODO: test cmd.
        [Command("givemebag")]
        public void GiveMeBag(Client player, int type, int design)
        {
            API.sendChatMessageToPlayer(player, InventoryManager.GiveInventoryItem(player.GetCharacter(), new BagItem() { BagDesign = design, BagType = type }, 1, true).ToString());
        }
        [Command("setmyclothes")]
        public void setmyclothes(Client player, int slot, int drawable, int texture)
        {
            API.setPlayerClothes(player, slot, drawable, texture);
        }
    }
}
