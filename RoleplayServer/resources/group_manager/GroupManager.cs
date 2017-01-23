using System.Collections.Generic;
using GTANetworkServer;
using MongoDB.Driver;
using RoleplayServer.resources.core;
using RoleplayServer.resources.database_manager;
using RoleplayServer.resources.player_manager;

namespace RoleplayServer.resources.group_manager
{
    public class GroupManager : Script
    {

        public static List<Group> Groups = new List<Group>();

        public GroupManager()
        {
            DebugManager.DebugMessage("[GroupM] Initalizing group manager...");

            load_groups(); 

            API.onPlayerConnected += OnPlayerConnected;
            API.onPlayerDisconnected += OnPlayerDisconnected;

            DebugManager.DebugMessage("[GroupM] Group Manager initalized!");

        }

        public void OnPlayerConnected(Client player)
        {
            Character connected = API.getEntityData(player.handle, "Character");
            foreach (var c in PlayerManager.Players)
            {
                if (c.GroupId == connected.GroupId)
                {
                    API.sendChatMessageToPlayer(c.Client, connected.CharacterName + "from your group has connected to the server.");
                }
            }
        }

        public void OnPlayerDisconnected(Client player, string reason)
        {
            Character disconnected = API.getEntityData(player.handle, "Character");
            foreach (var c in PlayerManager.Players)
            {
                if (c.GroupId == disconnected.GroupId)
                {
                    API.sendChatMessageToPlayer(c.Client, disconnected.CharacterName + "from your group has disconnected ("+reason+").");
                }
            }
        }


        [Command("listgroups")]
        public void listgroups_cmd(Client player)
        {
            Account acc = API.getEntityData(player.handle, "Account");

            if (acc.AdminLevel < 4)
                return;

            Groups = DatabaseManager.GroupTable.Find<Group>(Builders<Group>.Filter.Empty).ToList<Group>();
            API.sendChatMessageToPlayer(player, "-----------------------------------------------------------------------------------");
            foreach (var g in Groups)
            {
                switch(g.GroupType)
                {
                    case 1: API.sendChatMessageToPlayer(player, "Group Name: " + g.GroupName + "| Type: Faction | Command Type: " + g.GroupCommandType + "."); break;
                    case 2: API.sendChatMessageToPlayer(player, "Group Name: " + g.GroupName + "| Type: Gang | Command Type: " + g.GroupCommandType + "."); break;
                }
            }
            API.sendChatMessageToPlayer(player, "---------------------------------------------------------------------------------- ");
        }

        [Command("remoteuninvite")]
        public void remoteunivite_cmd(Client player, string playername)
        {
            Character sender = API.getEntityData(player.handle, "Character");

            if (sender.GroupId > 0 && sender.GroupRank > 6)
            {
                var filter = Builders<Character>.Filter.Eq("CharacterName", playername);
                var foundCharacters = DatabaseManager.CharacterTable.Find(filter).ToList();

                foreach (var c in foundCharacters)
                {
                    if (c.GroupId == sender.GroupId)
                    {
                        c.GroupId = 0;
                        c.Group = null;
                        c.GroupRank = 0;
                        c.Save();
                    }
                    API.sendChatMessageToPlayer(player, "You have remote-uninvited " + c.CharacterName + "from the " + sender.Group.GroupName + ".");
                }
            }
        }

        [Command("setrank")]
        public void setrank_cmd(Client player, string id, int rank)
        {
            var  receiver = PlayerManager.ParseClient(id);
            Character member = API.getEntityData(receiver.handle, "Character");
            Character sender = API.getEntityData(player.handle, "Character");

            if(sender.GroupId == member.GroupId)
            {
                var oldrank = member.GroupRank;
                if(oldrank > rank)
                {
                    API.sendChatMessageToPlayer(receiver, "You have been demoted to " + member.Group.GroupRankNames[rank] + " by " + sender.CharacterName + ".");
                }
                else
                {
                    API.sendChatMessageToPlayer(receiver, "You have been promoted to " + member.Group.GroupRankNames[rank] + " by " + sender.CharacterName + ".");
                }
                API.sendChatMessageToPlayer(player, "You have changed " + member.CharacterName + "'s rank to " + rank + " (was " + oldrank + ").");
                member.GroupRank = rank;
            }
        }

        [Command("group", Alias = "g", GreedyArg = true)]
        public void group_cmd(Client player, string message)
        {
            SendGroupMessage(player, message);
        }

        [Command("accept")]
        public void accept_cmd(Client player, string option)
        {
            if (option == "groupinvitation")
            {
                Character sender = API.getEntityData(player.handle, "Character");
                Character character = API.getEntityData(player.handle, "GroupInvitation");

                sender.GroupId = character.GroupId ;
                sender.Group = character.Group;
                sender.GroupRank = 1;
                sender.Save();

                API.sendChatMessageToPlayer(player, "You have joined " + sender.Group.GroupName + ".");

                foreach (var c in PlayerManager.Players)
                {
                    if (c.GroupId == sender.GroupId)
                    {
                        API.sendChatMessageToPlayer(c.Client, sender.CharacterName + "has joined the group (INVITATION).");

                    }
                }
            }
        }

