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

        [Command("me")]
        public void me_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            RoleplayMessage(playerchar, action, ROLEPLAY_ME, 10);
        }

        [Command("ame")]
        public void ame_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string ame = playerchar.character_name + action;
            Vector3 PlayerPos = API.getEntityPosition(player);
            var textlabel = API.createTextLabel(ame, PlayerPos, 20, 3);
            API.attachEntityToEntity(player, textlabel, "head", PlayerPos , 0);
        }

        [Command("do")]
        public void do_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            RoleplayMessage(playerchar, action, ROLEPLAY_DO, 10);
        }

        [Command("shout")]
        public void shout_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = playerchar.character_name + " shouts: " + text;
            NearbyMessage(player, 25, msg);
        }

        [Command("b")]
        public void b_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = "((" + playerchar.character_name + ": " + text + "))";
            NearbyMessage(player, 10, msg);
        }


        [Command("low")]
        public void low_cmd(Client player, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            string msg = playerchar.character_name + " whispers: " + text;
            NearbyMessage(player, 5, msg);
        }


        [Command("rp")]
        public void rp_cmd(Client player, Client receiver, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            string messagetoplayer = "RP To " + receiverid.character_name + ": " + text;
            API.sendChatMessageToPlayer(player, messagetoplayer);
            string messagetoreceiver = "RP From " + playerchar.character_name + ": " + text;
            API.sendChatMessageToPlayer(receiver, messagetoreceiver);
        }

        [Command("w")]
        public void w_cmd(Client player, Client receiver, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            string messagetoplayer = "Whisper sent to "+receiverid.character_name + " : " + text;
            API.sendChatMessageToPlayer(player, messagetoplayer);
            string messagetoreceiver = "Whisper from " + playerchar.character_name + " : " + text;
            API.sendChatMessageToPlayer(receiver, messagetoreceiver);
            string autome = playerchar.character_name + " has whispered something to " + receiverid.character_name + ".";
        }


        [Command("pm")]
        public void pm_cmd(Client player, Client receiver, string text)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            string messagetoplayer = "PM To " + receiverid.character_name + ": " + text;
            API.sendChatMessageToPlayer(player, messagetoplayer);
            string messagetoreceiver = "PM From " + playerchar.character_name + ": " + text;
            API.sendChatMessageToPlayer(receiver, messagetoreceiver);
        }


        [Command("pay")]
        public void pay_cmd(Client player, Client receiver, int amount)
        {
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            if (playerid.money < amount)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount on you.");
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


        [Command("givecheck")]
        public void givecheck_cmd(Client player, Client receiver, int amount)
        {
            Character playerid = API.shared.getEntityData(player.handle, "Character");
            Character receiverid = API.shared.getEntityData(receiver.handle, "Character");
            if (playerid.bank_balance < amount)
            {
                API.sendChatMessageToPlayer(player, "You don't have that amount in your bank account");
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
