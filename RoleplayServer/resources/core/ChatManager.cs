using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class ChatManager : Script
    {
        public ChatManager()
        {
            DebugManager.debugMessage("[ChatM] Initalizing chat manager...");

            API.onChatMessage += OnChatMessage;
            API.onClientEventTrigger += OnClientEventTrigger;

            DebugManager.debugMessage("[ChatM] Chat Manager initalized.");
        }

        public void OnChatMessage(Client player, string msg, CancelEventArgs e)
        {
            Character character = API.getEntityData(player, "Character");
            
            //Local Chat
            msg = character.character_name + " says: " + msg;
            NearbyMessage(player, 15, msg);
            e.Cancel = true;
        }

        public void OnClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            if(eventName == "NearbyMessage")
            {
                NearbyMessage(player, (float)arguments[0], (string)arguments[1]);
            }
        }

        public static void NearbyMessage(Client player, float radius, string msg)
        {
            List<Client> players = API.shared.getPlayersInRadiusOfPlayer(radius, player);

            foreach(Client i in players)
            {
                API.shared.sendChatMessageToPlayer(i, msg);
            }
        }

        public const int ROLEPLAY_ME = 0;
        public const int ROLEPLAY_DO = 1;

        public static void RoleplayMessage(Character character, string action, int type, float radius = 10)
        {
            string roleplay_msg = null;

            switch (type)
            {
                case 0: //ME
                    roleplay_msg = "* " + character.character_name + " " + action; 
                    break;
                case 1: //DO
                    roleplay_msg = "* " + action + " ((" + character.character_name + "))";
                    break;
            }

            NearbyMessage(character.client, radius, roleplay_msg);
        }
    }
}