        [Command("quitgroup")]
        public void quitgroup_cmd(Client player)
        {
            Character sender = API.getEntityData(player.handle, "Character");

            if(sender.GroupId == 0)
            {
                API.sendChatMessageToPlayer(player, "You are not in any group.");
                return;
            }

            API.sendChatMessageToPlayer(player, "You have left " + sender.Group.GroupName + ".");

            foreach (var c in PlayerManager.Players)
            {
                if (c.GroupId == sender.GroupId)
                {
                    API.sendChatMessageToPlayer(c.Client, sender.CharacterName + "has left the group (QUIT).");

                }
            }

            sender.GroupId = 0;
            sender.Group = null;
            sender.GroupRank = 0;
            sender.Save();

        }
        
        [Command("scenario", GreedyArg = true)]
        public void scenario_cmd(Client player, string scenarioName)
        {
            if(scenarioName == "off")
            {
                API.stopPlayerAnimation(player);
            }
            API.playPlayerScenario(player, scenarioName);
        }

        [Command("invite")]
        public void invite_cmd(Client player, string id)
        {
            var invited = PlayerManager.ParseClient(id);
            Character sender = API.getEntityData(player.handle, "Character");
            Character invitedchar = API.getEntityData(invited.handle, "Character");

            if ( sender.GroupId > 0 && sender.GroupRank > 6)
            {
                API.setEntityData(invited.handle, "GroupInvitation", sender);

                API.sendChatMessageToPlayer(invited, Color.Pm, "You have been invited to " + sender.Group.GroupName + ". Type /accept groupinvitation.");
                API.sendChatMessageToPlayer(player, "You sent a group invitation to " + invitedchar.CharacterName + ".");
            }
        }

        [Command("setrankname", GreedyArg = true)]
        public void setrankname_cmd(Client player, int rankid, string rankname)
        {
            Character character = API.getEntityData(player.handle, "Character");
            Account account = API.getEntityData(player.handle, "Account");

            if ((character.GroupId != 0 && character.GroupRank >= 8) || account.AdminLevel >= 5)
            {
                character.Group.GroupRankNames.RemoveAt(rankid);
                character.Group.GroupRankNames.Insert(rankid, rankname);
                API.sendChatMessageToPlayer(player, "You have set Group ID " + character.Group.Id + " (" + character.Group.GroupName + ")'s Rank " + rankid + " to " + rankname + ".");
                character.Group.Save();
            }
        }

        [Command("setdivisionname", GreedyArg = true)]
        public void setdivisionname_cmd(Client player, int divid, string divname)
        {
            Character character = API.getEntityData(player.handle, "Character");
            Account account = API.getEntityData(player.handle, "Account");

            if ((character.GroupId != 0 && character.GroupRank >= 8) || account.AdminLevel >= 5)
            {
                character.Group.GroupDivisions.RemoveAt(divid);
                character.Group.GroupDivisions.Insert(divid, divname);
                API.sendChatMessageToPlayer(player, "You have set Group ID " + character.Group.Id + " (" + character.Group.GroupName + ")'s Division " + divid + " to " + divname + ".");
                character.Group.Save();
            }
        }

        [Command("set")]
        public void set_cmd(Client player, string id, string option, int amount)
        {
            var receiver = PlayerManager.ParseClient(id);
            Account account = API.getEntityData(player.handle, "Account");
            Character character = API.getEntityData(player.handle, "Character");

            if (account.AdminLevel == 0)
                return;

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            if (option == "group")
            {
                character.GroupId = amount;
                character.Group = GetGroupById(amount);
                character.Save();
                API.sendChatMessageToPlayer(player, "You have set " + PlayerManager.GetName(receiver) + "[" + id + "]" + "'s group to " + amount + ".");
            }
            else if (option == "grouprank")
            {
                character.GroupRank = amount;
                character.Save();
                API.sendChatMessageToPlayer(player, "You have set " + PlayerManager.GetName(receiver) + "[" + id + "]" + "'s group rank to " + amount + ".");
            }
        }

        [Command("creategroup", GreedyArg = true)]
        public void creategroup_cmd(Client player, int type, string name)
        {
            Account account = API.getEntityData(player.handle, "Account");
            if (account.AdminLevel < 4)
                return;

            var group = new Group();

            group.GroupName = name;
            group.GroupType = type;
            group.Insert();

            API.sendChatMessageToPlayer(player, Color.Grey, "You have created group " + group.Id + " ( " + group.GroupName + ", Type: " + group.GroupType + " ). Use /editgroup to edit it.");
        }

        public static Group GetGroupById(int id)
        {
            if (id == 0 || id > Groups.Count)
                return null;

            return (Group)Groups.ToArray().GetValue(id - 1);
        }

        public void SendGroupMessage(Client player, string message)
        {
            Character sender = API.getEntityData(player.handle, "Character");
            foreach (var c in PlayerManager.Players)
            {
                if(c.GroupId == sender.GroupId)
                {
                    API.sendChatMessageToPlayer(c.Client, Color.GroupChat, "[G][" + sender.GroupRank +"] " + sender.Group.GroupRankNames[sender.GroupRank]+ " " +sender.CharacterName + " : " + "~w~" + message);
                }
            }
        }

        public void load_groups()
        {
            Groups = DatabaseManager.GroupTable.Find<Group>(Builders<Group>.Filter.Empty).ToList<Group>();

            foreach (var g in Groups)
            {
                
            }

            DebugManager.DebugMessage("Loaded " + Groups.Count + " groups from the database.");
        }
    }
}
