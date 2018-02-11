using System;
using System.Linq;

using GTANetworkAPI;

using mtgvrp.core;
using mtgvrp.group_manager;
using mtgvrp.job_manager;
using mtgvrp.player_manager;
using mtgvrp.core.Help;
using mtgvrp.vehicle_manager.modding;

namespace mtgvrp.vehicle_manager.vehicle_editor
{
    class VehicleEditor : Script
    {

        public VehicleEditor()
        {
        }

        [RemoteEvent("vehicle_edit_change_spawn")]
        private void VehicleEditChangeSpawn(Client player, params object[] arguments)
        {
            GameVehicle veh = API.GetEntityData(player.Handle, "EDIT_VEH");

            if (veh == null)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                return;
            }

            veh.SpawnPos = API.GetEntityPosition(veh.NetHandle);
            veh.SpawnRot = API.GetEntityRotation(veh.NetHandle);
            veh.SpawnDimension = (int)API.GetEntityDimension(veh.NetHandle);
            veh.Save();

            API.SendChatMessageToPlayer(player, "Vehicle position spawn saved to current location.");
        }

        [RemoteEvent("vehicle_edit_save")]
        private void VehicleEditSave(Client player, params object[] arguments)
        {
            GameVehicle veh = API.GetEntityData(player.Handle, "EDIT_VEH");

            if (veh == null)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                return;
            }

            var vehId = Convert.ToInt32(arguments[0]);
            var model = Convert.ToString(arguments[1]);
            var owner = Convert.ToString(arguments[2]);
            var licensePlate = Convert.ToString(arguments[3]);
            var color1 = (string)arguments[4];
            var color2 = (string)arguments[5];
            var respawnDelay = Convert.ToInt32(arguments[6]);
            var jobId = Convert.ToInt32(arguments[7]);
            var groupId = Convert.ToInt32(arguments[8]);

            if (veh.Id != vehId)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle editor error. Vehicle edit IDs are not equal.");
                return;
            }

            var modelHash = API.VehicleNameToModel(model);

            if (modelHash == 0)
            {
                API.TriggerClientEvent(player, "send_veh_edit_error", "Invalid vehicle model entered!");
                return;
            }

            if (!Character.IsCharacterRegistered(owner) && owner != "NONE")
            {
                API.TriggerClientEvent(player, "send_veh_edit_error",
                    "Invalid owner entered. (Character name does not exist.)");
                return;
            }

            if (JobManager.GetJobById(jobId) == null && jobId != 0)
            {
                API.TriggerClientEvent(player, "send_veh_edit_error", "Invalid job ID entered!");
                return;
            }

            if (GroupManager.GetGroupById(groupId) == null && groupId != 0)
            {
                API.TriggerClientEvent(player, "send_veh_edit_error", "Invalid group ID entered!");
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
            veh.VehMods[ModdingManager.PrimaryColorId.ToString()] = color1;
            veh.VehMods[ModdingManager.SecondryColorId.ToString()] = color2;
            veh.RespawnDelay = TimeSpan.FromMinutes(respawnDelay);
            veh.JobId = jobId;
            veh.Job = JobManager.GetJobById(veh.JobId);
            veh.Group = GroupManager.GetGroupById(veh.GroupId);
            veh.GroupId = groupId;

            VehicleManager.respawn_vehicle(veh, API.GetEntityPosition(veh.NetHandle));
            API.SetPlayerIntoVehicle(player, veh.NetHandle, -1);

            veh.Save();
            API.SendChatMessageToPlayer(player, "Vehicle editor changes saved!");
            API.TriggerClientEvent(player, "finish_veh_edit");
        }

        [RemoteEvent("cancel_veh_edit")]
        private void CancelVehEdit(Client player, params object[] arguments)
        {
            API.ResetEntityData(player, "EDIT_VEH");
            API.SendChatMessageToPlayer(player, "~r~Vehicle editing canceled.");
        }

        [RemoteEvent("edit_veh_delete")]
        private void EditVehDelete(Client player, params object[] arguments)
        {
            GameVehicle veh = API.GetEntityData(player.Handle, "EDIT_VEH");

            if (veh == null)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                return;
            }

            VehicleManager.despawn_vehicle(veh);
            veh.Delete();
            API.SendChatMessageToPlayer(player, "Vehicle deleted successfully!");
            API.TriggerClientEvent(player, "finish_veh_edit");
            API.ResetEntityData(player, "EDIT_VEH");
        }

        [RemoteEvent("vehicle_edit_respawn")]
        private void VehicleEditRespawn(Client player, params object[] arguments)
        {
            GameVehicle veh = API.GetEntityData(player.Handle, "EDIT_VEH");

            if (veh == null)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle editor error. Null vehicle.");
                return;
            }

            VehicleManager.respawn_vehicle(veh);
            API.ResetEntityData(player, "EDIT_VEH");
            API.SendChatMessageToPlayer(player, "~g~Vehicle respawned!");
            API.TriggerClientEvent(player, "finish_veh_edit");
        }

        [Command("editvehicle"), Help(HelpManager.CommandGroups.AdminLevel4, "Editing different stats of a vehicle", null)]
        public void editvehicle_cmd(Client player)
        {
            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            if (!API.IsPlayerInAnyVehicle(player))
            {
                API.SendPictureNotificationToPlayer(player, "You must be in a vehicle to use this command.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            var veh = VehicleManager.GetVehFromNetHandle(API.GetPlayerVehicle(player));

            if(veh == null)
            {
                API.SendChatMessageToPlayer(player, "~r~Vehicle is null!");
                return;
            }

            if(veh.is_saved() == false)
            {
                API.SendPictureNotificationToPlayer(player, "Only a saved vehicle may be edited.", "CHAR_BLOCKED", 0, 1, "Server", "~r~Command Error");
                return;
            }

            API.SetEntityData(player.Handle, "EDIT_VEH", veh);
            API.TriggerClientEvent(player, "show_vehicle_edit_menu", veh.Id, veh.VehModel.ToString(), (veh.OwnerId == 0 ? "NONE" : PlayerManager.Players.Single(x => x.Id == veh.OwnerId).CharacterName), veh.LicensePlate, veh.VehMods[ModdingManager.PrimaryColorId.ToString()], veh.VehMods[ModdingManager.SecondryColorId.ToString()], veh.RespawnDelay.TotalMinutes.ToString("G"), veh.JobId, veh.GroupId);
        }
    }
}
