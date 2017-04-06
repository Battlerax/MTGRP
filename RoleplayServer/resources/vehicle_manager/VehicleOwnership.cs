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
            Character character = API.getEntityData(sender, "Character");
            switch (eventName)
            {
                case "myvehicles_locatecar":
                    vehicle_manager.Vehicle lcVeh =
                        character.OwnedVehicles.Single(x => x.NetHandle.Value == Convert.ToInt32(arguments[0]));
                    Vector3 loc = API.getEntityPosition(lcVeh.NetHandle);
                    API.triggerClientEvent(sender, "myvehicles_setCheckpointToCar", loc.X, loc.Y, loc.Z);
                    API.sendChatMessageToPlayer(sender, "A checkpoint has been set to the vehicle.");
                    break;

                case "myvehicles_abandoncar":
                    vehicle_manager.Vehicle acVeh =
                        character.OwnedVehicles.Single(x => x.Id == Convert.ToInt32(arguments[0]));
                    VehicleManager.despawn_vehicle(acVeh);
                    character.OwnedVehicles.Remove(acVeh);
                    API.sendChatMessageToPlayer(sender, $"You have sucessfully abandoned your ~r~{API.getVehicleDisplayName(acVeh.VehModel)}~w~");
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
