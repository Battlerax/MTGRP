using System;
using System.Linq;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared.Math;
using mtgvrp.core.Discord;
using mtgvrp.core.Help;
using mtgvrp.group_manager.lspd.MDC;
using mtgvrp.inventory;
using mtgvrp.phone_manager;
using mtgvrp.player_manager;

namespace mtgvrp.core
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
            API.onChatCommand += API_onChatCommand;
            API.onClientEventTrigger += OnClientEventTrigger;

            DebugManager.DebugMessage("[ChatM] Chat Manager initalized.");
        }

        private void API_onChatCommand(Client sender, string command, CancelEventArgs cancel)
        {
            if (command.EndsWith("login") || command.EndsWith("register"))
                return;

            LogManager.Log(LogManager.LogTypes.Commands, $"{sender.GetCharacter()?.CharacterName}[{sender.GetAccount()?.AccountName}] has executed: " + command);
        }

        public void OnChatMessage(Client player, string msg, CancelEventArgs e)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (account == null || character == null || account.IsLoggedIn == false)
            {
                e.Cancel = true;
                return;
            }

            if (character.IsRagged)
            {
                API.sendChatMessageToPlayer(player, "You are ragged.");
                e.Cancel = true;
                return;
            }

            if (API.hasEntityData(player, "MegaphoneStatus"))
            {
                if (API.getEntityData(player, "MegaphoneStatus") == true)
                {
                    msg = "~y~[MEGAPHONE] " + character.rp_name() + " says: " + msg;
                    NearbyMessage(player, 30, msg);
                    e.Cancel = true;
                    LogManager.Log(LogManager.LogTypes.ICchat,
                        "[MEGAPHONE] " + character.CharacterName + $"[{account.AccountName}]" + " says: " + msg);
                    return;
                }
            }

            if (API.hasEntityData(player, "MicStatus"))
            {
                if (API.getEntityData(player, "MicStatus") == true)
                {
                    msg = "~p~ [BROADCAST] " + character.CharacterName + " : " + msg;
                    BroadcastMessage(msg);
                    NearbyMessage(player, 30, msg);
                    e.Cancel = true;
                    LogManager.Log(LogManager.LogTypes.ICchat, "[BROADCAST] " + character.CharacterName + $"[{account.AccountName}]" + " says: " + msg);
                    return;
                }
            }

            //Phone
            if (account.AdminDuty == false && character.InCallWith != Character.None)
            {
                Character talkingTo = character.InCallWith;
                string phonemsg;
                var charitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                var targetitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                var charphone = (Phone)charitems[0];
                var targetphone = (Phone)targetitems[0];
                var newmsg = "[Phone]" + character.rp_name() + " says: " + msg;
                ChatManager.NearbyMessage(player, 15, newmsg, Color.Grey);
                if (targetphone.HasContactWithNumber(charphone.PhoneNumber))
                {
                    phonemsg = "[" + targetphone.Contacts.Find(pc => pc.Number == charphone.PhoneNumber).Name + "]" +
                               character.rp_name() + " says: " + msg;
                }
                else
                {
                    phonemsg = "[" + charphone.PhoneNumber + "]" + character.rp_name() + " says: " + msg;
                }
                API.sendChatMessageToPlayer(talkingTo.Client, Color.Grey, phonemsg);
                e.Cancel = true;
                e.Reason = "Phone";
                LogManager.Log(LogManager.LogTypes.Phone, $"[Phone] {character.CharacterName}[{account.AccountName}] To {talkingTo.CharacterName}[{talkingTo.Client.socialClubName}]: {msg}");
                return;
            }
            else if (account.AdminDuty == false && character.Calling911 == true)
            {
                //API.getZoneName(player.position);

                var charitems = InventoryManager.DoesInventoryHaveItem(character, typeof(Phone));
                var charphone = (Phone)charitems[0];

                Mdc.Add911Call(charphone.PhoneNumber, msg, "Los Santos");

                var newmsg = "[Phone]" + character.rp_name() + " says: " + msg;
                ChatManager.NearbyMessage(player, 15, newmsg, Color.Grey);

                API.sendChatMessageToPlayer(player, Color.Grey, "911 Operator says: Thank you for reporting your emergency, a unit will be dispatched shortly.");
                PhoneManager.h_cmd(player);
                group_manager.lspd.Lspd.SendToCops(player, $"~r~911: #{charphone.PhoneNumber} reported a crime: {msg}");
                e.Cancel = true;
                e.Reason = "Phone";
                LogManager.Log(LogManager.LogTypes.Phone, $"[Phone] {character.CharacterName}[{account.AccountName}] To LSPD(911): {msg}");
                return;
            }

            if (account.AdminDuty == false)
            {
                msg = character.rp_name() + " says: " + msg;
                NearbyMessage(player, 15, msg);
                LogManager.Log(LogManager.LogTypes.ICchat, $"{character.CharacterName}[{account.AccountName}] says: {msg}");
                e.Cancel = true;
            }
            else
            {
                b_cmd(player, msg);

                //Not sure where to log this so just why not both lmao
                LogManager.Log(LogManager.LogTypes.ICchat, $"((Admin {account.AdminName} says: {msg}))");
                LogManager.Log(LogManager.LogTypes.OOCchat, $"((Admin {account.AdminName} says: {msg}))");

                e.Cancel = true;
            }
        }

        public void BroadcastMessage(string msg)
        {
            foreach (var i in PlayerManager.Players)
            {
                if(i.IsWatchingBroadcast == true)
                {
                    API.sendChatMessageToPlayer(i.Client, msg);
                }
            }
        }

        [Command("rand", GreedyArg = true), Help.Help(HelpManager.CommandGroups.General, "Generate a random number.", "The upper limit")]
        public void startRand(Client sender, String upperBoundary)
        {
            const int maxLimit = 100;
            int upperlimit;
            if (Int32.TryParse(upperBoundary, out upperlimit))
            {
                if (upperlimit <= maxLimit && upperlimit > 0)
                {
                    int outcome = new Random().Next(0, upperlimit + 1);
                    
                    NearbyMessage(sender, 10, " [RAND]: (( "  + sender.GetCharacter().CharacterName +  " has randomised the number " + outcome + " out of " + upperlimit + " ))",Color.Ooc);
                    return;

                }
            }
            API.sendChatMessageToPlayer(sender, "SYNTAX : /rand 1-" + maxLimit);

        }

        [Command("dice", GreedyArg = true), Help.Help(HelpManager.CommandGroups.General, "Roll multiple dice.", "The number of dice")]
        public void Dice(Client player, string diceNo)
        {
            const int upperDiceLimit = 2;
            int numOfDie;
            int diceRoll;
            // Generate a random BEFORE the actual loop, same values are extremely likely when done inside of the loop, due to the seed being Sys time.
            Random roll = new Random();
            if (Int32.TryParse(diceNo, out numOfDie))
            {
                if (numOfDie > upperDiceLimit || numOfDie < 1)
                {
                    API.sendChatMessageToPlayer(player, "SYNTAX : /roll 1/2");
                    return;
                }

                int[] diceArr = new int[numOfDie];
                for (int x = 0; x <= numOfDie - 1; x++)
                {
                    diceRoll = roll.Next(1, 7);
                    diceArr[x] = diceRoll;
                }
                if(numOfDie == 1) RoleplayMessage(player, "has rolled a dice and it lands on " + diceArr[0],RoleplayMe);
                else RoleplayMessage(player, "has rolled " + numOfDie + " die and they landed on " + string.Join(" and ",diceArr),RoleplayMe);
            }

        }

        [Command("togglenewbie"), Help.Help(HelpManager.CommandGroups.AdminLevel2, "Used to toggle newbie chat on and off.", null)]
        public void togglenewbie_cmd(Client player)
        {
            if(player.GetAccount().AdminLevel < 2)
            {
                return;
            }

            NewbieStatus = !NewbieStatus;
            API.sendChatMessageToAll("Newbie chat has been toggled " + ((NewbieStatus == true) ? ("on") : ("off")) + " by an admin.");
            LogManager.Log(LogManager.LogTypes.AdminActions, player.GetAccount().AdminName + " has toggled newbie chat " + ((NewbieStatus == true) ? ("on") : ("off")) + ".");
            return;
        }


        [Command("toggleooc"), Help.Help(HelpManager.CommandGroups.AdminLevel2, "Used to toggle ooc chat on and off.", null)]
        public void toggleooc_cmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 2)
            {
                return;
            }

            OocStatus = !OocStatus;
            API.sendChatMessageToAll("OOC chat has been toggled " + ((OocStatus == true) ? ("on") : ("off")) + " by an admin.");
            LogManager.Log(LogManager.LogTypes.AdminActions, player.GetAccount().AdminName + " has toggled ooc chat " + ((OocStatus == true) ? ("on") : ("off")) + ".");
            return;
        }

        [Command("togglevip"), Help.Help(HelpManager.CommandGroups.AdminLevel2, "Used to toggle VIP chat on and off.", null)]
        public void togglevip_cmd(Client player)
        {
            if (player.GetAccount().AdminLevel < 2)
            {
                return;
            }

            VipStatus = !VipStatus;
            API.sendChatMessageToAll("VIP chat has been toggled " + ((VipStatus == true) ? ("on") : ("off")) + " by an admin.");
            LogManager.Log(LogManager.LogTypes.AdminActions, player.GetAccount().AdminName + " has toggled VIP chat " + ((VipStatus == true) ? ("on") : ("off")) + ".");
            return;
        }

        [Command("newbiechat", Alias = "n", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Talk in the newbie chat to get help.", "Your question")]
        public void newbie_cmd(Client player, string message)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (NewbieStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Newbie chat is currently disabled.");
                return;
            }

            if (character.NMutedExpiration > TimeManager.GetTimeStamp)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You are muted from newbie chat.");
                return;
            }

            Character c = player.GetCharacter();

            if (c.NewbieCooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player,
                    "~r~ERROR:~w~You must wait 60 seconds before using newbie chat again.");
                return;
            }

