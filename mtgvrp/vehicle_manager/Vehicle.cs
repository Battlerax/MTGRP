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


using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RoleplayServer.database_manager;
using RoleplayServer.job_manager;
using RoleplayServer.player_manager;
using RoleplayServer.group_manager;

namespace RoleplayServer.vehicle_manager
{
    public class Vehicle
    {
        [BsonIgnore]
        public const int VehTypeTemp = 0;
        [BsonIgnore]
        public const int VehTypePerm = 1;
        [BsonIgnore]
        public const int VehTypeGroup = 2;

        public int Id { get; set; }
       
        public VehicleHash VehModel { get; set; }
        public Vector3 SpawnPos { get; set; }
        public Vector3 SpawnRot { get; set; }
        public int[] SpawnColors = new int[2];
        public int SpawnDimension { get; set; }
        public int OwnerId { get; set; }
        public string LicensePlate { get; set; }

        public int RespawnDelay { get; set; }
        [BsonIgnore]
        public Timer RespawnTimer { get; set; }

        public int VehType { get; set; }
       
        [BsonIgnore]
        public Job Job { get; set; }
        public int JobId { get; set; }

        [BsonIgnore]
        public Group Group { get; set; }
        public int GroupId { get; set; }

        [BsonIgnore]
        public NetHandle NetHandle { get; private set; }
        [BsonIgnore]
        private Client OwnerClient { get; set; }
        [BsonIgnore]
        public bool IsSpawned { get; set; }

        [BsonIgnore]
        public NetHandle Blip { get; set; }

        [BsonIgnore]
        public Character Driver { get; set; }

        public Vehicle()
        {
            Id = 0;
            OwnerId = 0;

            SpawnPos = new Vector3(0.0, 0.0, 0.0);
            SpawnRot = new Vector3(0.0, 0.0, 0.0);
            SpawnColors = new int[2];
            SpawnDimension = 0;
            LicensePlate = "DEFAULT";

            RespawnDelay = 600;
            VehType = VehTypeTemp;

            NetHandle = new NetHandle();
            OwnerClient = null;
            IsSpawned = false;
            Driver = null;

            JobId = 0;
            GroupId = 0;
        }


        /*  Respawns a vehicle at its default spawn */
        public int Respawn()
        {
            if (IsSpawned)
                Despawn();
            Spawn();
            return 1;
        }

        /* Respawns a vehicle at a given location */
        public int Respawn(Vector3 pos)
        {
            if (IsSpawned)
                Despawn();
            Spawn(pos);
            return 1;
        }

        /*  Spawns a vehicle at its default spawn location. */
        public int Spawn()
        {
            return Spawn(SpawnPos);
        }

        /*  Spawns a vehicle at the given position in the world */
        public int Spawn(Vector3 pos)
        {
            if (SpawnPos == null)
                return -1; // No valid spawn position available

            if (IsSpawned)
                return 0; // Vehicle is already spawned

           
            NetHandle = API.shared.createVehicle(VehModel, pos, SpawnRot, SpawnColors[0], SpawnColors[1], SpawnDimension);
            API.shared.setVehicleNumberPlate(NetHandle, LicensePlate);

            Blip = API.shared.createBlip(NetHandle);
            API.shared.setBlipColor(Blip, 40);
            API.shared.setBlipSprite(Blip, API.shared.getVehicleClass(VehModel) == 14 ? 410 : 225);
            API.shared.setBlipScale(Blip, (float)0.7);
            API.shared.setBlipShortRange(Blip, true);

            //Set owner detials.
            OwnerClient = PlayerManager.ParseClient(OwnerId.ToString());

            IsSpawned = true;

            return 1; // Successful spawn
        }

        /*  Despawns a vehicle, removing it from the world  */
        public int Despawn()
        {
            if (IsSpawned == false)
                return 0; // Vehicle is not spawned

            API.shared.deleteEntity(Blip);
            API.shared.deleteEntity(NetHandle);
            IsSpawned = false;
            return 1; // Successful despawn
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("vehicles");
            DatabaseManager.VehicleTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = Builders<Vehicle>.Filter.Eq("_id", Id);
            DatabaseManager.VehicleTable.ReplaceOneAsync(filter, this);
        }

        public void Delete()
        {
            var filter = Builders<Vehicle>.Filter.Eq("_id", Id);
            DatabaseManager.VehicleTable.DeleteOne(filter);
        }

        public bool is_saved()
        {
            if (Id == 0)
                return false;

            var filter = Builders<Vehicle>.Filter.Eq("_id", Id);

            if (DatabaseManager.VehicleTable.Find(filter).Count() > 0)
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
