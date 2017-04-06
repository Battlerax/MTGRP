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
                        VehicleManager.Vehicles.Single(x => x.NetHandle.Value == Convert.ToInt32(arguments[0]) && x.OwnerId == character.Id);
                    Vector3 loc = API.getEntityPosition(lcVeh.NetHandle);
                    API.triggerClientEvent(sender, "myvehicles_setCheckpointToCar", loc.X, loc.Y, loc.Z);
                    API.sendChatMessageToPlayer(sender, "A checkpoint has been set to the vehicle.");
                    break;

                case "myvehicles_abandoncar":
                    vehicle_manager.Vehicle acVeh =
                        VehicleManager.Vehicles.Single(x => x.Id == Convert.ToInt32(arguments[0]) && x.OwnerId == character.Id);
                    VehicleManager.despawn_vehicle(acVeh);
                    VehicleManager.delete_vehicle(acVeh);
                    acVeh.Delete();
                    character.OwnedVehicles.Remove(acVeh.Id);
                    API.sendChatMessageToPlayer(sender, $"You have sucessfully abandoned your ~r~{API.getVehicleDisplayName(acVeh.VehModel)}~w~");
                    break;
            }
        }

        [Command("myvehicles")]
        public void myvehicles_cmd(Client player)
        {
            //Get all owned vehicles and send them.
            Character character = API.getEntityData(player, "Character");
            string[][] cars = VehicleManager.Vehicles
                .Where(x => x.OwnerId == character.Id)
                .Select(x => new [] { API.getVehicleDisplayName(x.VehModel), x.Id.ToString(), x.NetHandle.Value.ToString()}).ToArray();

            API.triggerClientEvent(player, "myvehicles_showmenu", API.toJson(cars));
        }
    }
}
