using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;
using RoleplayServer.database_manager;
using RoleplayServer.player_manager;
using Vehicle = RoleplayServer.vehicle_manager.Vehicle;

namespace RoleplayServer.door_manager
{
    public class Door
    {
        [BsonIgnore]
        public const ulong SetStateOfClosestDoorOfType = 0xF82D8F1926A02C3D;

        [BsonIgnore]
        public static List<Door> Doors = new List<Door>(); 

        public int Id { get; set; }
        public int Hash { get; set; }
        public Vector3 Position { get; set; }
        public bool Locked { get; set; }
        public float State { get; set; }
        public string Description { get; set; }
        public bool DoesShowInAdmin { get; set; }
        public int GroupId { get; set; }
        public int PropertyId { get; set; }

        [BsonIgnore]
        public ColShape Shape;

        [BsonIgnore]
        public TextLabel Text;

        public Door(int model, Vector3 pos, string desc, bool locked, bool doeshow)
        {
            Id = -1;
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
            Text.delete();
            Shape.onEntityEnterColShape -= Shape_onEntityEnterColShape;
            API.shared.deleteColShape(Shape);

            var filter = MongoDB.Driver.Builders<Door>.Filter.Eq("_id", Id);
            DatabaseManager.DoorsTable.DeleteOne(filter);
            Doors.Remove(this);
        }

        public void RegisterDoor()
        {
            Shape = API.shared.createSphereColShape(Position, 35f);
            Text = API.shared.createTextLabel("~g~Door Id: " + Id, Position.Add(new Vector3(-1, 0, 0)), 5f, 1f, true);
            Shape.onEntityEnterColShape += Shape_onEntityEnterColShape;
            Doors.Add(this);
        }

        public void RefreshDoor()
        {
            foreach (var person in Shape.getAllEntities())
            {
                var player = API.shared.getPlayerFromHandle(person);
                if (player == null) continue;
                API.shared.sendNativeToPlayer(player, SetStateOfClosestDoorOfType,
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

                API.shared.sendNativeToPlayer(player, SetStateOfClosestDoorOfType,
                    Hash, Position.X, Position.Y, Position.Z,
                    Locked, State, false);
            }
        }
    }
}
