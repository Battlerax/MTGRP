using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.vehicle_manager
{
    class VehicleOwnership : Script
    {
        public VehicleOwnership()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "myvehicles_locatecar":
                    Character character = API.getEntityData(sender, "Character");
                    vehicle_manager.Vehicle veh =
                        character.OwnedVehicles.Single(x => x.NetHandle.Value == Convert.ToInt32(arguments[0]));
                    Vector3 loc = API.getEntityPosition(veh.NetHandle);
                    API.triggerClientEvent(sender, "myvehicles_setCheckpointToCar", loc.X, loc.Y, loc.Z);
                    API.sendChatMessageToPlayer(sender, "A checkpoint has been set to the vehicle.");
                    break;
            }
        }

        [Command("myvehicles")]
        public void myvehicles_cmd(Client player)
        {
            //Get all owned vehicles and send them.
            Character character = API.getEntityData(player, "Character");
            string[][] cars = character.OwnedVehicles.Select(x => new [] { API.getVehicleDisplayName(x.VehModel), x.Id.ToString(), x.NetHandle.Value.ToString()}).ToArray();

            API.triggerClientEvent(player, "myvehicles_showmenu", API.toJson(cars));
        }
    }
}
