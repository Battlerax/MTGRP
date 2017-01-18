/*
 *  File: VehicleManager.cs
 *  Author: Chenko
 *  Date: 12/24/2016
 * 
 * 
 *  Purpose: Loads vehicles from the database and provides functions to manage them
 * 
 * 
 * */


using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using MongoDB.Driver;

namespace RoleplayServer
{
    public class VehicleManager : Script
    {
        public static List<Vehicle> vehicles = new List<Vehicle>();
       
        /*
        * 
        * ========== CONSTRUCTOR =========
        * 
        */

        public VehicleManager()
        {
            DebugManager.debugMessage("[VehicleM] Initilizing vehicle manager...");

            // Register callbacks
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onVehicleDeath += OnVehicleDeath;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;

            // Create vehicle table + 
            load_all_unowned_vehicles();

            DebugManager.debugMessage("[VehicleM] Vehicle Manager initalized!");
        }

        /*
        * 
        * ========== COMMANDS =========
        * 
        */

        [Command("spawnveh")]
        public void spawnveh_cmd(Client player, VehicleHash model, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            Vector3 pos = player.position;
            Vector3 rot = player.rotation;

            Vehicle veh = createUnownedVehicle(model, pos, rot, color1, color2, dimension);
            spawn_vehicle(veh);
            
            API.setPlayerIntoVehicle(player, veh.net_handle, -1);
            API.setVehicleEngineStatus(veh.net_handle, true);

            API.sendChatMessageToPlayer(player, "You have spawned a " + model);
            API.sendChatMessageToPlayer(player, "This vehicle is unsaved and may behave incorrectly.");
            return;
        }

        [Command("savevehicle")]
        public void savevehicle_cmd(Client player)
        {
            NetHandle veh_handle = API.getPlayerVehicle(player);
            Vehicle veh = getVehFromNetHandle(veh_handle);

            if(veh == null)
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: You are not inside a vehicle.");
                return;
            }

            if(veh.veh_type != Vehicle.VEH_TYPE_TEMP)
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: You must be inside a temporary vehicle to save it.");
                return;
            }

