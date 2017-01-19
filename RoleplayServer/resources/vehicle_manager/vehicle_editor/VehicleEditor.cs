using System;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer
{
    class VehicleEditor : Script
    {

        public VehicleEditor()
        {
            API.onClientEventTrigger += onClientEventTrigger;
        }

        private void onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            
            if(eventName == "vehicle_edit_change_spawn")
            {
                Vehicle veh = API.getEntityData(player.handle, "EDIT_VEH");

                if (veh == null)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                    return;
                }

                veh.spawn_pos = API.getEntityPosition(veh.net_handle);
                veh.spawn_rot = API.getEntityRotation(veh.net_handle);
                veh.spawn_dimension = API.getEntityDimension(veh.net_handle);
                veh.save();

                API.sendChatMessageToPlayer(player, "Vehicle position spawn saved to current location.");
            }
            else if(eventName == "vehicle_edit_save")
            {
                Vehicle veh = API.getEntityData(player.handle, "EDIT_VEH");

                if (veh == null)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                    return;
                }

                var veh_id = Convert.ToInt32(arguments[0]);
                var model = Convert.ToString(arguments[1]);
                var owner = Convert.ToString(arguments[2]);
                var license_plate = Convert.ToString(arguments[3]);
                var color_1 = Convert.ToInt32(arguments[4]);
                var color_2 = Convert.ToInt32(arguments[5]);
                var respawn_delay = Convert.ToInt32(arguments[6]);
                var job_id = Convert.ToInt32(arguments[7]);

                if (veh._id != veh_id)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Vehicle edit IDs are not equal.");
                    return;
                }

                VehicleHash model_hash = API.vehicleNameToModel((string)model);

                if(model_hash == 0)
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid vehicle model entered!");
                    return;
                }

                if (!Character.IsCharacterRegistered(owner) && owner != "None")
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid owner entered. (Character name does not exist.)");
                    return;
                }
                                        
                if (JobManager.getJobById(job_id) == null && job_id != 0)
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid job ID entered!");
                    return;
                }

                veh.veh_model = model_hash;
                veh.owner_name = owner;
                veh.license_plate = license_plate;
                veh.spawn_colors[0] = color_1;
                veh.spawn_colors[1] = color_2;
                veh.respawn_delay = respawn_delay;
                veh.job_id = job_id;
                veh.job = JobManager.getJobById(veh.job_id);

                VehicleManager.respawn_vehicle(veh, API.getEntityPosition(veh.net_handle));
                API.setPlayerIntoVehicle(player, veh.net_handle, -1);

                veh.save();
                API.sendChatMessageToPlayer(player, "Vehicle editor changes saved!");
                API.triggerClientEvent(player, "finish_veh_edit");
            }
            else if(eventName == "cancel_veh_edit")
            {
                API.resetEntityData(player, "EDIT_VEH");
                API.sendChatMessageToPlayer(player, "~r~Vehicle editing canceled.");
            }
            else if(eventName == "vehicle_edit_respawn")
            {
                Vehicle veh = API.getEntityData(player.handle, "EDIT_VEH");

                if (veh == null)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                    return;
                }

                VehicleManager.respawn_vehicle(veh);
                API.resetEntityData(player, "EDIT_VEH");
                API.sendChatMessageToPlayer(player, "~g~Vehicle respawned!");
                API.triggerClientEvent(player, "finish_veh_edit");
            }
        }

        [Command("editvehicle")]
        public void editvehicle_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.admin_level < 4)
                return;

            if (!API.isPlayerInAnyVehicle(player))
            {
                API.sendPictureNotificationToPlayer(player, "You must be in a vehicle to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            Vehicle veh = VehicleManager.getVehFromNetHandle(API.getPlayerVehicle(player));

            if(veh == null)
            {
                API.sendChatMessageToPlayer(player, "~r~Vehicle is null!");
                return;
            }

            if(veh.is_saved() == false)
            {
                API.sendPictureNotificationToPlayer(player, "Only a saved vehicle may be edited.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            API.setEntityData(player.handle, "EDIT_VEH", veh);
            API.triggerClientEvent(player, "show_vehicle_edit_menu", veh._id, API.getVehicleDisplayName(veh.veh_model), veh.owner_name, veh.license_plate, veh.spawn_colors[0], veh.spawn_colors[1], veh.respawn_delay, veh.job_id);
        }
    }
}
