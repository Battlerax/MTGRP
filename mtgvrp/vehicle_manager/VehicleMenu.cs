using System;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.player_manager;

namespace mtgvrp.vehicle_manager
{
    class VehicleMenu : Script
    {
        public VehicleMenu()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            DebugManager.DebugMessage("[VehicleMenu] Vehicle Menu initalized.");
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "OnVehicleMenuTrigger":
                    var vehicleHandle = (NetHandle)arguments[0];
                    var option = (string)arguments[1];

                    Character character = API.shared.getEntityData(player.handle, "Character");
                    Vehicle vehicle = API.shared.getEntityData(vehicleHandle, "Vehicle");

                    var playerSeat = API.shared.getPlayerVehicleSeat(player);

                    //Check that player vehicle is the same as the menu vehicle...
                    if(API.shared.getPlayerVehicle(player) != vehicleHandle)
                    {
                        DebugManager.DebugMessage("[VehicleMenu] " + character.CharacterName + "(" + player.socialClubName + ", " + player.handle + ") used VehicleMenu option in a different vehicle handle.");
                        return;
                    }

                    var vehAccess = VehicleManager.DoesPlayerHaveVehicleAccess(player, vehicle);

                    if (option.Equals("park") && !vehAccess)
                    {
                        API.shared.sendChatMessageToPlayer(player, "~r~ You do not have access to this vehicle.");
                        return;
                    }

                    if((option.Equals("engine") || option.Equals("park")) && playerSeat != -1)
                    {
                        API.shared.sendChatMessageToPlayer(player, "~r~ You can only access these options in the driver seat.");
                        return;
                    }

                    switch (option)
                    {
                        case "engine":

                            var engineState = API.shared.getVehicleEngineStatus(vehicleHandle);

                            if (!engineState)
                            {
                                if (vehicle.Fuel <= 0)
                                {
                                    API.sendChatMessageToPlayer(player, "The vehicle has no fuel.");
                                    return;
                                }
                            }

                            if (vehAccess)
                            {
                                if (engineState)
                                {
                                    API.shared.setVehicleEngineStatus(vehicleHandle, false);
                                    ChatManager.RoleplayMessage(character, "turns off the vehicle engine.", ChatManager.RoleplayMe);
                                }
                                else
                                {
                                    API.shared.setVehicleEngineStatus(vehicleHandle, true);
                                    ChatManager.RoleplayMessage(character, "turns on the vehicle engine.", ChatManager.RoleplayMe);
                                }
                            }
                            else
                            {
                                if (engineState)
                                {
                                    API.shared.setVehicleEngineStatus(vehicleHandle, false);
                                    ChatManager.RoleplayMessage(character, "turns off the vehicle engine.", ChatManager.RoleplayMe);
                                }
                                else
                                {
                                    var ran = new Random();

                                    var hotwireChance = ran.Next(100);

                                    ChatManager.RoleplayMessage(character, player.GetCharacter().CharacterName + " attempts to hotwire the vehicle.", ChatManager.RoleplayMe);

                                    if (hotwireChance < 40)
                                    {
                                        API.shared.setVehicleEngineStatus(vehicleHandle, true);
                                        ChatManager.RoleplayMessage(character, "successfully hotwires the vehicle.", ChatManager.RoleplayMe);
                                    }
                                    else
                                    {
                                        API.setPlayerHealth(player, player.health - 10);
                                        player.sendChatMessage("You attempted to hotwire the vehicle and got shocked!");
                                        ChatManager.RoleplayMessage(character, player.GetCharacter().CharacterName + " failed to hotwire the vehicle.", ChatManager.RoleplayMe);
                                    }
                                }
                            }
                            break;
                        case "lock":
                            var lockState = API.shared.getVehicleLocked(vehicleHandle);

                            if(lockState)
                            {
                                API.shared.setVehicleLocked(vehicleHandle, false);
                                ChatManager.RoleplayMessage(character, "unlocks the doors of the vehicle.", ChatManager.RoleplayMe);
                            }
                            else
                            {
                                API.shared.setVehicleLocked(vehicleHandle, true);
                                ChatManager.RoleplayMessage(character, "locks the doors of the vehicle.", ChatManager.RoleplayMe);
                            }


                            break;
                        case "park":

                            var pos = API.getEntityPosition(vehicleHandle);
                            var rot = API.getEntityRotation(vehicleHandle);
                            var dimension = API.getEntityDimension(vehicleHandle);

                            vehicle.SpawnPos = pos;
                            vehicle.SpawnRot = rot;
                            vehicle.SpawnDimension = dimension;

                            vehicle.Save();

                            API.shared.sendChatMessageToPlayer(player, "Car spawn location saved to current location.");
                            break;
                        case "door":
                            var doorIndex = (int)arguments[2];

                            var doorName = "";
                            switch (doorIndex)
                            {
                                case 0:
                                    doorName = "front left door";
                                    break;
                                case 1:
                                    doorName = "front right door";
                                    break;
                                case 2:
                                    doorName = "back left door";
                                    break;
                                case 3:
                                    doorName = "back right door";
                                    break;
                                case 4:
                                    doorName = "hood";
                                    break;
                                case 5:
                                    doorName = "trunk";
                                    break;
                            }

                            var doorState = API.getVehicleDoorState(vehicleHandle, doorIndex);

                            if(doorState)
                            {
                                API.setVehicleDoorState(vehicleHandle, doorIndex, false);
                                ChatManager.RoleplayMessage(character, "closed the " + doorName + " of the vehicle.", ChatManager.RoleplayMe);
                            }
                            else
                            {
                                API.setVehicleDoorState(vehicleHandle, doorIndex, true);
                                ChatManager.RoleplayMessage(character, "opened the " + doorName + " of the vehicle.", ChatManager.RoleplayMe);
                            }

                            break;
                    }
                    break;
            }
        }
    }
}
