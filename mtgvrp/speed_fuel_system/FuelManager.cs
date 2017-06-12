using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.vehicle_manager;

namespace mtgvrp.speed_fuel_system
{
    class FuelManager : Script
    {
        public Timer FuelTimer;

        public FuelManager()
        {
            FuelTimer = new Timer(53000);
            FuelTimer.Elapsed += FuelTimer_Elapsed;
            FuelTimer.Start();

            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "fuel_getvehiclefuel" && API.isPlayerInAnyVehicle(sender) && API.getPlayerVehicleSeat(sender) == -1 )
            {
                RoleplayServer.vehicle_manager.Vehicle veh = API.getEntityData(API.getPlayerVehicle(sender), "Vehicle");
                API.triggerClientEvent(sender, "fuel_updatevalue", veh.Fuel);
            }
        }

        private void FuelTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var veh in VehicleManager.Vehicles)
            {
                if (!veh.IsSpawned) continue;
                if (API.getVehicleEngineStatus(veh.NetHandle) != true || veh.Fuel <= 0) continue;

                var ocups = API.getVehicleOccupants(veh.NetHandle);

                //Reduce fuel by one.
                veh.Fuel -= 1;
                if (veh.Fuel <= 0)
                {
                    API.setVehicleEngineStatus(veh.NetHandle, false);
                    if(ocups.Length > 0)
                        API.sendChatMessageToPlayer(ocups[0], "~y~The vehicle fuel has finished.");
                }

                //Notify driver with loss of fuel.
                if (ocups.Length > 0)
                {
                    API.triggerClientEvent(ocups[0], "fuel_updatevalue", veh.Fuel);
                }
            }
        }
    }
}
