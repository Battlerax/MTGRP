using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using System.Linq;

namespace RoleplayServer.resources.group_manager.lspd
{
    public class Lspd : Script
    {

        public Lspd()
        {
            API.onUpdate += onUpdate;
            API.onResourceStart += startLspd;
        }

        //arrest location (admin command to change location pending).
        public static readonly Vector3 arrest_loc = new Vector3(468.7845f, -1015.69f, 26.38641f);

        //Jails
        public static readonly Vector3 jailOne = new Vector3(458.0021f, -1001.581f, 24.91485f);
        public static readonly Vector3 jailTwo = new Vector3(458.7058f, -998.1188f, 24.91487f);
        public static readonly Vector3 jailThree = new Vector3(459.6695f, -994.0704f, 24.91487f);

        //Jail Shapes
        public ColShape arrestShape;

        public static readonly Vector3 freeJail = new Vector3(427.7434f, -976.0182f, 30.70999f);


        public static Dictionary<Client, long> jailTimer = new Dictionary<Client, long>();

        public void startLspd()
        {
            //Bounds
            var jailShapeOne = API.createSphereColShape(jailOne, 3.7f);
            var jailShapeTwo = API.createSphereColShape(jailTwo, 3.7f);
            var jailShapeThree = API.createSphereColShape(jailThree, 3.7f);
            
            arrestShape = API.createSphereColShape(arrest_loc, 3.7f);

            API.createMarker(2, arrest_loc, new Vector3(), new Vector3(), new Vector3(0.5, 0.5, 0.5), 255, 255, 255, 255);
        }

        public void jailControl(Client player, int seconds)
        {
            int jailOnePlayers = API.getPlayersInRadiusOfPosition(3.7f, jailOne).Count;
            int jailTwoPlayers = API.getPlayersInRadiusOfPosition(3.7f, jailTwo).Count;
            int jailThreePlayers = API.getPlayersInRadiusOfPosition(3.7f, jailThree).Count;
            int smallest = API.getAllPlayers().Count;
            int chosenCell = 0;
            List<int> list = new List<int>(new int[] { jailOnePlayers, jailTwoPlayers, jailThreePlayers });


            //Choose correct cell for player
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] < smallest)
                {
                    smallest = list[i];
                    chosenCell = i;
                }
            }

            if (chosenCell == 0)
                API.setEntityPosition(player, jailOne);
            else if (chosenCell == 1)
                API.setEntityPosition(player, jailTwo);
            else
                API.setEntityPosition(player, jailThree);

            API.removeAllPlayerWeapons(player);
            API.setEntityData(player, "Jailed", true);
            API.sendChatMessageToPlayer(player, "You have been jailed for " + seconds/60 + " minutes.");


            lock (jailTimer) jailTimer.Set(player, API.TickCount + seconds * 1000);
        }

        public void setFree(Client player)
        {
            API.setEntityData(player, "Jailed", false);
            API.setEntityData(player, "JailTime", 0);
            API.sendChatMessageToPlayer(player, "You have done time and are free to go.");
            API.setEntityPosition(player, freeJail);
            lock (jailTimer) jailTimer.Remove(player);

        }

        public bool arrestPointCheck(NetHandle entity)
        {
            return arrestShape.containsEntity(entity);
        }

        public void onUpdate()
        {
            lock (jailTimer)
            {
                for (int i = jailTimer.Count - 1; i >= 0; i--)
                {
                    var timedup = jailTimer.ElementAt(i);
                    if (API.TickCount - timedup.Value > 0)
                    {
                        setFree(timedup.Key);
                    }
                }
            }
        }

    }
}