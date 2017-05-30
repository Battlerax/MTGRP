using GTANetworkShared;
using GTANetworkServer;

namespace RoleplayServer.resources.core
{
    class DoorManager : Script
    {
        public DoorManager()
        {
            API.onResourceStart += onResourceStart;
        }

        public readonly Vector3 jailPosOne = new Vector3(461.8065, -994.4086, 25.06443);
        public readonly Vector3 jailPosTwo = new Vector3(461.8065, -997.6583, 25.06443);
        public readonly Vector3 jailPosThree = new Vector3(461.8065, -1001.302, 25.06443);

        public void onResourceStart()
        {

            /*int i = API.exported.doormanager.registerDoor(631614199, jailPosOne);
            API.exported.doormanager.setDoorState(i, true, 0);

            int z = API.exported.doormanager.registerDoor(631614199, jailPosTwo);
            API.exported.doormanager.setDoorState(z, true, 0);

            int v = API.exported.doormanager.registerDoor(631614199, jailPosThree);
            API.exported.doormanager.setDoorState(v, true, 0);*/
        }
    }


}
