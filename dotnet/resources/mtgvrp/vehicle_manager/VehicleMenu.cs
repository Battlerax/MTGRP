


using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.player_manager;

namespace mtgvrp.vehicle_manager
{
    class VehicleMenu : Script
    {
        public VehicleMenu()
        {
            DebugManager.DebugMessage("[VehicleMenu] Vehicle Menu initalized.");
        }

        [RemoteEvent("OnVehicleMenuTrigger")]
        public void OnVehicleMenuTrigger(Player player, params object[] arguments)
        {
            var vehicleHandle = (Entity)arguments[0];
            var option = (string)arguments[1];

            Character character = player.GetCharacter();
            GameVehicle vehicle = API.Shared.GetEntityData(vehicleHandle, "Vehicle");

            var playerSeat = API.Shared.GetPlayerVehicleSeat(player);

            //Check that player vehicle is the same as the menu vehicle...
            if(API.Shared.GetPlayerVehicle(player) != vehicleHandle)
            {
                DebugManager.DebugMessage("[VehicleMenu] " + character.CharacterName + "(" + player.SocialClubName + ", " + player + ") used VehicleMenu option in a different vehicle handle.");
                return;
            }

            var vehAccess = VehicleManager.DoesPlayerHaveVehicleAccess(player, vehicle);
            var parkAccess = VehicleManager.DoesPlayerHaveVehicleParkLockAccess(player, vehicle);

            if ((option.Equals("park") || option.Equals("lock")) && !parkAccess)
            {
                API.Shared.SendChatMessageToPlayer(player, "~r~ You do not have access to this vehicle.");
                return;
            }

            if((option.Equals("engine") || option.Equals("park")) && playerSeat != -1)
            {
                API.Shared.SendChatMessageToPlayer(player, "~r~ You can only access these options in the driver seat.");
                return;
            }

            switch (option)
            {
                case "engine":
                    if (vehAccess)
                    {
                        VehicleManager.engine_cmd(player);
                    }
                    else
                    {
                        VehicleManager.hotwire_cmd(player);
                    }
                    break;
                case "lock":
                    var lockState = API.Shared.GetVehicleLocked(vehicleHandle);

                    if(lockState)
                    {
                        API.Shared.SetVehicleLocked(vehicleHandle, false);
                        ChatManager.RoleplayMessage(character, "unlocks the doors of the vehicle.", ChatManager.RoleplayMe);
                    }
                    else
                    {
                        API.Shared.SetVehicleLocked(vehicleHandle, true);
                        ChatManager.RoleplayMessage(character, "locks the doors of the vehicle.", ChatManager.RoleplayMe);
                    }


                    break;
                case "park":

                    var pos = NAPI.Entity.GetEntityPosition(vehicleHandle);
                    var rot = API.GetEntityRotation(vehicleHandle);
                    var dimension = API.GetEntityDimension(vehicleHandle);

                    vehicle.SpawnPos = pos;
                    vehicle.SpawnRot = rot;
                    vehicle.SpawnDimension = (int)dimension;

                    vehicle.Save();

                    API.Shared.SendChatMessageToPlayer(player, "Car spawn location saved to current location.");
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

                    var doorState = API.GetVehicleDoorState(vehicleHandle, doorIndex);

                    if(doorState)
                    {
                        API.SetVehicleDoorState(vehicleHandle, doorIndex, false);
                        ChatManager.RoleplayMessage(character, "closed the " + doorName + " of the vehicle.", ChatManager.RoleplayMe);
                    }
                    else
                    {
                        API.SetVehicleDoorState(vehicleHandle, doorIndex, true);
                        ChatManager.RoleplayMessage(character, "opened the " + doorName + " of the vehicle.", ChatManager.RoleplayMe);
                    }

                    break;
            }
        }
    }
}
