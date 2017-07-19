using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Shared.Math;

namespace mtgvrp.core
{
    public class ObjectRemovel : Script
    {
        private Timer _timer;
        public ObjectRemovel()
        {
            _timer = new Timer((state) =>
            {
                foreach (var player in API.getAllPlayers())
                {
                    foreach (var obj in _objects)
                    {
                        if (player.position.DistanceTo(obj[0]) <= 175.0f)
                        {
                            API.deleteObject(player, obj[0], obj[1]);
                        }
                    }
                }

            }, null, 1000, 1000);
        }

        private static List<dynamic[]> _objects = new List<dynamic[]>();

        public static void RegisterObject(Vector3 position, int hash)
        {
            _objects.Add(new dynamic[] {position, hash});
        }
    }
}
