using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.player_manager;
using Vehicle = RoleplayServer.resources.vehicle_manager.Vehicle;

namespace RoleplayServer.resources.door_manager
{
    public class Door
    {
        [BsonIgnore]
        public const ulong SET_STATE_OF_CLOSEST_DOOR_OF_TYPE = 0xF82D8F1926A02C3D;

        [BsonIgnore]
        public static List<Door> Doors = new List<Door>(); 

        public int Id { get; set; }
        public int Hash { get; set; }
        public Vector3 Position { get; set; }
        public bool Locked { get; set; }
        public float State { get; set; }
        public string Description { get; set; }
        public bool DoesShowInAdmin { get; set; }

        [BsonIgnore]
        public ColShape Shape;

        public Door(int model, Vector3 pos, string desc, bool locked, bool doeshow)
        {
            Hash = model;
            Position = pos;
            Locked = locked;
            Description = desc;
            DoesShowInAdmin = doeshow;
            State = 0f; //We want it to be closed by default.
        }

        public void Insert()
        {
            Id = DatabaseManager.GetNextId("doors");
            DatabaseManager.DoorsTable.InsertOne(this);
        }

        public void Save()
        {
            var filter = MongoDB.Driver.Builders<Door>.Filter.Eq("_id", Id);
            DatabaseManager.DoorsTable.ReplaceOne(filter, this);
        }

        public void Delete()
        {
            Locked = true;
            RefreshDoor();

            var filter = MongoDB.Driver.Builders<Door>.Filter.Eq("_id", Id);
            DatabaseManager.DoorsTable.DeleteOne(filter);
            Doors.Remove(this);
        }

        public void RegisterDoor()
        {
            Shape = API.shared.createSphereColShape(Position, 35f);
            Shape.onEntityEnterColShape += Shape_onEntityEnterColShape;
            Doors.Add(this);
        }

        public void RefreshDoor()
        {
            foreach (var person in Shape.getAllEntities())
            {
                var player = API.shared.getPlayerFromHandle(person);
                if (player == null) continue;
                API.shared.sendNativeToPlayer(player, SET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                    Hash, Position.X, Position.Y, Position.Z,
                    Locked, State, false);
            }
        }

        private void Shape_onEntityEnterColShape(ColShape colshape, NetHandle entity)
        {
            if (colshape == Shape && API.shared.getEntityType(entity) == EntityType.Player)
            {
                var player = API.shared.getPlayerFromHandle(entity);

                if (player == null) return;

                API.shared.sendNativeToPlayer(player, SET_STATE_OF_CLOSEST_DOOR_OF_TYPE,
                    Hash, Position.X, Position.Y, Position.Z,
                    Locked, State, false);
            }
        }
    }
}
