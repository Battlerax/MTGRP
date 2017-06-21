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
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.player_manager;
using MongoDB.Driver;

namespace mtgvrp.vehicle_manager
{
    public class VehicleManager : Script
    {
        public static List<Vehicle> Vehicles = new List<Vehicle>();

        /*
        * 
        * ========== CONSTRUCTOR =========
        * 
        */
        public ColShape dropcarShape;
        public Vector3 dropcarPosition = new Vector3(487.0575, -1334.377, 29.30219);

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
            dropcarShape = API.createCylinderColShape(dropcarPosition, 2f, 3f);

            dropcarShape.onEntityEnterColShape += (shape, entity) =>
            {
                Client player;
                if ((player = API.getPlayerFromHandle(entity)) != null)
                {
                    Character character = API.getEntityData(player.handle, "Character");

                    if (!character.IsOnDropcar)
                    {
                        return;
                    }

                    var veh = GetVehFromNetHandle(API.getPlayerVehicle(player));
                    float payment = API.getVehicleHealth(API.getPlayerVehicle(player)) / 2;
                    VehicleManager.respawn_vehicle(veh);
                    API.shared.setVehicleEngineStatus(veh.NetHandle, false);
                    inventory.InventoryManager.GiveInventoryItem(character, new Money(), (int) payment);
                    character.IsOnDropcar = false;
                    API.triggerClientEvent(player, "dropcar_removewaypoint");
                    player.sendChatMessage("Vehicle delivered. You earned $" + (int)payment);
                }
            };


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

            var veh = CreateVehicle(model, pos, rot, "ABC123", 0, Vehicle.VehTypeTemp, color1, color2, dimension);
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

        [Command("vstorage")]
        public void VehicleStorage(Client player)
        {
            var lastVeh = GetNearestVehicle(player);

            if (lastVeh == null) return;
            if (!DoesPlayerHaveVehicleAccess(player, lastVeh))
            {
                API.sendChatMessageToPlayer(player, "You must have access to the vehicle.");
                return;
            }

            if (!API.getVehicleDoorState(lastVeh.NetHandle, 5))
            {
                API.sendChatMessageToPlayer(player, "Trunk must be open to access the storage.");
                return;
            }

            if (lastVeh.Inventory == null) lastVeh.Inventory = new List<IInventoryItem>();
            InventoryManager.ShowInventoryManager(player, player.GetCharacter(), lastVeh, "Inventory: ", "Vehicle: ");
        }

        [Command ("engine", Alias = "e")]
        public void engine_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");
            var vehicleHandle = API.getPlayerVehicle(player);
            Vehicle vehicle = API.shared.getEntityData(vehicleHandle, "Vehicle");

