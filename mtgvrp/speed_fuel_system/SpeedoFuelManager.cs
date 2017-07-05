using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;

namespace mtgvrp.speed_fuel_system
{
    class SpeedoFuelManager : Script
    {
        public Timer FuelTimer;

        public SpeedoFuelManager()
        {
            FuelTimer = new Timer(53000);
            FuelTimer.Elapsed += FuelTimer_Elapsed;
            FuelTimer.Start();

            API.onClientEventTrigger += API_onClientEventTrigger;
            API.onPlayerExitVehicle += API_onPlayerExitVehicle;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "fuel_getvehiclefuel" && API.isPlayerInAnyVehicle(sender) &&
                API.getPlayerVehicleSeat(sender) == -1)
            {
                Vehicle veh = API.getEntityData(API.getPlayerVehicle(sender), "Vehicle");
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
                    if (ocups.Length > 0)
                        API.sendChatMessageToPlayer(ocups[0], "~y~The vehicle fuel has finished.");
                }

                //Notify driver with loss of fuel.
                if (ocups.Length > 0)
                {
                    API.triggerClientEvent(ocups[0], "fuel_updatevalue", veh.Fuel);
                }
            }
        }

        [Command("togspeedo")]
        public void TogSpeedo(Client player)
        {
            Account a = player.GetAccount();
            a.IsSpeedoOn = !a.IsSpeedoOn;
            API.sendChatMessageToPlayer(player, a.IsSpeedoOn ? "You've sucessfully turned on the speedometer." : "You've sucessfully turned off the speedometer.");
        }

        [Command("refuel")]
        public void Refuel(Client player, int fuel = 0)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type == PropertyManager.PropertyTypes.GasStation)
            {
                if (API.isPlayerInAnyVehicle(player) && API.getPlayerVehicleSeat(player) == -1)
                {
                    var vehEntity = API.getPlayerVehicle(player);
                    Vehicle veh = API.getEntityData(vehEntity, "Vehicle");

                    if (API.getVehicleEngineStatus(vehEntity))
                    {
                        API.sendChatMessageToPlayer(player, "Vehicle engine must be off.");
                        return;
                    }

                    if (fuel == 0)
                        fuel = 100 - veh.Fuel;

                    var pendingFuel = fuel;

                    if (pendingFuel > 100 || pendingFuel + veh.Fuel > 100 || pendingFuel < 0)
                    {
                        API.sendChatMessageToPlayer(player, "Vehicle fuel can't be above 100 or negative.");
                    }

                    if (Money.GetCharacterMoney(player.GetCharacter()) < pendingFuel * prop.ItemPrices["gas"])
                    {
                        API.sendChatMessageToPlayer(player,
                            $"You don't have enough money to get ~r~{pendingFuel}~w~ units of fuel.~n~Its worth ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~.");
                        return;
                    }

                    API.sendChatMessageToPlayer(player,
                        $"You will be charged ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~ to get ~r~{pendingFuel}~w~ units of fuel.");
                    API.freezePlayer(player, true);
                    API.setEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
                    veh.RefuelProp = prop;
                    FuelVeh(new[] {player, vehEntity});
                    if (API.hasEntityData(vehEntity, "PENDING_FUEL"))
                    {
                        API.setEntityData(player, "FUELING_VEHICLE", vehEntity);
                        veh.FuelingTimer = new System.Threading.Timer(FuelVeh, new[] {player, vehEntity}, 3000, 3000);
                        return;
                    }
                }
                else
                {
                    API.sendChatMessageToPlayer(player, "You must be driving a vehicle.");
                }
            }
            else
            {
                API.sendChatMessageToPlayer(player, "You must be at a gas station.");
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle)
        {
            if (API.hasEntityData(player, "FUELING_VEHICLE"))
            {
                var vehEntity = API.getEntityData(player, "FUELING_VEHICLE");
                if (vehEntity == vehicle)
                {
                    API.sendChatMessageToPlayer(player, "Ended Refuel.");
                    Vehicle veh = API.getEntityData(vehicle, "Vehicle");
                    veh.FuelingTimer?.Dispose();
                    API.freezePlayer(player, false);
                    veh.Save();
                }
            }
        }

        private void FuelVeh(System.Object vars)
        {
            var handles = (NetHandle[]) vars;
            Client playerEntity = API.getPlayerFromHandle((NetHandle) handles[0]);
            NetHandle vehEntity = (NetHandle) handles[1];
            Vehicle veh = API.getEntityData(vehEntity, "Vehicle");
            Character c = API.getEntityData(playerEntity, "Character");

            if (API.getVehicleEngineStatus(vehEntity))
            {
                veh.FuelingTimer?.Dispose();
                API.resetEntityData(vehEntity, "PENDING_FUEL");
                API.resetEntityData(playerEntity, "FUELING_VEHICLE");
                API.freezePlayer(playerEntity, false);
                API.sendChatMessageToPlayer(playerEntity, "Refuel has been cancelled cause the engine has been turned on.");
                veh.Save();
                return;
            }

            int pendingFuel = API.getEntityData(vehEntity, "PENDING_FUEL");

            if (pendingFuel <= 0 || veh.RefuelProp.Supplies <= 0)
            {
                API.triggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
                veh.FuelingTimer?.Dispose();
                API.resetEntityData(vehEntity, "PENDING_FUEL");
                API.resetEntityData(playerEntity, "FUELING_VEHICLE");
                API.freezePlayer(playerEntity, false);
                API.sendChatMessageToPlayer(playerEntity, "Refuel has been finished.");
                veh.Save();
                return;
            }

            if (pendingFuel < 10)
            {
                veh.Fuel += pendingFuel;
                pendingFuel -= pendingFuel;
                InventoryManager.DeleteInventoryItem<Money>(c, pendingFuel * veh.RefuelProp.ItemPrices["gas"]);
                veh.RefuelProp.Supplies--;
            }
            else
            {
                veh.Fuel += 10;
                pendingFuel -= 10;
                InventoryManager.DeleteInventoryItem<Money>(c, 10 * veh.RefuelProp.ItemPrices["gas"]);
                veh.RefuelProp.Supplies--;
            }

            API.triggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
            API.setEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
        }
    }
}