            if(veh.is_saved() == true)
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: This vehicle is already saved into the database.");
                return;
            }

            
            veh.insert();

            veh.veh_type = Vehicle.VEH_TYPE_PERM;
            veh.save();

            API.sendChatMessageToPlayer(player, "You have saved vehicle " + vehicles.IndexOf(veh) + ". (SQL: " + veh._id + ")");
            return;
        }

        [Command("savepos")]
        public void savepos_cmd(Client player, int i)
        {
            Vector3 pos = player.position;
            Vector3 rot = player.rotation;

            API.consoleOutput(i + " " + pos.ToString() + " " + rot.ToString());
            API.sendNotificationToPlayer(player, "Saved");
        }

        [Command("tele")]
        public void tele(Client player)
        {
            API.setEntityPosition(player.handle, new Vector3(403, -996, -99));
            API.sendChatMessageToPlayer(player, "teleported");
        }

        /*
        * 
        * ========== CALLBACKS =========
        * 
        */

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicle_handle)
        {
            // Admin check in future

            Vehicle veh = getVehFromNetHandle(vehicle_handle);
            API.setBlipTransparency(veh.blip, 0);


            Character character = API.getEntityData(player.handle, "Character");

            API.sendChatMessageToPlayer(player, "~w~[VehicleM] You have entered vehicle ~r~" + vehicles.IndexOf(veh) + "(Owned by: " + veh.owner_name + ")");

            API.sendChatMessageToPlayer(player, "~y~ Press \"N\" on your keyboard to access the vehicle menu.");

            //Vehicle Interaction Menu Setup
            string veh_info = API.getVehicleDisplayName(veh.veh_model) + " - " + veh.license_plate;
            API.setEntitySyncedData(player.handle, "CurrentVehicleInfo", veh_info);
            API.setEntitySyncedData(player.handle, "OwnsVehicle", (bool)(DoesPlayerHaveVehicleAccess(player, veh)));

            if (API.getPlayerVehicleSeat(player) == -1)
            {
                veh.driver = character;
            }
        }

        public void OnPlayerExitVehicle(Client player, NetHandle vehicle_handle)
        {
            Vehicle veh = getVehFromNetHandle(vehicle_handle);
            API.setBlipTransparency(veh.blip, 100);

            if(veh == null)
            {
                DebugManager.debugMessage("[VehicleVM] OnPlayerExitVehicle received null Vehicle.");
                return;
            }

            if (veh.veh_type == Vehicle.VEH_TYPE_TEMP)
            {
                despawn_vehicle(veh);
                delete_vehicle(veh);

                API.sendNotificationToPlayer(player, "Your vehicle was deleted on exit because it was temporary.", false);
            }

            if (veh.driver == API.getEntityData(player, "Character"))
                veh.driver = null;
        }

        public void OnVehicleDeath(NetHandle vehicle_handle)
        {
            Vehicle veh = getVehFromNetHandle(vehicle_handle);
            API.consoleOutput("Vehicle " + vehicle_handle + " died");
            API.delay(veh.respawn_delay, true, () =>
            {
                respawn_vehicle(veh);
            });
        }

        /*
        * 
        * ========== FUNCTIONS =========
        * 
        */

        public Vehicle createUnownedVehicle(VehicleHash model, Vector3 pos, Vector3 rot, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            Vehicle veh = new Vehicle();

            veh.veh_model = model;
            veh.spawn_pos = pos;
            veh.spawn_rot = rot;
            veh.spawn_colors[0] = color1;
            veh.spawn_colors[1] = color2;
            veh.spawn_dimension = dimension;
            veh.license_plate = "ABC123";

            vehicles.Add(veh);

            return veh;
        }

        public void delete_vehicle(Vehicle veh)
        {
            vehicles.Remove(veh);
        }

        public static Vehicle getVehFromNetHandle(NetHandle handle)
        {
            return API.shared.getEntityData(handle, "Vehicle");
        }

        public int spawn_vehicle(Vehicle veh, Vector3 pos)
        {
            int return_code = veh.spawn(pos);
 
            if (return_code == 1)
            {
                API.setEntityData(veh.net_handle, "Vehicle", veh);
            }
            API.setVehicleEngineStatus(veh.net_handle, false);
            return return_code;
        }

        public int spawn_vehicle(Vehicle veh)
        {
            return spawn_vehicle(veh, veh.spawn_pos);
        }

        public int despawn_vehicle(Vehicle veh)
        {
            int return_code = veh.despawn();

            if (return_code == 1)
            {
                API.resetEntityData(veh.net_handle, "Vehicle");
            }

            return return_code;
        }

        public int respawn_vehicle(Vehicle veh, Vector3 pos)
        {
            if (API.hasEntityData(veh.net_handle, "Vehicle"))
            {
                despawn_vehicle(veh);
            }
            return spawn_vehicle(veh, pos);
        }

        public int respawn_vehicle(Vehicle veh)
        {
            return respawn_vehicle(veh, veh.spawn_pos);
        }

        public static bool DoesPlayerHaveVehicleAccess(Client player, Vehicle vehicle)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (account.admin_level >= 3) { return true; }
            if (character._id == vehicle.owner_id) { return true; }
            //faction check
            if(character.job_one == vehicle.job) { return true; }
            //gang check

            return false;
        }

        public void load_all_unowned_vehicles()
        {
            FilterDefinition<Vehicle> filter = Builders<Vehicle>.Filter.Eq("owner_name", "None");
            List<Vehicle> unowned_vehicles = DatabaseManager.vehicle_table.Find<Vehicle>(filter).ToList<Vehicle>();

            foreach (Vehicle v in unowned_vehicles)
            {
                v.job = JobManager.getJobById(v.job_id);
                spawn_vehicle(v);
                vehicles.Add(v);
            }

            DebugManager.debugMessage("Loaded " + unowned_vehicles.Count + " unowned vehicles from the database.");
        }

    }
}
