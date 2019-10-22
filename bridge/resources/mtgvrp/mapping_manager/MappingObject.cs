


using GTANetworkAPI;
using MongoDB.Bson.Serialization.Attributes;
using mtgvrp.core;

namespace mtgvrp.mapping_manager
{
    public class MappingObject
    {
        public enum ObjectType
        {
            CreateObject,
            DeleteObject,
        }

        public ObjectType Type;
        public int Model;
        public Vector3 Pos;
        public Vector3 Rot;

        [BsonIgnore]
        public NetHandle handle;

        public MappingObject(ObjectType type, int model, Vector3 pos, Vector3 rot)
        {
            Type = type;
            Model = model;
            Pos = pos;
            Rot = rot;
        }

        public void Spawn(int dim)
        {
            if(Type == ObjectType.CreateObject)
            {
                handle = API.Shared.CreateObject(Model, Pos, Rot.ToQuat(), (uint)dim);
            }
        }

        public void Despawn()
        {
            API.Shared.DeleteEntity(handle);
        }
    }
}
