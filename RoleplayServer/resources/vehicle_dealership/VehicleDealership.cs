using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.vehicle_dealership
{
    class VehicleDealership : Script
    {
        //Vars: 
        private readonly Vector3[] _dealershipsLocations = {
            new Vector3(0, 0, 0), //TODO: set this coords correctly.
        };

        public VehicleDealership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            throw new NotImplementedException();
        }

        [Command("buyvehicle")]
        public void BuyVehicle(Client player)
        {
            var currentPos = API.getEntityPosition(player);
            if (_dealershipsLocations.Any(dealer => currentPos.DistanceTo(dealer) < 10F))
            {
                API.triggerClientEvent(player, "dealership_showbuyvehiclemenu");
            }
            else
                API.sendChatMessageToPlayer(player, "You aren't near any dealership.");
        }
    }
}
