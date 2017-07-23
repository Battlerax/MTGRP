using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace mtgvrp.core
{
    public class ObjectRemovel : Script
    {
        private static readonly List<dynamic[]> _objects = new List<dynamic[]>();

        public static void RegisterObject(Vector3 position, int hash)
        {
            var colZone = API.shared.createSphereColShape(position, 175f);
            colZone.onEntityEnterColShape += OnPlayerEnterObjectZone;
            _objects.Add(new dynamic[] {position, hash, colZone});
        }

        public static void RemoveObject(Vector3 position, int hash)
        {
            var obj = _objects.FirstOrDefault(x => x[0] == position && x[1] == hash);
            if (obj == null)
            {
                return;
            }

            GrandTheftMultiplayer.Server.API.API.shared.deleteColShape(obj[2]);
            _objects.Remove(obj);
        }

        public static void OnPlayerEnterObjectZone(ColShape shape, NetHandle entity)
        {
            if (API.shared.getEntityType(entity) == EntityType.Player)
            {
                var obj = _objects.FirstOrDefault(x => x[2] == shape);

                if (obj == null)
                    return;

                API.shared.deleteObject(API.shared.getPlayerFromHandle(entity), obj[0], obj[1]);
            }
        }
    }
}