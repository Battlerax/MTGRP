using System.Collections.Generic;

using GTANetworkAPI;



using mtgvrp.database_manager;
using MongoDB.Bson.Serialization.Attributes;

namespace mtgvrp.door_manager
{
    // TODO: add dimensions
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
            Text.Delete();
            Shape.OnEntityEnterColShape -= Shape_onEntityEnterColShape;
            API.Shared.DeleteColShape(Shape);

            var filter = MongoDB.Driver.Builders<Door>.Filter.Eq("_id", Id);
            DatabaseManager.DoorsTable.DeleteOne(filter);
            Doors.Remove(this);
        }

        public void RegisterDoor()
        {
            Shape = API.Shared.CreateSphereColShape(Position, 35f);
            Text = API.Shared.CreateTextLabel("~g~Door Id: " + Id, Position.Add(new Vector3(-1, 0, 0)), 5f, 1f, 1, new GTANetworkAPI.Color(1, 1, 1), true);
            Shape.OnEntityEnterColShape += Shape_onEntityEnterColShape;
            Doors.Add(this);
        }

        // CONV NOTE: fix this
        public void RefreshDoor()
        {
            /*foreach (var person in Shape.GetAllEntities())
            {
                var player = API.Shared.GetPlayerFromHandle(person);
                if (player == null) continue;
                API.Shared.SendNativeToPlayer(player, SetStateOfClosestDoorOfType,
                    Hash, Position.X, Position.Y, Position.Z,
                    Locked, State, false);
            }*/
        }

        private void Shape_onEntityEnterColShape(ColShape colshape, Player entity)
        {
            if (colshape == Shape && API.Shared.GetEntityType(entity) == EntityType.Player)
            {
                var player = API.Shared.GetPlayerFromHandle(entity);

                if (player == null) return;

                API.Shared.SendNativeToPlayer(player, SetStateOfClosestDoorOfType,
                    Hash, Position.X, Position.Y, Position.Z,
                    Locked, State, false);
            }
        }
    }
}
