using System;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.core
{
    public class ChatManager : Script
    {
        public bool NewbieStatus = true;
        public bool OocStatus = true;
        public bool VipStatus = true;


        public ChatManager()
        {
            DebugManager.DebugMessage("[ChatM] Initalizing chat manager...");

            API.onChatMessage += OnChatMessage;
            API.onClientEventTrigger += OnClientEventTrigger;

            DebugManager.DebugMessage("[ChatM] Chat Manager initalized.");
        } 

        public void OnChatMessage(Client player, string msg, CancelEventArgs e)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            //Local Chat
            if (API.getEntityData(player, "MegaphoneStatus") == true)
            {
                msg = "[MEGAPHONE] " + character.rp_name() + " says: " +  msg;
                NearbyMessage(player, 30, msg);
                return;
            }

            if (API.getEntityData(player, "MicStatus") == true)
            {
                msg = "~p~ [BROADCAST] " + character.CharacterName + " : " + msg;
                broadcastMessage(msg);
                return;
            }
            if (account.AdminDuty == false)
            {
                msg = character.rp_name() + " says: " + msg;
                NearbyMessage(player, 15, msg);
                return;
            }
            else
            {
                b_cmd(player, msg);
            }
           
            e.Cancel = true;
        }

        public void broadcastMessage(string msg)
        {
            foreach (var i in API.getAllPlayers())
            {
                Character character = API.getEntityData(i.handle, "Character");
                if(character.IsWatchingBroadcast == true)
                {
                    API.sendChatMessageToPlayer(i, msg);
                }
            }
        }

        [Command("newbiechat", Alias = "n", GreedyArg = true)]
        public void newbie_cmd(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (NewbieStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Newbie chat is currently disabled.");
                return;
            }

            if (character.VMutedExpiration > DateTime.Now)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You are muted from newbie chat.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            if(c.NewbieCooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You must wait 60 seconds before using newbie chat again.");
                return;
            }

            API.sendChatMessageToAll(Color.NewbieChat, "[N] " + c.CharacterName + ": " + message);
            if(account.AdminLevel == 0)
            {
                c.NewbieCooldown = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 60;
            }
        }

       

        [Command("ooc", Alias = "o", GreedyArg = true)]
        public void ooc_cmd(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if (OocStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Global OOC chat is currently disabled.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            if (c.OocCooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You must wait 60 seconds before using global OOC chat again.");
                return;
            }

            API.sendChatMessageToAll(Color.GlobalOoc, "[OOC] " + c.CharacterName + ": " + message);
            if (account.AdminLevel == 0)
            {
                c.OocCooldown = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 60;
            }
        }

        [Command("vip", Alias ="v", GreedyArg =true)]
        public void vip_chat(Client player, string message)
        {
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (VipStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~VIP chat is currently disabled.");
                return;
            }

            if (character.VMutedExpiration > DateTime.Now)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You are muted from VIP chat.");
                return;
            }

            if (account.VipLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~y~You must be a VIP to use VIP chat.");
                return;
            }

            Character c = API.getEntityData(player.handle, "Character");

            var players = API.getAllPlayers();
            foreach(var p in players)
            {
                Account pAccount = API.getEntityData(p.handle, "Account");
                if(pAccount.VipLevel > 0)
                {
                    API.sendChatMessageToPlayer(p, Color.VipChat, "[V] " + c.CharacterName + ": " + message);
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
            var players = API.shared.getPlayersInRadiusOfPlayer(radius, player);
           
            foreach(var i in players)
            {
                API.shared.sendChatMessageToPlayer(i, color, msg);
            }
        }


        public static void NearbyMessage(Client player, float radius, string msg)
        {
            var players = API.shared.getPlayersInRadiusOfPlayer(radius, player);

            foreach (var i in players)
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
            RoleplayMessage(playerchar, action, RoleplayMe, 10, 0);
        }

        [Command("ame", GreedyArg = true)]
        public void ame_cmd(Client player, string action)
        {
            Character character = API.getEntityData(player.handle, "Character");
            AmeLabelMessage(player, action, 8000);
        }

       
        [Command("do", GreedyArg = true)]
        public void do_cmd(Client player, string action)
        {
            Character playerchar = API.shared.getEntityData(player.handle, "Character");
            RoleplayMessage(playerchar, action, RoleplayDo, 10, 0);
        }

        [Command("shout", Alias = "s", GreedyArg = true)]
        public void shout_cmd(Client player, string text)
        {
            NearbyMessage(player, 25, PlayerManager.GetName(player) + " shouts: " + text);
        }

        [Command("low", GreedyArg = true)]
        public void low_cmd(Client player, string text)
        {
            NearbyMessage(player, 5, PlayerManager.GetName(player) + " whispers: " + text, Color.Grey);
        }

        [Command("b", GreedyArg = true)]
        public void b_cmd(Client player, string text)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if(account.AdminDuty == false)
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.GetName(player) + ": " + text + " ))", Color.Ooc);
            }
            else
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.GetAdminName(player) + ": " + text + " ))", Color.AdminOrange);
            }
        }
        
        [Command("admin", Alias = "a", GreedyArg = true)]
        public void admin_cmd(Client player,  string text)
        {
            Account account = API.getEntityData(player.handle, "Account");

            if(account.AdminLevel > 0)
            {
                foreach (var c in API.getAllPlayers())
                {
                    Account receiverAccount = API.getEntityData(player.handle, "Account");

                    if (receiverAccount.AdminLevel > 0)
                    {
                        API.sendChatMessageToPlayer(c, Color.AdminChat, "[A] " + account.AdminName + text);
                    }
                }
            }
        }

        [Command("rp", GreedyArg = true)]
        public void rp_cmd(Client player, string id, string text)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.LongDistanceRoleplay, "RP to " + PlayerManager.GetName(receiver) + ": " + text);
            API.sendChatMessageToPlayer(receiver, Color.LongDistanceRoleplay, "RP from " + PlayerManager.GetName(player) + ": " + text);
        }

        [Command("whisper", Alias = "w", GreedyArg = true)]
        public void w_cmd(Client player, string id, string text)
        {
            var receiver = PlayerManager.ParseClient(id);

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

            API.sendChatMessageToPlayer(player, Color.Whisper, "Whisper to " + PlayerManager.GetName(receiver) + ": " + text);
            API.sendChatMessageToPlayer(receiver, Color.Whisper, "Whisper from " + PlayerManager.GetName(player) + ": " + text);
            RoleplayMessage(player, "whispers to " + PlayerManager.GetName(receiver), RoleplayMe);
        }


        [Command("pm", GreedyArg = true)]
        public static void pm_cmd(Client player, string id, string text)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.shared.sendChatMessageToPlayer(player, Color.Pm, "PM to " + PlayerManager.GetName(receiver) + ": " + text);
            API.shared.sendChatMessageToPlayer(receiver, Color.Pm, "PM from " + PlayerManager.GetName(player) + ": " + text);
        }

        public const int RoleplayMe = 0;
        public const int RoleplayDo = 1;

        public static void RoleplayMessage(Character character, string action, int type, float radius = 10, int auto = 1)
        {
            string roleplayMsg = null;

            switch (type)
            {
                case 0: //ME
                    roleplayMsg = "* " + character.CharacterName + " " + action; 
                    break;
                case 1: //DO
                    roleplayMsg = "* " + action + " ((" + character.CharacterName + "))";
                    break;
            }

            var color = auto == 1 ? Color.AutoRoleplay : Color.PlayerRoleplay;

            NearbyMessage(character.Client, radius, roleplayMsg, color);
        }

        public static void RoleplayMessage(Client player, string action, int type, float radius = 10, int auto = 1)
        {
            string roleplayMsg = null;

            switch (type)
            { 
                case 0: //ME
                    roleplayMsg = "* " + PlayerManager.GetName(player) + " " + action;
                    break;
                case 1: //DO
                    roleplayMsg = "* " + action + " ((" + PlayerManager.GetName(player) + "))";
                    break;
            }

            var color = auto == 1 ? Color.AutoRoleplay : Color.PlayerRoleplay;
            NearbyMessage(player, radius, roleplayMsg, color);
        }

        public static void AmeLabelMessage(Client player, string action, int time)
        {
            Character character = API.shared.getEntityData(player.handle, "Character");
            if (API.shared.doesEntityExist(character.AmeText))
            {
                API.shared.deleteEntity(character.AmeText);
                character.AmeTimer.Stop();
            }

            character.AmeText = API.shared.createTextLabel(Color.PlayerRoleplay + character.CharacterName + " " + action, player.position, 15, (float)(0.5), false, player.dimension);
            API.shared.setTextLabelColor(character.AmeText, 194, 162, 218, 255);
            API.shared.attachEntityToEntity(character.AmeText, player.handle, "SKEL_Head", new Vector3(0.0, 0.0, 1.3), new Vector3(0, 0, 0));

            character.AmeTimer = new System.Timers.Timer {Interval = time};
            character.AmeTimer.Elapsed += delegate { RemoveAmeText(character); };
            character.AmeTimer.Start();
        }

        public static void RemoveAmeText(Character c)
        {
            if (API.shared.doesEntityExist(c.AmeText))
            {
                API.shared.deleteEntity(c.AmeText);
            }
            c.AmeTimer.Stop();
        }
    }
}
