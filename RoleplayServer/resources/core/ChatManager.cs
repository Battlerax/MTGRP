using System;
using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;

namespace RoleplayServer
{
    public class ChatManager : Script
    {
        public bool newbie_status = true;
        public bool ooc_status = true;
        public bool vip_status = true;


        public ChatManager()
        {
            DebugManager.debugMessage("[ChatM] Initalizing chat manager...");

            API.onChatMessage += OnChatMessage;
            API.onClientEventTrigger += OnClientEventTrigger;

            DebugManager.debugMessage("[ChatM] Chat Manager initalized.");
        } 

        public void OnChatMessage(Client player, string msg, CancelEventArgs e)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");
            
            //Local Chat
            if(account.admin_duty == 0)
            {
                msg = character.character_name + " says: " + msg;
                NearbyMessage(player, 15, msg);
            }
            else
            {
                b_cmd(player, msg);
            }
           
            e.Cancel = true;
        }

        [Command("newbiechat", Alias = "n", GreedyArg = true)]
        public void newbie_cmd(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (newbie_status == false && account.admin_level == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Newbie chat is currently disabled.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            if(c.newbie_cooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You must wait 60 seconds before using newbie chat again.");
                return;
            }

            API.sendChatMessageToAll(Color.NewbieChat, "[N] " + c.character_name + ": " + message);
            if(account.admin_level == 0)
            {
                c.newbie_cooldown = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 60);
            }
        }

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void ooc_cmd(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (ooc_status == false && account.admin_level)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Global OOC chat is currently disabled.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            if (c.ooc_cooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You must wait 60 seconds before using global OOC chat again.");
                return;
            }

            API.sendChatMessageToAll(Color.GlobalOOC, "[OOC] " + c.character_name + ": " + message);
            if (account.admin_level == 0)
            {
                c.ooc_cooldown = (new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 60);
            }
        }

        [Command("vip", Alias ="v", GreedyArg =true)]
        public void vip_chat(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (vip_status == false && account.admin_level == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~VIP chat is currently disabled.");
                return;
            }

            if(account.vip_level == 0)
            {
                API.sendNotificationToPlayer(player, "~y~You must be a VIP to use VIP chat.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            List<Client> players = API.getAllPlayers();
            foreach(Client p in players)
            {
                Account p_account = API.getEntityData(p.handle, "Account");
                if(p_account.vip_level > 0)
                {
                    API.sendChatMessageToPlayer(p, Color.VIPChat, "[V] " + c.character_name + ": " + message);
                }
            }
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
            Character character = API.getEntityData(player.handle, "Character");

            if (API.doesEntityExist(character.ame_text))
            {
                API.deleteEntity(character.ame_text);
                character.ame_timer.Stop();
            }

            character.ame_text = API.createTextLabel(Color.PlayerRoleplay + character.character_name + " " + action, player.position, 15, (float)(0.5), false, player.dimension);
            API.setTextLabelColor(character.ame_text, 194, 162, 218, 255);
            API.attachEntityToEntity(character.ame_text, player.handle, "SKEL_Head", new Vector3(0.0, 0.0, 1.3), new Vector3(0, 0, 0));

            character.ame_timer = new System.Timers.Timer();
            character.ame_timer.Interval = 8000;
            character.ame_timer.Elapsed += delegate { RemoveAmeText(character); };
            character.ame_timer.Start();
        }

        public void RemoveAmeText(Character c)
        {
            if (API.doesEntityExist(c.ame_text))
            {
                API.deleteEntity(c.ame_text);
            }
            c.ame_timer.Stop();
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
            NearbyMessage(player, 25, PlayerManager.getName(player) + " shouts: " + text);
        }

        [Command("low", GreedyArg = true)]
        public void low_cmd(Client player, string text)
        {
            NearbyMessage(player, 5, PlayerManager.getName(player) + " whispers: " + text, Color.Grey);
        }

        [Command("b", GreedyArg = true)]
        public void b_cmd(Client player, string text)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if(account.admin_duty == 0)
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.getName(player) + ": " + text + " ))", Color.OOC);
            }
            else
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.getAdminName(player) + ": " + text + " ))", Color.AdminOrange);
            }
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
