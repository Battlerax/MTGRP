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

        public static void NearbyMessage(Client player, float radius, string msg, string color)
        {
            List<Client> players = API.shared.getPlayersInRadiusOfPlayer(radius, player);
           
            foreach(Client i in players)
            {
                API.shared.sendChatMessageToPlayer(i, color, msg);
            }
        }


        public static void NearbyMessage(Client player, float radius, string msg)
        {
            List<Client> players = API.shared.getPlayersInRadiusOfPlayer(radius, player);

            foreach (Client i in players)
            {
                API.shared.sendChatMessageToPlayer(i, msg);
            }
        }


        public float GetDistanceBetweenPlayers(Client player1, Client player2)
        {
            return player1.position.DistanceTo(player2.position);
        }

        

        [Command("me", GreedyArg = true)]
        public void me_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            RoleplayMessage(playerchar, action, ROLEPLAY_ME, 10, 0);
        }

        [Command("ame", GreedyArg = true)]
        public void ame_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string ame = playerchar.character_name + action;
            Vector3 PlayerPos = API.getEntityPosition(player);
            var textlabel = API.createTextLabel(ame, PlayerPos, 20, 3);
            API.attachEntityToEntity(player, textlabel, "head", PlayerPos , new Vector3(0, 0, 0.5));
        }

        [Command("do", GreedyArg = true)]
        public void do_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            RoleplayMessage(playerchar, action, ROLEPLAY_DO, 10, 0);
        }

        [Command("shout", Alias = "s", GreedyArg = true)]
        public void shout_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = playerchar.character_name + " shouts: " + text;
            NearbyMessage(player, 25, msg);
        }

        [Command("b", GreedyArg = true)]
        public void b_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = "((" + playerchar.character_name + ": " + text + "))";
            NearbyMessage(player, 10, msg);
        }


        [Command("low", GreedyArg = true)]
        public void low_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = playerchar.character_name + " whispers: " + text;
            NearbyMessage(player, 5, msg);
        }


        [Command("rp", GreedyArg = true)]
        public void rp_cmd(Client player, string id, string text)
        {
            Client receiver = PlayerManager.parseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.LongDistanceRoleplay, "RP to " + PlayerManager.getName(receiver) + ": " + text);
            API.sendChatMessageToPlayer(receiver, Color.LongDistanceRoleplay, "RP from " + PlayerManager.getName(player) + ": " + text);
        }

        [Command("whisper", Alias = "w", GreedyArg = true)]
        public void w_cmd(Client player, string id, string text)
        {
            Client receiver = PlayerManager.parseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if(GetDistanceBetweenPlayers(player, receiver) > 4)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You are too far away from that player.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.Whisper, "Whisper to " + PlayerManager.getName(receiver) + ": " + text);
            API.sendChatMessageToPlayer(receiver, Color.Whisper, "Whisper from " + PlayerManager.getName(player) + ": " + text);
            RoleplayMessage(player, "whispers to " + PlayerManager.getName(receiver), ROLEPLAY_ME, 10);
        }


        [Command("pm", GreedyArg = true)]
        public void pm_cmd(Client player, string id, string text)
        {
            Client receiver = PlayerManager.parseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.PM, "PM to " + PlayerManager.getName(receiver) + ": " + text);
            API.sendChatMessageToPlayer(receiver, Color.PM, "PM from " + PlayerManager.getName(player) + ": " + text);
        }


        /*[Command("pay", GreedyArg = true)]
        public void pay_cmd(Client player, Client receiver, int amount)
        {
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            if (playerid.money < amount)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount on you.");
                return;
            }

            if (GetDistanceBetweenPlayers(player, receiver) > 7)
            {
                API.sendChatMessageToPlayer(player, "That player is far from you.");
                return;
            }

            string messagetoplayer = "You have paid $" + amount + " to " + receiverid.character_name + ".";
            API.sendChatMessageToPlayer(player, messagetoplayer);
            string messagetoreceiver = "You have been paid $" + amount + " by" + playerid.character_name + ".";
            API.sendChatMessageToPlayer(receiver, messagetoreceiver);
            string autome = playerid.character_name + " has paid " + receiverid.character_name + " some money.";
            NearbyMessage(player, 10, autome);
            playerid.money -= amount;
            receiverid.money += amount;
        }


        [Command("givecheck", GreedyArg = true)]
        public void givecheck_cmd(Client player, Client receiver, int amount)
        {
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            if (playerid.bank_balance < amount)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank account");
                return;
            }

            if (GetDistanceBetweenPlayers(player, receiver) > 7)
            {
                API.sendChatMessageToPlayer(player, "That player is far from you.");
                return;
            }


            string messagetoplayer = "You have given $" + amount + " to " + receiverid.character_name + ". This has been taken from your bank balance.";
            API.sendChatMessageToPlayer(player, messagetoplayer);
            string messagetoreceiver = "You have been given a check for $" + amount + " from " + playerid.character_name + ".";
            API.sendChatMessageToPlayer(receiver, messagetoreceiver);
            API.sendChatMessageToPlayer(receiver, "You must visit the bank and use /redeemcheck to redeem the check balance.");
            string autome = playerid.character_name + " signs a check and gives it to " + receiverid.character_name + ".";
            NearbyMessage(player, 10, autome);
            playerid.bank_balance -= amount;
            receiverid.checks += amount;
        }*/

        public const int ROLEPLAY_ME = 0;
        public const int ROLEPLAY_DO = 1;

        public static void RoleplayMessage(Character character, string action, int type, float radius = 10, int auto = 1)
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

            string color = "";
            if(auto == 1)
            {
                color = Color.AutoRoleplay;
            }
            else
            {
                color = Color.PlayerRoleplay;
            }

            NearbyMessage(character.client, radius, roleplay_msg, color);
        }

        public static void RoleplayMessage(Client player, string action, int type, float radius = 10, int auto = 1)
        {
            string roleplay_msg = null;

            switch (type)
            {
                case 0: //ME
                    roleplay_msg = "* " + PlayerManager.getName(player) + " " + action;
                    break;
                case 1: //DO
                    roleplay_msg = "* " + action + " ((" + PlayerManager.getName(player) + "))";
                    break;
            }

            string color = "";
            if (auto == 1)
            {
                color = Color.AutoRoleplay;
            }
            else
            {
                color = Color.PlayerRoleplay;
            }

            NearbyMessage(player, radius, roleplay_msg, color);
        }
    }
}
