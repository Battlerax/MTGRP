using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Bson.Serialization.Attributes;

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
                handle = API.shared.createObject(Model, Pos, Rot, dim);
            }
        }

        public void Spawn(Client player)
        {
            if(Type == ObjectType.DeleteObject)
            {
                API.shared.deleteObject(player, Pos, Model);
            }
        }

        public void Despawn()
        {
            API.shared.deleteEntity(handle);
        }
    }
}
