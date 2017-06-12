using System.Collections.Generic;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.core;
using RoleplayServer.inventory;
using RoleplayServer.player_manager;
using RoleplayServer.phone_manager;
using System.Linq;

namespace RoleplayServer.property_system.businesses
{
    public class Advertising : Script
    {
        //Players can only advertise every 15 seconds.
        public bool CanAdvertise = true;
        public Timer AdvertTimer;


        // Commands

        [Command("advertise", Alias = "ad", GreedyArg = true)]
        public void advertisement_cmd(Client player, string text)
        {

            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Advertising)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an advertising interaction point.");
                return;
            }

            Character character = API.getEntityData(player, "Character");

            var phone = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));

            if (Money.GetCharacterMoney(character) - biz.AdvertisingPrice < 0)
            {
                API.sendChatMessageToPlayer(player, "~r~Advertising costs " + biz.AdvertisingPrice + "$. You don't have enough money.");
                return;
            }

            if (phone.Length == 0)
            {
                player.sendChatMessage("You must own a phone before submitting an advertisement.");
                return;
            }
            
            if (!CanAdvertise)
            {
                player.sendChatMessage("An advertisement has just been placed. Please wait 15 seconds.");
                return;
            }

            if (text.Length < 5)
            {
                player.sendChatMessage("Advertisement text must be longer than 5 characters.");
                return;
            }

            foreach (var receiver in PlayerManager.Players)
            {
                var senderPhone = InventoryManager.DoesInventoryHaveItem<Phone>(character)[0];
                var receiverPhone = InventoryManager.DoesInventoryHaveItem<Phone>(receiver)[0];

                InventoryManager.DeleteInventoryItem(character, typeof(Money), biz.AdvertisingPrice);

                if (receiverPhone.IsOn)
                {
                    receiver.Client.sendChatMessage("~g~[AD] (#" + senderPhone.Number + "): " + text);
                }

                player.sendChatMessage("Advertisement subimtted.");
                CanAdvertise = false;
                AdvertTimer = new Timer { Interval = 15000 };
                AdvertTimer.Elapsed += delegate { ResetAdvertTimer(); };
                AdvertTimer.Start();
            }

        }

        [Command("setadvertprice")]
        public void setadvertprice_cmd(Client player, string price)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Advertising)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an advertising interaction point.");
                return;
            }

            Character character = API.getEntityData(player, "Character");
            Account account = API.getEntityData(player, "Account");

            if (biz.OwnerId == character.Id)
            {
                biz.AdvertisingPrice = int.Parse(price);
                player.sendChatMessage("Advertising price changed.");
            }
            else { player.sendChatMessage("Invalid permissions."); }
        }


        //LATER TO ADD ADVERTISING LISTINGS TO AN ADVERTISEMENT PHONE APP.

        public void ResetAdvertTimer()
        {
            AdvertTimer.Stop();
            CanAdvertise = true;
        }
    }
}
