using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;

namespace mtgvrp.core
{
    public static class General
    {


        public static bool IsPlayerFacingVehicle(NetHandle vehicle, NetHandle player, bool reverse = false)
        {
            var playerpos = API.shared.getEntityPosition(player);
            var vehiclepos = API.shared.getEntityPosition(vehicle);
            var distance = playerpos.DistanceTo(vehiclepos);
            if (distance < 6.0)
            {
                var angle = API.shared.getEntityRotation(vehicle).Z;

                if (reverse)
                    angle += 180;
                
                vehiclepos.X += (float)(distance * Math.Sin(-angle));
                vehiclepos.Y += (float)(distance * Math.Cos(-angle));
                distance = playerpos.DistanceTo(vehiclepos);

                if (distance < 1.0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
