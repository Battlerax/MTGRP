/*
 *  File: Vehicle.cs
 *  Author: Chenko
 *  Date: 12/24/2016
 * 
 * 
 *  Purpose: Vehicle class used to vehicles created on the server.
 * 
 * 
 * */


using System;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace RoleplayServer
{
    public class Vehicle
    {
        [BsonIgnore]
        public const int VEH_TYPE_TEMP = 0;
        [BsonIgnore]
        public const int VEH_TYPE_PERM = 1;

        public int _id { get; set; }
       
        public VehicleHash veh_model { get; set; }
        public Vector3 spawn_pos { get; set; }
        public Vector3 spawn_rot { get; set; }
        public int[] spawn_colors = new int[2];
        public int spawn_dimension { get; set; }
        public string owner_name { get; set; }
        public int owner_id { get; set; }
        public string license_plate { get; set; }

        public int respawn_delay { get; set; }
        public int veh_type { get; set; }

        [BsonIgnore]
        public NetHandle net_handle { get; private set; }
        [BsonIgnore]
        private Client owner_client { get; set; }
        [BsonIgnore]
        private bool is_spawned { get; set; }

        [BsonIgnore]
        public NetHandle blip { get; set; }

        public Vehicle()
        {
            _id = 0;
            owner_id = 0;

            spawn_pos = new Vector3(0.0, 0.0, 0.0);
            spawn_rot = new Vector3(0.0, 0.0, 0.0);
            spawn_colors = new int[2];
            spawn_dimension = 0;
            owner_name = "None";
            license_plate = "DEFAULT";

            respawn_delay = 600;
            veh_type = VEH_TYPE_TEMP;

            net_handle = new NetHandle();
            owner_client = null;
            is_spawned = false;
        }


        /*  Respawns a vehicle at its default spawn */
        public int respawn()
        {
            if (is_spawned == true)
                despawn();
            spawn();
            return 1;
        }

        /* Respawns a vehicle at a given location */
        public int respawn(Vector3 pos)
        {
            if (is_spawned == true)
                despawn();
            spawn(pos);
            return 1;
        }

        /*  Spawns a vehicle at its default spawn location. */
        public int spawn()
        {
            return spawn(spawn_pos);
        }

        /*  Spawns a vehicle at the given position in the world */
        public int spawn(Vector3 pos)
        {
            if (spawn_pos == null)
                return -1; // No valid spawn position available

            if (is_spawned == true)
                return 0; // Vehicle is already spawned

            is_spawned = true;
            net_handle = API.shared.createVehicle(veh_model, pos, spawn_rot, spawn_colors[0], spawn_colors[1], spawn_dimension);
            API.shared.setVehicleNumberPlate(net_handle, "ABC123");

            blip = API.shared.createBlip(net_handle);
            API.shared.setBlipColor(blip, 40);
            API.shared.setBlipSprite(blip, 225);
            API.shared.setBlipScale(blip, (float)(0.7));
            API.shared.setBlipShortRange(blip, true);

            return 1; // Successful spawn
        }

        /*  Despawns a vehicle, removing it from the world  */
        public int despawn()
        {
            if (is_spawned == false)
                return 0; // Vehicle is not spawned

            API.shared.deleteEntity(net_handle);
            API.shared.deleteEntity(blip);
            is_spawned = false;
            return 1; // Successful despawn
        }

        public void insert()
        {
            this._id = DatabaseManager.getNextId("vehicles");
            DatabaseManager.vehicle_table.InsertOne(this);
        }

        public void save()
        {
            FilterDefinition<Vehicle> filter = Builders<Vehicle>.Filter.Eq("_id", this._id);
            DatabaseManager.vehicle_table.ReplaceOneAsync(filter, this);
        }

        public bool is_saved()
        {
            if (this._id == 0)
                return false;

            FilterDefinition<Vehicle> filter = Builders<Vehicle>.Filter.Eq("_id", this._id);

            if (DatabaseManager.vehicle_table.Find(filter).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
