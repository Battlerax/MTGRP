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
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.database_manager;
using mtgvrp.group_manager;
using mtgvrp.inventory;
using mtgvrp.job_manager;
using mtgvrp.player_manager;
using mtgvrp.property_system;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace mtgvrp.vehicle_manager
{
    public class Vehicle : IStorage
    {

        //Inventory System
        public List<IInventoryItem> Inventory { get; set; }
        public int MaxInvStorage => 1000; //TODO: to be changed to be for each vehicle class

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
        public string OwnerName { get; set; }
        public string LicensePlate { get; set; }

        public TimeSpan RespawnDelay { get; set; }
        [BsonIgnore]
        public Timer CustomRespawnTimer { get; set; }

        public int VehType { get; set; }

        public Dictionary<string, string> VehMods { get; set; }

        [BsonIgnore]
        public Job Job { get; set; }
        public int JobId { get; set; }
        public int GarbageBags { get; set; }
        public TextLabel Label { get; set; }

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
        public bool IsVip { get; set; }

        [BsonIgnore]
        public NetHandle Blip { get; set; }

        [BsonIgnore]
        public Character Driver { get; set; }

        public int Fuel { get; set; }

        [BsonIgnore]
        public System.Threading.Timer FuelingTimer { get; set; }

        [BsonIgnore]
        public Property RefuelProp { get; set; }

        public bool IsRegistered { get; set; }

        [BsonIgnore]
        public DateTime LastOccupied { get; set; }

        public Vehicle()
        {
            Id = 0;
            OwnerId = 0;

            SpawnPos = new Vector3(0.0, 0.0, 0.0);
            SpawnRot = new Vector3(0.0, 0.0, 0.0);
            SpawnColors = new int[2];
            SpawnDimension = 0;
            LicensePlate = "DEFAULT";
            Fuel = 100;

            RespawnDelay = TimeSpan.FromMinutes(15);
            VehType = VehTypeTemp;

            NetHandle = new NetHandle();
            OwnerClient = null;
            IsSpawned = false;
            Driver = null;

            JobId = 0;
            Job = Job.None;
            GroupId = 0;
            Group = Group.None;
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
            API.shared.setBlipTransparency(Blip, 100);

            //Set owner detials.
            OwnerClient = PlayerManager.Players.SingleOrDefault(x => x.Id == OwnerId)?.Client;
            IsSpawned = true;

            if (OwnerId == 0 && GroupId == 0)
            {
                Fuel = 25;
            }

            if (Job != Job.None)
            {
                Fuel = 100;
            }

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
            DatabaseManager.VehicleTable.ReplaceOne(filter, this);
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

        public void DestroyMarkers()
        {
            Label?.delete();
        }

        public void UpdateMarkers()
        {
            DestroyMarkers();
            if (this.Job != null)
            {
                if (this.Job?.Type == JobManager.JobTypes.Garbageman)
                {
                    this.Label = API.shared.createTextLabel("~g~" + $"Garbage Bags\n{this.GarbageBags}/10", API.shared.getEntityPosition(this.NetHandle), 25f, 0.5f, true, API.shared.getEntityDimension(this.NetHandle));
                    API.shared.attachEntityToEntity(this.Label, this.NetHandle, "tipper", new Vector3(0, 0, 0), new Vector3(0, 0, 0));

                }
            }
        }
    }
}