//Figure rank.
            string rank = "";
            if (account.DevLevel == 1) rank = "Developer";
            else if(account.AdminLevel == 1) rank = "Moderator";
            else if (account.AdminLevel > 1) rank = "Admin";
            else if (account.VipLevel >= 1) rank = "VIP";
            else if (account.TotalPlayingHours < 2) rank = "Guest";
            else if (account.TotalPlayingHours >= 2 && account.TotalPlayingHours < 75) rank = "Player";
            else if (account.TotalPlayingHours >= 75 && account.TotalPlayingHours < 250) rank = "MTG-Player";
            else if (account.TotalPlayingHours >= 250 && account.TotalPlayingHours < 750) rank = "MTG-Pro";
            else if (account.TotalPlayingHours >= 750 && account.TotalPlayingHours < 1250) rank = "MTG-All Star";
            else if (account.TotalPlayingHours >= 1250 && account.TotalPlayingHours < 2000) rank = "MTG-Legend";
            else if (account.TotalPlayingHours >= 2000) rank = "MTG-Icon";

            API.sendChatMessageToAll(Color.NewbieChat, $"[N] {rank} " + c.CharacterName + ": " + message);
            if (account.AdminLevel == 0 && account.DevLevel == 0)
            {
                c.NewbieCooldown = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 60;
            }
        }



        [Command("ooc", Alias = "o", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Talk on the global OOC channel.", "The message")]
        public void ooc_cmd(Client player, string message)
        {
            Account account = player.GetAccount();

            if (OocStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~Global OOC chat is currently disabled.");
                return;
            }

            Character c = player.GetCharacter();

            if (c.OocCooldown > new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds())
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You must wait 5 seconds before using global OOC chat again.");
                return;
            }

            API.sendChatMessageToAll(Color.GlobalOoc, "[OOC] " + c.CharacterName + ": " + message);
            LogManager.Log(LogManager.LogTypes.OOCchat, "[OOC] " + c.CharacterName +$"[{account.AccountName}]" + ": " + message);
            if (account.AdminLevel == 0)
            {
                c.OocCooldown = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + 5;
            }
        }

        [Command("vip", Alias ="v", GreedyArg =true), Help.Help(HelpManager.CommandGroups.Chat, "Talk in the VIP channel.", "The message")]
        public void vip_chat(Client player, string message)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            if (VipStatus == false && account.AdminLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~VIP chat is currently disabled.");
                return;
            }

            if (character.VMutedExpiration > TimeManager.GetTimeStamp)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~You are muted from VIP chat.");
                return;
            }

            if (account.VipLevel == 0)
            {
                API.sendNotificationToPlayer(player, "~y~You must be a VIP to use VIP chat.");
                return;
            }

            Character c = player.GetCharacter();

            var players = API.getAllPlayers();
            foreach(var p in players)
            {
                if (p == null)
                    continue;

                Account pAccount = p.GetAccount();

                if(pAccount?.VipLevel > 0)
                {
                    API.sendChatMessageToPlayer(p, Color.VipChat, "[V] " + c.CharacterName + ": " + message);
                }
            }
            DiscordManager.SendVIPMessage("[V] " + c.CharacterName + $"[{account.AccountName}]" + ": " + message);
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
            foreach(var i in API.shared.getAllPlayers())
            {
                if (i == null)
                    continue;

                if (i.position.DistanceTo(player.position) > radius)
                    continue;

                API.shared.sendChatMessageToPlayer(i, color, msg);
            }
        }


        public static void NearbyMessage(Client player, float radius, string msg)
        {   
            foreach (var i in API.shared.getAllPlayers())
            {
                if(i == null)
                    continue;

                if(i.position.DistanceTo(player.position) > radius || i.dimension != player.dimension)
                    continue;

                API.shared.sendChatMessageToPlayer(i, msg);
            }
        }


        public float GetDistanceBetweenPlayers(Client player1, Client player2)
        {
            return player1.position.DistanceTo(player2.position);
        }

        

        [Command("me", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Roleplay, "Descrive an action you're doing.", "The action")]
        public void me_cmd(Client player, string action)
        {
            Character playerchar = player.GetCharacter();
            RoleplayMessage(playerchar, action, RoleplayMe, 10, 0);
        }

        [Command("ame", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Roleplay, "Describe an action you're doing, shows on top of your head.", "The action")]
        public void ame_cmd(Client player, string action)
        {
            Character character = player.GetCharacter();
            AmeLabelMessage(player, action, 8000);
        }

       
        [Command("do", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Roleplay, "Describe an event that's occuring near you.", "The action")]
        public void do_cmd(Client player, string action)
        {
            Character playerchar = player.GetCharacter();
            RoleplayMessage(playerchar, action, RoleplayDo, 10, 0);
        }

        [Command("shout", Alias = "s", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Sends a mesage to nearby players but with a high range. (Shouts)", "The message")]
        public void shout_cmd(Client player, string text)
        {
            if (API.hasEntityData(player, "IS_MOUTH_RAGGED"))
            {
                API.sendChatMessageToPlayer(player, "You are ragged.");
                return;
            }
            NearbyMessage(player, 25, PlayerManager.GetName(player) + " shouts: " + text);
        }

        [Command("low", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Sends a message to nearby players with a low range.", "The message")]
        public void low_cmd(Client player, string text)
        {
            if (API.hasEntityData(player, "IS_MOUTH_RAGGED"))
            {
                API.sendChatMessageToPlayer(player, "You are ragged.");
                return;
            }
            NearbyMessage(player, 5, PlayerManager.GetName(player) + " whispers: " + text, Color.Grey);
        }

        [Command("b", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Sends a local OOC chat.", "The message")]
        public void b_cmd(Client player, string text)
        {
            Account account = player.GetAccount();
            if(account.AdminDuty == false)
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.GetName(player) + ": " + text + " ))", Color.Ooc);
                LogManager.Log(LogManager.LogTypes.ICchat, $"[B] {player.GetCharacter().CharacterName}[{account.AccountName}] says: {text}");
            }
            else
            {
                NearbyMessage(player, 10, "(( " + PlayerManager.GetAdminName(player) + ": " + text + " ))", Color.AdminOrange);
                LogManager.Log(LogManager.LogTypes.ICchat, $"[B] Admin {account.AdminName} says: {text}");
            }
            
        }
        
        [Command("admin", Alias = "a", GreedyArg = true), Help.Help(HelpManager.CommandGroups.AdminLevel1, "Talk in admin channel.", "The message")]
        public void admin_cmd(Client player,  string text)
        {
            Account account = player.GetAccount();

            if(account.AdminLevel > 0)
            {
                foreach (var c in API.getAllPlayers())
                {
                    if (c == null)
                        continue;

                    Account receiverAccount = c.GetAccount();

                    if (receiverAccount?.AdminLevel > 0)
                    {
                        API.sendChatMessageToPlayer(c, Color.AdminChat, "[A] " + account.AdminName + ": " + text);
                    }
                }
                DiscordManager.SendAdminMessage("[A] " + account.AdminName + ": " + text);
            }
        }

        [Command("rp", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Roleplay, "Send an RP message to someone far away not in range of /me or /do", "Id of player", "The RP")]
        public void rp_cmd(Client player, string id, string text)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.sendChatMessageToPlayer(player, Color.LongDistanceRoleplay, "RP to " + PlayerManager.GetName(receiver) + "(" + PlayerManager.GetPlayerId(receiver.GetCharacter()) + "): " + text);
            API.sendChatMessageToPlayer(receiver, Color.LongDistanceRoleplay, "RP from " + PlayerManager.GetName(player) + "(" + PlayerManager.GetPlayerId(player.GetCharacter()) + "): " + text);
        }

        [Command("whisper", Alias = "w", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "Simply whisper someone.", "Id of player", "The message to whisper")]
        public void w_cmd(Client player, string id, string text)
        {
            if (API.hasEntityData(player, "IS_MOUTH_RAGGED"))
            {
                API.sendChatMessageToPlayer(player, "You are ragged.");
                return;
            }

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


        [Command("pm", GreedyArg = true), Help.Help(HelpManager.CommandGroups.Chat, "PM/DM a player.", "Id of player", "PM text")]
        public static void pm_cmd(Client player, string id, string text)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.shared.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            API.shared.sendChatMessageToPlayer(player, Color.Pm, "PM to " + PlayerManager.GetName(receiver) + "(" + PlayerManager.GetPlayerId(receiver.GetCharacter()) + "): " + text);
            API.shared.sendChatMessageToPlayer(receiver, Color.Pm, "PM from " + PlayerManager.GetName(player) + "(" + PlayerManager.GetPlayerId(player.GetCharacter()) + "): " + text);
            LogManager.Log(LogManager.LogTypes.PMchat, "PM to " + PlayerManager.GetName(receiver) + "(" + PlayerManager.GetPlayerId(receiver.GetCharacter()) + "): " + text);
        }

        public const int RoleplayMe = 0;
        public const int RoleplayDo = 1;

        public static void RoleplayMessage(Character character, string action, int type, float radius = 10, int auto = 1)
        {
            string roleplayMsg = null;

            switch (type)
            {
                case 0: //ME
                    roleplayMsg = "* " + character.rp_name() + " " + action; 
                    break;
                case 1: //DO
                    roleplayMsg = "* " + action + " ((" + character.rp_name() + "))";
                    break;
            }

            var color = auto == 1 ? Color.AutoRoleplay : Color.PlayerRoleplay;

            NearbyMessage(character.Client, radius, roleplayMsg, color);
        }

        public static void RoleplayMessage(Client player, string action, int type, float radius = 10, int auto = 1)
        {
            string roleplayMsg = null;
            Character currChar = player.GetCharacter();

            if (currChar == null)
            {
                return;
            }
            
            switch (type)
            { 
                case 0: //ME
                    roleplayMsg = "* " + currChar.rp_name() + " " + action;
                    break;
                case 1: //DO
                    roleplayMsg = "* " + action + " ((" + currChar.rp_name() + "))";
                    break;
            }

            var color = auto == 1 ? Color.AutoRoleplay : Color.PlayerRoleplay;
            NearbyMessage(player, radius, roleplayMsg, color);
        }

        public static void AmeLabelMessage(Client player, string action, int time)
        {
            Character character = player.GetCharacter();
            if (API.shared.doesEntityExist(character.AmeText))
            {
                API.shared.deleteEntity(character.AmeText);
                character.AmeTimer.Stop();
            }

            character.AmeText = API.shared.createTextLabel(Color.PlayerRoleplay + "* " + character.rp_name() + " " + action, player.position, 15, (float)(0.5), false, player.dimension);
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
