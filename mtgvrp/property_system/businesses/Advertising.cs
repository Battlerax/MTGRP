using System.Timers;

using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.phone_manager;
using mtgvrp.player_manager;
using mtgvrp.core.Help;

namespace mtgvrp.property_system.businesses
{
    public class Advertising : Script
    {
        //Players can only advertise every 15 seconds.
        public bool CanAdvertise = true;
        public Timer AdvertTimer;


        // Commands

        [Command("advertise", Alias = "ad", GreedyArg = true), Help(HelpManager.CommandGroups.General, "To create an IC advertisment for everyone with a phone to see.", new[] { "Text for your ad." })]
        public void advertisement_cmd(Client player, string text)
        {

            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Advertising)
            {
                API.sendChatMessageToPlayer(player, "You aren't at an advertising interaction point.");
                return;
            }

            Character character = player.GetCharacter();

            var phone = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));

            int price = biz.ItemPrices.SingleOrDefault(x => x.Key == "advertprice").Value;

            if (Money.GetCharacterMoney(character) - price < 0)
            {
                API.sendChatMessageToPlayer(player, "~r~Advertising costs " + price + "$. You don't have enough money.");
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
                var receiverPhone = InventoryManager.DoesInventoryHaveItem<Phone>(receiver);
                
                InventoryManager.DeleteInventoryItem(character, typeof(Money), price);

                if(receiverPhone.Length == 0)
                    continue;

                if (receiverPhone[0].IsOn)
                {
                    receiver.Client.sendChatMessage("~g~[AD] (#" + senderPhone.PhoneNumber + "): " + text);
                }
            }

            CanAdvertise = false;
            AdvertTimer = new Timer { Interval = 15000 };
            AdvertTimer.Elapsed += delegate { ResetAdvertTimer(); };
            AdvertTimer.Start();

            player.sendChatMessage("Advertisement subimtted.");
            LogManager.Log(LogManager.LogTypes.Ads, $"{player.GetCharacter().CharacterName}[{player.GetAccount().AccountName}]: {text}");
        }


        //LATER TO ADD ADVERTISING LISTINGS TO AN ADVERTISEMENT PHONE APP.

        public void ResetAdvertTimer()
        {
            AdvertTimer.Stop();
            CanAdvertise = true;
        }
    }
}
