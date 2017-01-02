using System;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer
{
    class VehicleMenu : Script
    {
        public VehicleMenu()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
            DebugManager.debugMessage("[VehicleMenu] Vehicle Menu initalized.");
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "OnVehicleMenuTrigger":
                    NetHandle vehicle_handle = (NetHandle)(arguments[0]);
                    string option = (string)(arguments[1]);

                    Character character = API.shared.getEntityData(player.handle, "Character");
                    Vehicle vehicle = API.shared.getEntityData(vehicle_handle, "Vehicle");

                    int player_seat = API.shared.getPlayerVehicleSeat(player);

                    //Check that player vehicle is the same as the menu vehicle...
                    if(API.shared.getPlayerVehicle(player) != vehicle_handle)
                    {
                        DebugManager.debugMessage("[VehicleMenu] " + character.character_name + "(" + player.socialClubName + ", " + player.handle + ") used VehicleMenu option in a different vehicle handle.");
                        return;
                    }

                    bool veh_access = VehicleManager.DoesPlayerHaveVehicleAccess(player, vehicle);

                    if ((option.Equals("park")) && !veh_access)
                    {
                        API.shared.sendChatMessageToPlayer(player, "~r~ You do not have access to this vehicle.");
                        return;
                    }

                    if((option.Equals("engine") || option.Equals("park")) && player_seat != -1)
                    {
                        API.shared.sendChatMessageToPlayer(player, "~r~ You can only access these options in the driver seat.");
                        return;
                    }

                    switch (option)
                    {
                        case "engine":

                            bool engine_state = API.shared.getVehicleEngineStatus(vehicle_handle);

                            if (veh_access)
                            {
                                if (engine_state == true)
                                {
                                    API.shared.setVehicleEngineStatus(vehicle_handle, false);
                                    ChatManager.RoleplayMessage(character, "turns off the vehicle engine.", ChatManager.ROLEPLAY_ME, 10);
                                }
                                else
                                {
                                    API.shared.setVehicleEngineStatus(vehicle_handle, true);
                                    ChatManager.RoleplayMessage(character, "turns on the vehicle engine.", ChatManager.ROLEPLAY_ME, 10);
                                }
                            }
                            else
                            {
                                if (engine_state == true)
                                {
                                    API.shared.setVehicleEngineStatus(vehicle_handle, false);
                                    ChatManager.RoleplayMessage(character, "turns off the vehicle engine.", ChatManager.ROLEPLAY_ME, 10);
                                }
                                else
                                {
                                    Random ran = new Random();

                                    int hotwire_chance = ran.Next(100);

                                    if (hotwire_chance < 40)
                                    {
                                        API.shared.setVehicleEngineStatus(vehicle_handle, true);
                                        ChatManager.RoleplayMessage(character, "successfully hotwires the vehicle.", ChatManager.ROLEPLAY_ME, 10);
                                    }
                                    else
                                    {
                                        API.shared.sendChatMessageToPlayer(player, "You failed to hotwire the vehicle.");
                                    }
                                }
                            }
                            break;
                        case "lock":
                            bool lock_state = API.shared.getVehicleLocked(vehicle_handle);

                            if(lock_state == true)
                            {
                                API.shared.setVehicleLocked(vehicle_handle, false);
                                ChatManager.RoleplayMessage(character, "unlocks the doors of the vehicle.", ChatManager.ROLEPLAY_ME, 10);
                            }
                            else
                            {
                                API.shared.setVehicleLocked(vehicle_handle, true);
                                ChatManager.RoleplayMessage(character, "locks the doors of the vehicle.", ChatManager.ROLEPLAY_ME, 10);
                            }


                            break;
                        case "park":

                            Vector3 pos = API.getEntityPosition(vehicle_handle);
                            Vector3 rot = API.getEntityRotation(vehicle_handle);
                            int dimension = API.getEntityDimension(vehicle_handle);

                            vehicle.spawn_pos = pos;
                            vehicle.spawn_rot = rot;
                            vehicle.spawn_dimension = dimension;

                            vehicle.save();

                            API.shared.sendChatMessageToPlayer(player, "Car spawn location saved to current location.");
                            break;
                        case "door":
                            int door_index = (int)arguments[2];

                            string door_name = "";
                            switch (door_index)
                            {
                                case 0:
                                    door_name = "front left door";
                                    break;
                                case 1:
                                    door_name = "front right door";
                                    break;
                                case 2:
                                    door_name = "back left door";
                                    break;
                                case 3:
                                    door_name = "back right door";
                                    break;
                                case 4:
                                    door_name = "hood";
                                    break;
                                case 5:
                                    door_name = "trunk";
                                    break;
                            }

                            bool door_state = API.getVehicleDoorState(vehicle_handle, door_index);

                            if(door_state == true)
                            {
                                API.setVehicleDoorState(vehicle_handle, door_index, false);
                                ChatManager.RoleplayMessage(character, "closed the " + door_name + " of the vehicle.", ChatManager.ROLEPLAY_ME, 10);
                            }
                            else
                            {
                                API.setVehicleDoorState(vehicle_handle, door_index, true);
                                ChatManager.RoleplayMessage(character, "opened the " + door_name + " of the vehicle.", ChatManager.ROLEPLAY_ME, 10);
                            }

                            break;
                    }
                    break;
            }
        }
    }
}
