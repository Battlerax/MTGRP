using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;
using mtgvrp.vehicle_manager;

namespace mtgvrp.dmv
{
    public class DmvManager : Script
    {
        public static Vector3[] testCheckpoints =
        {
            new Vector3(275.7367, -381.1554, 44.43955),
            new Vector3(295.1309, -451.0204, 42.92255),
            new Vector3(240.3799, -470.0326, 41.2066),
            new Vector3(81.14854, -476.974, 33.42332),
            new Vector3(-60.00119, -487.6081, 31.58802),
            new Vector3(-156.1391, -487.6473, 28.13439),
            new Vector3(-214.1319, -478.0857, 26.06907),
            new Vector3(-351.3909, -475.7861, 37.55101),
            new Vector3(-413.848, -538.1572, 41.53019),
            new Vector3(-415.9911, -650.6707, 36.68476),
            new Vector3(-415.7881, -711.6459, 36.66223),
            new Vector3(-416.6297, -908.2397, 36.6805),
            new Vector3(-415.8291, -1130.886, 36.6318),
            new Vector3(-417.3184, -1304.933, 36.59539),
            new Vector3(-393.2245, -1403.785, 37.98389),
            new Vector3(-381.8099, -1327.558, 36.63412),
            new Vector3(-293.8431, -1250.104, 36.7085),
            new Vector3(-186.7111, -1245.432, 36.65297),
            new Vector3(-78.04612, -1255.107, 36.51565),
            new Vector3(29.52601, -1262.698, 28.92844),
            new Vector3(56.68351, -1258.65, 28.7719),
            new Vector3(72.45765, -1217.64, 28.70172),
            new Vector3(71.9919, -1129.681, 28.74697),
            new Vector3(101.6237, -1026.051, 28.8411),
            new Vector3(147.6756, -901.7576, 29.75008),
            new Vector3(190.9861, -782.1708, 31.36706),
            new Vector3(217.9093, -707.7353, 34.88936),
            new Vector3(265.1003, -592.2642, 42.60115),
            new Vector3(296.4409, -510.9495, 42.73027),
            new Vector3(312.4164, -413.073, 44.50725),
            new Vector3(288.2681, -366.7339, 44.55527),
            new Vector3(290.8339, -339.3133, 44.36193),
        };

        [Command("/starttest")]
        public void StartTest(Client player)
        {
            var c = player.GetCharacter();

            if (c.IsInDmvTest)
                return;

            //SPAWN CAR. Temporary.
            var veh = VehicleManager.CreateVehicle(VehicleHash.Asea, player.position, player.rotation, "", 0, vehicle_manager.Vehicle.VehTypeTemp, 89, 89);
            VehicleManager.spawn_vehicle(veh);
            API.setPlayerIntoVehicle(player, veh.NetHandle, -1);

            c.TimeStartedTest = DateTime.Now;
            c.IsInDmvTest = true;
            c.DmvTestStep = 0;
            NextCheckpoint(player);
            API.sendChatMessageToPlayer(player, "GO!");
        }

        void NextCheckpoint(Client player)
        {
            var c = player.GetCharacter();

            if (c.NextCheckpointColShape != null)
                API.deleteColShape(c.NextCheckpointColShape);

            //Check next.
            if (c.DmvTestStep < testCheckpoints.Length)
            {
                c.NextCheckpointColShape = API.createSphereColShape(testCheckpoints[c.DmvTestStep], 5.0f);
                c.NextCheckpointColShape.onEntityEnterColShape += (shape, entity) =>
                {
                    if (entity != player) return;

                    API.sendChatMessageToPlayer(player, "Entered checkpoint " + c.DmvTestStep);
                    c.DmvTestStep += 1;
                    NextCheckpoint(player);
                };
                API.triggerClientEvent(player, "DMV_UPDATE_MARKER", testCheckpoints[c.DmvTestStep].Subtract(new Vector3(0, 0, 0.2)));
            }
            else
            {
                API.triggerClientEvent(player, "DMV_UPDATE_MARKER", new Vector3());
                API.sendChatMessageToPlayer(player, "DONE IN " + DateTime.Now.Subtract(c.TimeStartedTest).ToString());
                c.IsInDmvTest = false;
            }

        }
    }
}
