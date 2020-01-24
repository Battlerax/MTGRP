using System.Timers;

using GTANetworkAPI;


using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using mtgvrp.vehicle_manager;
using GameVehicle = mtgvrp.vehicle_manager.GameVehicle;
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
        }

        [RemoteEvent("fuel_getvehiclefuel")]
        public void FuelGetVehicleFuel(Client sender, params object[] arguments)
        {
            
            if (NAPI.Player.IsPlayerInAnyVehicle(sender) && NAPI.Player.GetPlayerVehicleSeat(sender) == -1)
            {
                GameVehicle veh = NAPI.Data.GetEntityData(NAPI.Player.GetPlayerVehicle(sender), "Vehicle");
                NAPI.ClientEvent.TriggerClientEvent(sender, "fuel_updatevalue", veh.Fuel);
            }
        }

        private void FuelTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var veh in VehicleManager.Vehicles)
            {
                if (!veh.IsSpawned) continue;
                if (NAPI.Vehicle.GetVehicleEngineStatus(veh.Entity) != true || veh.Fuel <= 0) continue;
                if (API.Shared.GetVehicleClass(veh.VehModel) == 13) continue; //Skip cycles
                
                var ocups = NAPI.Vehicle.GetVehicleOccupants(veh.Entity);

                //Reduce fuel by one.
                veh.Fuel -= 1;
                if (veh.Fuel <= 0)
                {
                    NAPI.Vehicle.SetVehicleEngineStatus(veh.Entity, false);
                    if (ocups.Count > 0)
                        NAPI.Chat.SendChatMessageToPlayer(ocups[0], "~y~The vehicle fuel has finished.");
                }

                //Notify driver with loss of fuel.
                if (ocups.Count > 0)
                {
                    NAPI.ClientEvent.TriggerClientEvent(ocups[0], "fuel_updatevalue", veh.Fuel);
                }
            }
        }

        [Command("togspeedo"), Help(HelpManager.CommandGroups.Vehicles, "Used to find your character statistics", null)]
        public void TogSpeedo(Client player)
        {
            Account a = player.GetAccount();
            a.IsSpeedoOn = !a.IsSpeedoOn;
            NAPI.Chat.SendChatMessageToPlayer(player, a.IsSpeedoOn ? "You've sucessfully turned on the speedometer." : "You've sucessfully turned off the speedometer.");
            a.Save();

            if (player.IsInVehicle)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "TOGGLE_SPEEDO");
            }
        }

        [Command("refuel"), Help(HelpManager.CommandGroups.Vehicles, "Command to refuel your vehicle from a gas station.", new[] { "Fuel amount wanted (out of 100)" })]
        public void Refuel(Client player, int fuel = 0)
        {
            var prop = PropertyManager.IsAtPropertyInteraction(player);
            if (prop?.Type == PropertyManager.PropertyTypes.GasStation)
            {
                if (NAPI.Player.IsPlayerInAnyVehicle(player) && NAPI.Player.GetPlayerVehicleSeat(player) == -1)
                {
                    var vehEntity = player.Vehicle;
                    GameVehicle veh = NAPI.Data.GetEntityData(vehEntity, "Vehicle");

                    if (NAPI.Vehicle.GetVehicleEngineStatus(vehEntity))
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "Vehicle engine must be off.");
                        return;
                    }

                    if (player.HasData("FUELING_VEHICLE"))
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "You're already refueling a vehicle.");
                        return;
                    }

                    if (fuel == 0)
                        fuel = 100 - veh.Fuel;

                    var pendingFuel = fuel;

                    if (pendingFuel > 100 || pendingFuel + veh.Fuel > 100 || pendingFuel < 0)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "Vehicle fuel can't be above 100 or negative.");
                        return;
                    }

                    if (Money.GetCharacterMoney(player.GetCharacter()) < pendingFuel * prop.ItemPrices["gas"] && player.GetCharacter().Group.CommandType != group_manager.Group.CommandTypeLspd)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player,
                            $"You don't have enough money to get ~r~{pendingFuel}~w~ units of fuel.~n~It's worth ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~.");
                        return;
                    }
                    
                    NAPI.Chat.SendChatMessageToPlayer(player,
                        $"You will be charged ~g~${pendingFuel * prop.ItemPrices["gas"]}~w~ for ~r~{pendingFuel}~w~ units of fuel.");
                    NAPI.Player.FreezePlayer(player, true);
                    NAPI.Data.SetEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
                    veh.RefuelProp = prop;
                    FuelVeh(player, vehEntity);
                    if (NAPI.Data.HasEntityData(vehEntity, "PENDING_FUEL"))
                    {
                        NAPI.Data.SetEntityData(player, "FUELING_VEHICLE", vehEntity);
                        veh.FuelingTimer = new System.Threading.Timer(FuelVehTimer, new object[] { player, vehEntity }, 3000, 3000);
                        return;
                    }
                }
                else
                {
                    NAPI.Chat.SendChatMessageToPlayer(player, "You must be driving a vehicle.");
                }
            }
            else
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be at a gas station.");
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Client player, Vehicle vehicle)
        {
            if (NAPI.Data.HasEntityData(player, "FUELING_VEHICLE"))
            {
                var vehEntity = NAPI.Data.GetEntityData(player, "FUELING_VEHICLE");
                NAPI.Chat.SendChatMessageToPlayer(player, "Refuel ended.");
                GameVehicle veh = NAPI.Data.GetEntityData(vehEntity, "Vehicle");
                veh.FuelingTimer?.Dispose();
                NAPI.Player.FreezePlayer(player, false);
                veh.Save();
            }
        }

        private void FuelVehTimer(object obj)
        {
            var t = (object[])obj;
            FuelVeh((Client)t[0], (Vehicle)t[1]);
        }

        private void FuelVeh(Client playerEntity, Vehicle vehEntity)
        {
            if (vehEntity.IsNull)
            {
                return;
            }

            GameVehicle veh = NAPI.Data.GetEntityData(vehEntity, "Vehicle");

            if (veh == null)
            {
                return;
            }

            if (playerEntity == null)
            {
                veh.FuelingTimer?.Dispose();
                NAPI.Data.ResetEntityData(vehEntity, "PENDING_FUEL");
                return;
            }

            Character c = playerEntity.GetCharacter();

            if (c == null)
            {
                veh.FuelingTimer?.Dispose();
                NAPI.Data.ResetEntityData(vehEntity, "PENDING_FUEL");
                return;
            }

            if (NAPI.Vehicle.GetVehicleEngineStatus(vehEntity))
            {
                veh.FuelingTimer?.Dispose();
                NAPI.Data.ResetEntityData(vehEntity, "PENDING_FUEL");
                NAPI.Data.ResetEntityData(playerEntity, "FUELING_VEHICLE");
                NAPI.Player.FreezePlayer(playerEntity, false);
                NAPI.Chat.SendChatMessageToPlayer(playerEntity, "Refuel has been cancelled cause the engine has turned on.");
                veh.Save();
                return;
            }

            int pendingFuel = NAPI.Data.GetEntityData(vehEntity, "PENDING_FUEL") ?? 0;

            if (pendingFuel <= 0 || veh.RefuelProp.Supplies <= 0)
            {
                NAPI.ClientEvent.TriggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
                veh.FuelingTimer?.Dispose();
                NAPI.Data.ResetEntityData(vehEntity, "PENDING_FUEL");
                NAPI.Data.ResetEntityData(playerEntity, "FUELING_VEHICLE");
                NAPI.Player.FreezePlayer(playerEntity, false);

                if(veh.RefuelProp.Supplies <= 0)
                    NAPI.Chat.SendChatMessageToPlayer(playerEntity, "The gas station ran out of gas.");
                else if (pendingFuel <= 0)
                    NAPI.Chat.SendChatMessageToPlayer(playerEntity, "Refueling finished.");

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

            NAPI.ClientEvent.TriggerClientEvent(playerEntity, "fuel_updatevalue", veh.Fuel);
            NAPI.Data.SetEntityData(vehEntity, "PENDING_FUEL", pendingFuel);
        }
    }
}
