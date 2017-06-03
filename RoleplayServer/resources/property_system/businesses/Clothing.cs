using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.property_system;

namespace RoleplayServer.resources.property_system.businesses
{
    class Clothing : Script
    {
        [Command("buyclothes")]
        public void BuyClothes(Client player)
        {
            var biz = PropertyManager.IsAtPropertyInteraction(player);
            if (biz?.Type != PropertyManager.PropertyTypes.Clothing)
            {
                API.sendChatMessageToPlayer(player, "You aren't at a clothing interaction point.");
                return;
            }
            API.setEntityPosition(player, new Vector3(403, -997, -100));
            API.setEntityRotation(player, new Vector3(0, 0, 177.2663));
            API.triggerClientEvent(player, "properties_buyclothes");
        }
    }
}
