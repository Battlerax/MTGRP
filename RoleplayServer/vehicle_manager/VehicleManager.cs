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
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.core;
using RoleplayServer.database_manager;
using RoleplayServer.job_manager;
using RoleplayServer.player_manager;
using RoleplayServer.group_manager;

namespace RoleplayServer.vehicle_manager
{
    public class VehicleManager : Script
    {
        public static List<Vehicle> Vehicles = new List<Vehicle>();
       
        /*
        * 
        * ========== CONSTRUCTOR =========
        * 
        */

        public VehicleManager()
        {
            DebugManager.DebugMessage("[VehicleM] Initilizing vehicle manager...");

            // Register callbacks
            API.onPlayerEnterVehicle += OnPlayerEnterVehicle;
            API.onVehicleDeath += OnVehicleDeath;
            API.onPlayerExitVehicle += OnPlayerExitVehicle;
            API.onPlayerDisconnected += API_onPlayerDisconnected;

            //Register for on character enter to show his cars.
            CharacterMenu.OnCharacterLogin += CharacterMenu_OnCharacterLogin;

            // Create vehicle table + 
            //load_all_unowned_vehicles();

            DebugManager.DebugMessage("[VehicleM] Vehicle Manager initalized!");
        }

        /*
        * 
        * ========== COMMANDS =========
        * 
        */

        [Command("spawnveh")]
        public void spawnveh_cmd(Client player, VehicleHash model, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            var pos = player.position;
            var rot = player.rotation;

            var veh = CreateVehicle(model, pos, rot, "ABC123", 0, color1, color2, dimension);
            spawn_vehicle(veh);
            
            API.setPlayerIntoVehicle(player, veh.NetHandle, -1);
            API.setVehicleEngineStatus(veh.NetHandle, true);

            API.sendChatMessageToPlayer(player, "You have spawned a " + model);
            API.sendChatMessageToPlayer(player, "This vehicle is unsaved and may behave incorrectly.");
        }

        [Command("savevehicle")]
        public void savevehicle_cmd(Client player)
        {
            var vehHandle = API.getPlayerVehicle(player);
            var veh = GetVehFromNetHandle(vehHandle);

            if(veh == null)
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: You are not inside a vehicle.");
                return;
            }

            if(veh.VehType != Vehicle.VehTypeTemp)
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: You must be inside a temporary vehicle to save it.");
                return;
            }

            if(veh.is_saved())
            {
                API.sendNotificationToPlayer(player, "~r~ ERROR: This vehicle is already saved into the database.");
                return;
            }

            
            veh.Insert();

            veh.VehType = Vehicle.VehTypePerm;
            veh.Save();

