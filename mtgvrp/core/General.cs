using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace mtgvrp.core
{
    class General
    {


        public (float, float) GetXYBehindVehicle(NetHandle vehicle, float distance)
        {
            var pos = API.shared.getEntityPosition(vehicle);
            var rot = API.shared.getEntityRotation(vehicle);
            var q = (float)(pos.X + (distance * -Math.Sin(-rot.Z)));
            var w = (float)(pos.Y + (distance * -Math.Cos(-rot.Z)));
            return (q, w);
        }
    }
}
