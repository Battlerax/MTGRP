using System.Linq;
using System.Collections.Generic;
using mtgvrp.player_manager;
using mtgvrp.core.Help;


using GTANetworkAPI;
using mtgvrp.core;

namespace mtgvrp.property_system.businesses
{
    public class VIPLounge : Script
    {

        [Command("buyweapontint"), Help(HelpManager.CommandGroups.General, "Used to buy a weapon tint as a VIP", null)]
        public void buyweapontint_cmd(Client player)
        {
            Account account = player.GetAccount();

            if (account.VipLevel < 1)
            {
                player.SendChatMessage("You must be a ~y~VIP~w~ to use this command.");
                return;
            }

            var biz = PropertyManager.IsAtPropertyInteraction(player);

            if (biz?.Type != PropertyManager.PropertyTypes.VIPLounge)
            {
                API.SendChatMessageToPlayer(player, "You aren't at the VIP interaction point.");
                return;
            }

            API.FreezePlayer(player, true);
            List<string[]> itemsWithPrices = new List<string[]>();
            foreach (var itm in ItemManager.VIPItems)
            {
                itemsWithPrices.Add(new[]
                {
                    itm[0], itm[1], itm[2],
                    biz.ItemPrices.SingleOrDefault(x => x.Key == itm[0]).Value.ToString()
                });
            }
            API.TriggerClientEvent(player, "property_buy", API.ToJson(itemsWithPrices.ToArray()),
                "VIP Weapon Tints",
                biz.PropertyName);

        }
    }
}
        
