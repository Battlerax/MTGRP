using System;
using System.Linq;
using GTANetworkServer;
using mtgvrp.group_manager;
using mtgvrp.job_manager;
using mtgvrp.player_manager;
using mtgvrp.core.Help;

namespace mtgvrp.vehicle_manager.vehicle_editor
{
    class VehicleEditor : Script
    {

        public VehicleEditor()
        {
            API.onClientEventTrigger += OnClientEventTrigger;
        }

        private void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            
            if(eventName == "vehicle_edit_change_spawn")
            {
                Vehicle veh = API.getEntityData(player.handle, "EDIT_VEH");

                if (veh == null)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                    return;
                }

                veh.SpawnPos = API.getEntityPosition(veh.NetHandle);
                veh.SpawnRot = API.getEntityRotation(veh.NetHandle);
                veh.SpawnDimension = API.getEntityDimension(veh.NetHandle);
                veh.Save();

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

                var vehId = Convert.ToInt32(arguments[0]);
                var model = Convert.ToString(arguments[1]);
                var owner = Convert.ToString(arguments[2]);
                var licensePlate = Convert.ToString(arguments[3]);
                var color1 = Convert.ToInt32(arguments[4]);
                var color2 = Convert.ToInt32(arguments[5]);
                var respawnDelay = Convert.ToInt32(arguments[6]);
                var jobId = Convert.ToInt32(arguments[7]);
                var groupId = Convert.ToInt32(arguments[8]);

                if (veh.Id != vehId)
                {
                    API.sendChatMessageToPlayer(player, "~r~Vehicle editor error. Vehicle edit IDs are not equal.");
                    return;
                }

                var modelHash = API.vehicleNameToModel(model);

                if(modelHash == 0)
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid vehicle model entered!");
                    return;
                }

                if (!Character.IsCharacterRegistered(owner) && owner != "NONE")
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid owner entered. (Character name does not exist.)");
                    return;
                }
                 
                if (JobManager.GetJobById(jobId) == null && jobId != 0)
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid job ID entered!");
                    return;
                }

                if (GroupManager.GetGroupById(groupId) == null && groupId != 0)
                {
                    API.triggerClientEvent(player, "send_veh_edit_error", "Invalid group ID entered!");
                    return;
                }

                if (owner == "NONE")
                {
                    veh.OwnerId = 0;
                }
                else
                {
                    veh.OwnerId = PlayerManager.Players.Single(x => x.CharacterName == owner).Id;
                }

                veh.VehModel = modelHash;
                veh.LicensePlate = licensePlate;
                veh.SpawnColors[0] = color1;
                veh.SpawnColors[1] = color2;
                veh.RespawnDelay = respawnDelay;
                veh.JobId = jobId;
                veh.Job = JobManager.GetJobById(veh.JobId);
                veh.Group = GroupManager.GetGroupById(veh.GroupId);
                veh.GroupId = groupId;

                VehicleManager.respawn_vehicle(veh, API.getEntityPosition(veh.NetHandle));
                API.setPlayerIntoVehicle(player, veh.NetHandle, -1);

                veh.Save();
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

        [Command("editvehicle"), Help(HelpManager.CommandGroups.AdminLevel4, "Editing different stats of a vehicle", null)]
        public void editvehicle_cmd(Client player)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 4)
                return;

            if (!API.isPlayerInAnyVehicle(player))
            {
                API.sendPictureNotificationToPlayer(player, "You must be in a vehicle to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            var veh = VehicleManager.GetVehFromNetHandle(API.getPlayerVehicle(player));

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
            API.triggerClientEvent(player, "show_vehicle_edit_menu", veh.Id, API.getVehicleDisplayName(veh.VehModel), (veh.OwnerId == 0 ? "NONE" : PlayerManager.Players.Single(x => x.Id == veh.OwnerId).CharacterName), veh.LicensePlate, veh.SpawnColors[0], veh.SpawnColors[1], veh.RespawnDelay, veh.JobId, veh.GroupId);
        }
    }
}
