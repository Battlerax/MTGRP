using System.Timers;

using GTANetworkAPI;


using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using Vehicle = mtgvrp.vehicle_manager.Vehicle;
using mtgvrp.core.Help;

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

            Event.OnClientEventTrigger += API_onClientEventTrigger;
            Event.OnPlayerExitVehicle += API_onPlayerExitVehicle;
        }

        private void API_onClientEventTrigger(Client sender, string eventName, params object[] arguments)
        {
            if (eventName == "fuel_getvehiclefuel" && API.IsPlayerInAnyVehicle(sender) &&
                API.GetPlayerVehicleSeat(sender) == -1)
            {
                Vehicle veh = API.GetEntityData(API.GetPlayerVehicle(sender), "Vehicle");
                API.TriggerClientEvent(sender, "fuel_updatevalue", veh.Fuel);
            }
        }

        private void FuelTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var veh in VehicleManager.Vehicles)
            {
                if (!veh.IsSpawned) continue;
                if (API.GetVehicleEngineStatus(veh.NetHandle) != true || veh.Fuel <= 0) continue;
                if (API.Shared.GetVehicleClass(veh.VehModel) == 13) continue; //Skip cycles

                var ocups = API.GetVehicleOccupants(veh.NetHandle);

                //Reduce fuel by one.
                veh.Fuel -= 1;
                if (veh.Fuel <= 0)
                {
                    API.SetVehicleEngineStatus(veh.NetHandle, false);
                    if (ocups.Length > 0)
                        API.SendChatMessageToPlayer(ocups[0], "~y~The vehicle fuel has finished.");
                }

                //Notify driver with loss of fuel.
                if (ocups.Length > 0)
                {
                    API.TriggerClientEvent(ocups[0], "fuel_updatevalue", veh.Fuel);
                }
            }
        }

        [Command("togspeedo"), Help(HelpManager.CommandGroups.Vehicles, "Used to find your character statistics", null)]
        public void TogSpeedo(Client player)
        {
            Account a = player.GetAccount();
            a.IsSpeedoOn = !a.IsSpeedoOn;
            API.SendChatMessageToPlayer(player, a.IsSpeedoOn ? "You've sucessfully turned on the speedometer." : "You've sucessfully turned off the speedometer.");
            a.Save();

            if (player.isInVehicle)
            {
                API.TriggerClientEvent(player, "TOGGLE_SPEEDO");
            }
        }

        [Command("refuel"), Help(HelpManager.CommandGroups.Vehicles, "Command to refuel your vehicle from a gas station.", new[] { "Fuel amount wanted (out of 100)" })]
        public void Refuel(Client player, int fuel = 0)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type == PropertyManager.PropertyTypes.GasStation)
            {
                if (API.IsPlayerInAnyVehicle(player) && API.GetPlayerVehicleSeat(player) == -1)
                {
                    var vehEntity = API.GetPlayerVehicle(player);
                    Vehicle veh = API.GetEntityData(vehEntity, "Vehicle");

                    if (API.GetVehicleEngineStatus(vehEntity))
                    {
                        API.SendChatMessageToPlayer(player, "Vehicle engine must be off.");
                        return;
                    }

                    if (player.hasData("FUELING_VEHICLE"))
                    {
                        API.SendChatMessageToPlayer(player, "You're already refueling a vehicle.");
                        return;
                    }

                    if (fuel == 0)
                        fuel = 100 - veh.Fuel;

                    var pendingFuel = fuel;

                    if (pendingFuel > 100 || pendingFuel + veh.Fuel > 100 || pendingFuel < 0)
                    {
                        API.SendChatMessageToPlayer(player, "Vehicle fuel can't be above 100 or negative.");
                        return;
                    }

                    if (Money.GetCharacterMoney(player.GetCharacter()) < pendingFuel * prop.ItemPrices["gas"] && player.GetCharacter().Group.CommandType != group_manager.Group.CommandTypeLspd)
                    {
                        API.SendChatMessageToPlayer(player,
                            $"You don't have enough money to get ~r~{pendingFuel}~w~ units of fuel.~n~It's worth ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~.");
                        return;
                    }

                    API.SendChatMessageToPlayer(player,
                        $"You will be charged ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~ for ~r~{pendingFuel}~w~ units of fuel.");
                    API.FreezePlayer(player, true);
                    API.SetEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
                    veh.RefuelProp = prop;
                    FuelVeh(new[] { player, vehEntity });
                    if (API.HasEntityData(vehEntity, "PENDING_FUEL"))
                    {
                        API.SetEntityData(player, "FUELING_VEHICLE", vehEntity);
                        veh.FuelingTimer = new System.Threading.Timer(FuelVeh, new[] { player, vehEntity }, 3000, 3000);
                        return;
                    }
                }
                else
                {
                    API.SendChatMessageToPlayer(player, "You must be driving a vehicle.");
                }
            }
            else
            {
                API.SendChatMessageToPlayer(player, "You must be at a gas station.");
            }
        }

        private void API_onPlayerExitVehicle(Client player, NetHandle vehicle, int seat)
        {
            if (API.HasEntityData(player, "FUELING_VEHICLE"))
            {
                var vehEntity = API.GetEntityData(player, "FUELING_VEHICLE");
                API.SendChatMessageToPlayer(player, "Refuel ended.");
                Vehicle veh = API.GetEntityData(vehEntity, "Vehicle");
                veh.FuelingTimer?.Dispose();
                API.FreezePlayer(player, false);
                veh.Save();
            }
        }

        private void FuelVeh(System.Object vars)
        {
            var handles = (NetHandle[])vars;
            Client playerEntity = API.GetPlayerFromHandle(handles[0]);
            NetHandle vehEntity = handles[1];

            if (vehEntity.IsNull)
            {
                return;
            }

            Vehicle veh = API.GetEntityData(vehEntity, "Vehicle");

            if (veh == null)
            {
                return;
            }

            if (playerEntity == null)
            {
                veh.FuelingTimer?.Dispose();
                API.ResetEntityData(vehEntity, "PENDING_FUEL");
                return;
            }

            Character c = playerEntity.GetCharacter();

            if (c == null)
            {
                veh.FuelingTimer?.Dispose();
                API.ResetEntityData(vehEntity, "PENDING_FUEL");
                return;
            }

            if (API.GetVehicleEngineStatus(vehEntity))
            {
                veh.FuelingTimer?.Dispose();
                API.ResetEntityData(vehEntity, "PENDING_FUEL");
                API.ResetEntityData(playerEntity, "FUELING_VEHICLE");
                API.FreezePlayer(playerEntity, false);
                API.SendChatMessageToPlayer(playerEntity, "Refuel has been cancelled cause the engine has turned on.");
                veh.Save();
                return;
            }

            int pendingFuel = API.GetEntityData(vehEntity, "PENDING_FUEL") ?? 0;

            if (pendingFuel <= 0 || veh.RefuelProp.Supplies <= 0)
            {
                API.TriggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
                veh.FuelingTimer?.Dispose();
                API.ResetEntityData(vehEntity, "PENDING_FUEL");
                API.ResetEntityData(playerEntity, "FUELING_VEHICLE");
                API.FreezePlayer(playerEntity, false);

                if(veh.RefuelProp.Supplies <= 0)
                    API.SendChatMessageToPlayer(playerEntity, "The gas station ran out of gas.");
                else if (pendingFuel <= 0)
                    API.SendChatMessageToPlayer(playerEntity, "Refueling finished.");

                veh.Save();
                return;
            }

            if (pendingFuel < 10)
            {
                veh.Fuel += pendingFuel;
                pendingFuel -= pendingFuel;
                if (c.Group.CommandType != group_manager.Group.CommandTypeLspd)
                {
                    InventoryManager.DeleteInventoryItem<Money>(c, pendingFuel * veh.RefuelProp.ItemPrices["gas"]);
                }
                veh.RefuelProp.Supplies--;
            }
            else
            {
                veh.Fuel += 10;
                pendingFuel -= 10;
                if (c.Group.CommandType != group_manager.Group.CommandTypeLspd)
                {
                    InventoryManager.DeleteInventoryItem<Money>(c, 10 * veh.RefuelProp.ItemPrices["gas"]);
                }
                veh.RefuelProp.Supplies--;
            }

            API.TriggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
            API.SetEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
        }
    }
}