            API.sendChatMessageToPlayer(player, "You have saved vehicle " + Vehicles.IndexOf(veh) + ". (SQL: " + veh.Id + ")");
        }

        [Command("savepos")]
        public void savepos_cmd(Client player, int i)
        {
            var pos = player.position;
            var rot = player.rotation;

            API.consoleOutput(i + " " + pos + " " + rot);
            API.sendNotificationToPlayer(player, "Saved");
            API.sendChatMessageToPlayer(player,"Position: " + pos + "Rotation: " + rot);

        }

        [Command("tele")]
        public void Tele(Client player)
        {
            API.setEntityPosition(player.handle, new Vector3(403, -996, -99));
            API.sendChatMessageToPlayer(player, "teleported");
        }

        [Command("lock")]
        public void Lockvehicle_cmd(Client player)
        {
            Vehicle lastVeh = null;
            float lastPos = 5f;
            foreach (Vehicle veh in Vehicles)
            {
                if(veh.IsSpawned == false) continue;
                
                if (API.getEntityPosition(veh.NetHandle).DistanceTo(player.position) < lastPos)
                {
                    lastVeh = veh;
                }
            }

            if (lastVeh == null) return;
            if (!DoesPlayerHaveVehicleAccess(player, lastVeh)) return;

            var lockState = API.getVehicleLocked(lastVeh.NetHandle);
            if (lockState)
            {
                API.setVehicleLocked(lastVeh.NetHandle, false);
                ChatManager.RoleplayMessage(player, "unlocks the doors of the vehicle.", ChatManager.RoleplayMe);
            }
            else
            {
                API.setVehicleLocked(lastVeh.NetHandle, true);
                ChatManager.RoleplayMessage(player, "locks the doors of the vehicle.", ChatManager.RoleplayMe);
            }
        }

        /*
        * 
        * ========== CALLBACKS =========
        * 
        */

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            //Load them.
            Vehicles.AddRange(DatabaseManager.VehicleTable.Find(x => x.OwnerId == e.character.Id).ToList());

            //Spawn his cars.
            var maxVehs = GetMaxOwnedVehicles(e.character.Client);
            if (maxVehs > e.character.OwnedVehicles.Count) maxVehs = e.character.OwnedVehicles.Count;
            for (int i = 0; i < maxVehs; i++)
            {
                var car =
                    Vehicles.SingleOrDefault(
                        x => x.Id == e.character.OwnedVehicles[i] && x.OwnerId == e.character.Id);
                if (car == null)
                    continue;

                if (spawn_vehicle(car) != 1)
                    API.consoleOutput($"There was an error spawning vehicle #{car.Id} of {e.character.CharacterName}.");
            }
        }

        private void API_onPlayerDisconnected(Client player, string reason)
        {
            //DeSpawn his cars.
            Character character = API.getEntityData(player.handle, "Character");
            if (character == null)
                return;
            var maxVehs = GetMaxOwnedVehicles(character.Client);
            if (maxVehs > character.OwnedVehicles.Count) maxVehs = character.OwnedVehicles.Count;
            for (int i = 0; i < maxVehs; i++)
            {
                var car =
                    Vehicles.SingleOrDefault(
                        x => x.Id == character.OwnedVehicles[i] && x.OwnerId == character.Id);
                if (car == null)
                    continue;

                if (despawn_vehicle(car) != 1)
                    API.consoleOutput($"There was an error despawning vehicle #{car.Id} of {character.CharacterName}.");
            }

            //Delete.
            Vehicles.RemoveAll(x => x.OwnerId == character.Id);
            if (character.isJailed)
            {
                character.jailTimer.Stop();
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicleHandle)
        {
            // Admin check in future

            var veh = GetVehFromNetHandle(vehicleHandle);
            API.setBlipTransparency(veh.Blip, 0);

            Character character = API.getEntityData(player.handle, "Character");

            if (veh.Group != null && character.Group != veh.Group && veh.Group != Group.None)
            {
                {
                    API.sendChatMessageToPlayer(player, "You must be a member of " + veh.Group.Name + " to use this vehicle.");
                    API.setEntityPosition(player, API.getEntityPosition(vehicleHandle) + new Vector3(-1, 0, 0));
                    return;
                }
            }
            API.sendChatMessageToPlayer(player, "~w~[VehicleM] You have entered vehicle ~r~" + Vehicles.IndexOf(veh) + "(Owned by: " + PlayerManager.Players.SingleOrDefault(x => x.Id == veh.OwnerId)?.CharacterName + ")");
            API.sendChatMessageToPlayer(player, "~y~ Press \"N\" on your keyboard to access the vehicle menu.");

            //Vehicle Interaction Menu Setup
            var vehInfo = API.getVehicleDisplayName(veh.VehModel) + " - " + veh.LicensePlate;
            API.setEntitySyncedData(player.handle, "CurrentVehicleInfo", vehInfo);
            API.setEntitySyncedData(player.handle, "OwnsVehicle", DoesPlayerHaveVehicleAccess(player, veh));

            if (API.getPlayerVehicleSeat(player) == -1)
            {
                veh.Driver = character;
            }
        }

        public void OnPlayerExitVehicle(Client player, NetHandle vehicleHandle)
        {
            var veh = GetVehFromNetHandle(vehicleHandle);

            if (veh == null)
            {
                DebugManager.DebugMessage("[VehicleVM] OnPlayerExitVehicle received null Vehicle.");
                return;
            }

            API.setBlipTransparency(veh.Blip, 100);

            if (veh.VehType == Vehicle.VehTypeTemp)
            {
                despawn_vehicle(veh);
                delete_vehicle(veh);

                API.sendNotificationToPlayer(player, "Your vehicle was deleted on exit because it was temporary.");
            }

            if (veh.Driver == player.GetCharacter())
                veh.Driver = null;

            Character character = API.getEntityData(player.handle, "Character");
            character.LastVehicle = veh;
        }

        public void OnVehicleDeath(NetHandle vehicleHandle)
        {
            var veh = GetVehFromNetHandle(vehicleHandle);
            API.consoleOutput("Vehicle " + vehicleHandle + " died");
            API.delay(veh.RespawnDelay, true, () =>
            {
                respawn_vehicle(veh);
            });
        }

        /*
        * 
        * ========== FUNCTIONS =========
        * 
        */

        public static int GetMaxOwnedVehicles(Client chr)
        {
            Account acc = API.shared.getEntityData(chr, "Account");
            switch (acc.VipLevel)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 4;
                case 3:
                    return 5;
            }
            return 0;
        }

    public static Vehicle CreateVehicle(VehicleHash model, Vector3 pos, Vector3 rot, string license, int ownerid, int vehtype, int color1 = 0, int color2 = 0, int dimension = 0)
        {
            var veh = new Vehicle
            {
                VehModel = model,
                SpawnPos = pos,
                SpawnRot = rot,
                SpawnColors =
                {
                    [0] = color1,
                    [1] = color2
                },
                SpawnDimension = dimension,
                LicensePlate = license,
                OwnerId = ownerid,
                VehType = vehtype
            };

            Vehicles.Add(veh);
            return veh;
        }

        public static void delete_vehicle(Vehicle veh)
        {
            Vehicles.Remove(veh);
        }

        public static Vehicle GetVehFromNetHandle(NetHandle handle)
        {
            return API.shared.getEntityData(handle, "Vehicle");
        }

        public static int spawn_vehicle(Vehicle veh, Vector3 pos)
        {
            var returnCode = veh.Spawn(pos);

            if (returnCode == 1)
            {
                API.shared.setEntityData(veh.NetHandle, "Vehicle", veh);
            }
            
            API.shared.setVehicleEngineStatus(veh.NetHandle, false);
            return returnCode;
        }

        public static int spawn_vehicle(Vehicle veh)
        {
            return spawn_vehicle(veh, veh.SpawnPos);
        }

        public static int despawn_vehicle(Vehicle veh)
        {
            var returnCode = veh.Despawn();

            if (returnCode == 1)
            {
                API.shared.resetEntityData(veh.NetHandle, "Vehicle");
            }

            return returnCode;
        }

        public static int respawn_vehicle(Vehicle veh, Vector3 pos)
        {
            if (API.shared.hasEntityData(veh.NetHandle, "Vehicle"))
            {
                despawn_vehicle(veh);
            }
            return spawn_vehicle(veh, pos);
        }

        public static int respawn_vehicle(Vehicle veh)
        {
            return respawn_vehicle(veh, veh.SpawnPos);
        }

        public static bool DoesPlayerHaveVehicleAccess(Client player, Vehicle vehicle)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character character = API.shared.getEntityData(player.handle, "Character");

            if (account.AdminLevel >= 3) { return true; }
            if (character.Id == vehicle.OwnerId) { return true; }
            //faction check
            if(character.JobOne == vehicle.Job) { return true; }
            //gang check

            return false;
        }

        /*public void load_all_group_vehicles()
        {
            int j = 0;

            foreach (var i in DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList())
            {

                var filter = Builders<Vehicle>.Filter.Eq("GroupId", i.Id);
                var groupVehicles = DatabaseManager.VehicleTable.Find(filter).ToList();

                foreach (var v in groupVehicles)
                {
                    spawn_vehicle(v);
                    Vehicles.Add(v);
                    j++;
                }
            }

            DebugManager.DebugMessage("Loaded " + j + " group vehicles from the database.");
        }*/

        public static void load_all_unowned_vehicles()
        {
            var filter = Builders<Vehicle>.Filter.Eq("OwnerId", "0");
            var unownedVehicles = DatabaseManager.VehicleTable.Find(filter).ToList();

            foreach (var v in unownedVehicles)
            {
                spawn_vehicle(v);

                v.Job = JobManager.GetJobById(v.JobId);
                v.Group = GroupManager.GetGroupById(v.GroupId);
                
                Vehicles.Add(v);
            }

            DebugManager.DebugMessage("Loaded " + unownedVehicles.Count + " unowned vehicles from the database.");
        }
    }
}