            var engineState = API.shared.getVehicleEngineStatus(vehicleHandle);
            var vehAccess = DoesPlayerHaveVehicleAccess(player, vehicle);
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
                API.sendChatMessageToPlayer(player, "You don't have access to this vehicle.");
            }

        }

        [Command("hotwire")]
        public void hotwire_cmd(Client player)
        {
            if (player.isInVehicle == false)
            {
                API.sendChatMessageToPlayer(player, "You are not in a vehicle.");
                return;
            }

            var veh = API.getPlayerVehicle(player);

            if (API.getVehicleEngineStatus(veh) == true)
            {
                API.sendChatMessageToPlayer(player, "This vehicle is already started.");
                return;
            }

            ChatManager.NearbyMessage(player, 6f, "~p~" + player.name + " attempts to hotwire the vehicle.");

            Random rand = new Random();

            if (rand.Next(0, 2) == 0)
            {
                API.setVehicleEngineStatus(veh, true);
                ChatManager.NearbyMessage(player, 6f, "~p~" + player.name + " succeeded in hotwiring the vehicle.");
            }
            else
            {
                API.setPlayerHealth(player, player.health - 10);
                player.sendChatMessage("You attempted to hotwire the vehicle and got shocked!");
                ChatManager.NearbyMessage(player, 6f, "~p~" + player.name + " failed to hotwire the vehicle.");
            }

        }

        [Command("dropcar")]
        public void dropcar_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (player.isInVehicle == false)
            {
                API.sendChatMessageToPlayer(player, "You are not in a vehicle.");
                return;
            }

            if (DateTime.Now < character.DropcarReset)
            {
                player.sendChatMessage("You can only do this every 15 minutes.");
                return;
            }

            var veh = GetVehFromNetHandle(API.getPlayerVehicle(player));

            if (veh.Group != Group.None || veh.OwnerId != 0)
            {
                API.sendChatMessageToPlayer(player, "This is an owned vehicle.");
                return;
            }

            character.DropcarReset = DateTime.Now.AddMinutes(15);
            character.IsOnDropcar = true;
            API.triggerClientEvent(player, "dropcar_setwaypoint", new Vector3(487.0575, -1334.377, 29.30219) - new Vector3(0, 0, 1));
            player.sendChatMessage("A waypoint has been set. Take this vehicle to the waypoint to earn money.");
        }

        [Command("lock")]
        public void Lockvehicle_cmd(Client player)
        {
            var lastVeh = GetNearestVehicle(player);

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

        [Command("respawnveh")]
        public void respawnveh_cmd(Client player, int id, bool originalPos = true)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }

            if (id > Vehicles.Count)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Invalid vehicle ID.");
                return; 
            }

            var vehicle = Vehicles[id];
            if (vehicle == null)
            {
                API.sendChatMessageToPlayer(player, Color.White,
                    "~r~[ERROR]~w~ No vehicle was found with that nethandle.");
                return;
            }

            if (originalPos == true)
            {
                VehicleManager.respawn_vehicle(vehicle);
            }
            else
            {
                VehicleManager.respawn_vehicle(vehicle, API.getEntityPosition(vehicle.NetHandle));
            }
            API.sendChatMessageToPlayer(player, Color.White, "You have respawned the vehicle with id " + id);
            return;
        }

        [Command("respawnunownedcars")]
        public void respawnallcars_cmd(Client player)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }

            foreach (var v in Vehicles)
            {
                if (v.Driver == null && v.OwnerId == 0 && v.GroupId == 0)
                {
                    VehicleManager.respawn_vehicle(v);
                }
            }
            API.sendChatMessageToPlayer(player, Color.White, "All unowned and unoccupied vehicles have been respawned.");
            return;
        }

        [Command("respawnnearbycars")]
        public void respawnnearbycars_cmd(Client player, int radius = 15)
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 4)
            {
                return;
            }

            if (radius <= 0)
            {
                return;
            }

            foreach (var v in Vehicles)
            {
                if (v.Driver == null)
                {
                    if (player.position.DistanceTo(API.getEntityPosition(v.NetHandle)) <= radius)
                    {
                        VehicleManager.respawn_vehicle(v);
                    }
                }
            }
            API.sendChatMessageToPlayer(player, Color.White, "Respawned all unowned and unoccupied cars in a radius of " + radius);
        }

        /*
        * 
        * ========== CALLBACKS =========
        * 
        */

        private void CharacterMenu_OnCharacterLogin(object sender, CharacterMenu.CharacterLoginEventArgs e)
        {
            //Load them.
            Vehicles.AddRange(DatabaseManager.VehicleTable.Find(x => x.OwnerId == e.Character.Id).ToList());

            //Spawn his cars.
            var maxVehs = GetMaxOwnedVehicles(e.Character.Client);
            if (maxVehs > e.Character.OwnedVehicles.Count) maxVehs = e.Character.OwnedVehicles.Count;
            for (int i = 0; i < maxVehs; i++)
            {
                var car =
                    Vehicles.SingleOrDefault(
                        x => x.Id == e.Character.OwnedVehicles[i] && x.OwnerId == e.Character.Id);
                if (car == null)
                    continue;

                if (spawn_vehicle(car) != 1)
                    API.consoleOutput($"There was an error spawning vehicle #{car.Id} of {e.Character.CharacterName}.");
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
            if (character.IsJailed)
            {
                character.JailTimer.Stop();
            }
        }

        private void OnPlayerEnterVehicle(Client player, NetHandle vehicleHandle)
        {
            // Admin check in future

            var veh = GetVehFromNetHandle(vehicleHandle);
            API.setBlipTransparency(veh.Blip, 0);

            Character character = API.getEntityData(player.handle, "Character");
            Account account = API.getEntityData(player.handle, "Account");

            //IS A GROUP VEHICLE
            if (veh.Group != null && character.Group != veh.Group && veh.Group != Group.None)
            {
                {
                    API.sendChatMessageToPlayer(player, "You must be a member of " + veh.Group.Name + " to use this vehicle.");
                    API.warpPlayerOutOfVehicle(player);
                    return;
                }
            }

            //IS A VIP VEHICLE
            if (veh.IsVip == true && account.VipLevel <= 1 && API.getPlayerVehicleSeat(player) == -1)
            {
                player.sendChatMessage("This is a ~y~VIP~y~ vehicle. You must be a VIP to drive it.");
                API.warpPlayerOutOfVehicle(player);
                return;
            }

            if (account.AdminLevel > 1)
            {
                API.sendChatMessageToPlayer(player, "~w~[VehicleM] You have entered vehicle ~r~" + Vehicles.IndexOf(veh) + "(Owned by: " + PlayerManager.Players.SingleOrDefault(x => x.Id == veh.OwnerId)?.CharacterName + ")");
            }
            
            if (API.getVehicleLocked(vehicleHandle))
            {
                API.warpPlayerOutOfVehicle(player);
                API.sendChatMessageToPlayer(player, "~r~The vehicle is locked.");
                return;
            }
            
            //Vehicle Interaction Menu Setup
            var vehInfo = API.getVehicleDisplayName(veh.VehModel) + " - " + veh.LicensePlate;
            API.setEntitySyncedData(player.handle, "CurrentVehicleInfo", vehInfo);
            API.setEntitySyncedData(player.handle, "OwnsVehicle", DoesPlayerHaveVehicleAccess(player, veh));

            if (API.getPlayerVehicleSeat(player) == -1)
            {
                veh.Driver = character;
                API.sendChatMessageToPlayer(player, "~y~Press 'N' to access the vehicle menu.");
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

            if (character.IsOnDropcar)
            {
                character.IsOnDropcar = false;
                API.triggerClientEvent(player, "dropcar_removewaypoint");
                player.sendChatMessage("You exited the vehicle. The dropcar has ended.");
            }
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

        public static Vehicle GetNearestVehicle(Client player, float radius = 5f)
        {
            Vehicle lastVeh = null;
            float lastPos = radius;
            foreach (Vehicle veh in Vehicles)
            {
                if (veh.IsSpawned == false) continue;

                if (API.shared.getEntityPosition(veh.NetHandle).DistanceTo(player.position) < lastPos)
                {
                    lastVeh = veh;
                }
            }
            return lastVeh;
        }

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
            else API.shared.consoleOutput("spawn_veh returned coded " + returnCode);
            
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
            if (vehicle.GroupId == character.GroupId && character.GroupId != 0) return true;
            if(character.JobOne == vehicle.Job) { return true; }

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

                API.shared.setBlipTransparency(v.Blip, 100);

                Vehicles.Add(v);
            }

            DebugManager.DebugMessage("Loaded " + unownedVehicles.Count + " unowned vehicles from the database.");
        }
    }
}
