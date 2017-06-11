using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;
using MongoDB.Driver;
using RoleplayServer.core;
using RoleplayServer.database_manager;
using RoleplayServer.player_manager;
using RoleplayServer.vehicle_manager;

namespace RoleplayServer.group_manager
{
    public class GroupManager : Script
    {

        public static List<Group> Groups = new List<Group>();

        public GroupManager()
        {
            DebugManager.DebugMessage("[GroupM] Initalizing group manager...");

            load_groups(); 

            DebugManager.DebugMessage("[GroupM] Group Manager initalized!");

        }

        [Command("listgroups")]
        public void listgroups_cmd(Client player)
        {
            Account acc = player.GetAccount();

            if (acc.AdminLevel < 4)
                return;

            Groups = DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList();
            API.sendChatMessageToPlayer(player, "-----------------------------------------------------------------------------------");
            foreach (var g in Groups)
            {
                switch(g.Type)
                {
                    case 1: API.sendChatMessageToPlayer(player, "Group Name: " + g.Name + " | Type: Faction | Command Type: " + g.CommandType + "."); break;
                    case 2: API.sendChatMessageToPlayer(player, "Group Name: " + g.Name + " | Type: Gang | Command Type: " + g.CommandType + "."); break;
                }
            }
            API.sendChatMessageToPlayer(player, "---------------------------------------------------------------------------------- ");
        }

        [Command("setgroupmapicon", GreedyArg = true)]
        public void setgroupmapicon_cmd(Client player, int groupId, int mapIconType, string mapIconText = "")
        {
            var account = player.GetAccount();
            if (account.AdminLevel < 5)
            {
                return;
            }

            var group = GetGroupById(groupId);
            if (group == Group.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ No group found with ID " + groupId);
                return;
            }

            group.MapIconId = mapIconType;
            group.MapIconPos = player.position;
            group.MapIconText = mapIconText;
            group.Save();
            group.UpdateMapIcon();
            API.sendChatMessageToPlayer(player, Color.Grey,
                "You have updated " + group.Name + "'s map icon to sprite " + group.MapIconId + " and text: " +
                group.MapIconText);
        }

        [Command("respawngroupvehicles")]
        public void respawngroupvehicles_cmd(Client player, int groupId)
        {
            var account = player.GetAccount();

            if (account.AdminLevel < 4)
            {
                return;
            }

            var group = GetGroupById(groupId);
            if (group == Group.None)
            {
                API.sendChatMessageToPlayer(player, Color.White, "~r~[ERROR]~w~ No group found with ID " + groupId);
                return;
            }

            var vehiclesToRespawn = VehicleManager.Vehicles.FindAll(v => v.Group == group);
            foreach(var v in vehiclesToRespawn)
            {
                v.Respawn();
            }
            API.sendChatMessageToPlayer(player,
                vehiclesToRespawn + " vehicles have been respawned for group: " + group.Name);
        }

 

        [Command("listgroupvehicles")]
        public void listgroupvehicles_cmd(Client player, string id)
        {
            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            if (account.AdminLevel < 4)
            {
                return;
            }

            API.sendChatMessageToPlayer(player, "======GROUP VEHICLES======");
            var filter = Builders<vehicle_manager.Vehicle>.Filter.Eq("GroupId", id);
            var groupVehicles = DatabaseManager.VehicleTable.Find(filter).ToList();

            int j = 0;
            foreach (var v in groupVehicles)
            {
                    API.sendChatMessageToPlayer(player, "ID: " + v.Id + " Vehicle: " + v.VehModel);
                    j++;
            }
            API.sendChatMessageToPlayer(player, "There are " + j + " vehicles in this group.");

        }

        [Command("uninvite")]
        public void uninvite_cmd(Client player, string nameToFind)
        {
            var character = player.GetCharacter();

            GroupCommandPermCheck(character, 6);

            var clientToUninvite = PlayerManager.ParseClient(nameToFind);

            if (clientToUninvite == null)
            {
                var filter = Builders<Character>.Filter.Eq("CharacterName", nameToFind);
                var foundCharacters = DatabaseManager.CharacterTable.Find(filter).ToList();

                if (foundCharacters.Count == 0)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "~r~[ERROR]~w~ No player online or offline found with that name.");
                    return;
                }

                foreach (var c in foundCharacters)
                {
                    if (c.GroupId != character.GroupId)
                    {
                        API.sendChatMessageToPlayer(player, Color.White,
                            "~r~[ERROR]~w~ That player is not in the same group as you.");
                        return;
                    }

                    if (c.GroupRank >= character.GroupRank)
                    {
                        API.sendChatMessageToPlayer(player, Color.White,
                            "~r~[ERROR]~w~ You cannot uninvite the same or higher rank.");
                        return;
                    }

                    c.GroupId = 0;
                    c.GroupRank = 0;
                    c.Save();

                    API.sendChatMessageToPlayer(player, Color.White,
                        "You have uninvited " + c.CharacterName + " from your group: " + character.Group.Name);

                    SendGroupMessage(player,
                        c.CharacterName + " has left the group. (Remote-uninvited by " + character.CharacterName + ")");
                }
            }
            else
            {
                var charToUninvite = clientToUninvite.GetCharacter();

                if (charToUninvite.Group != character.Group)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                        "~r~[ERROR]~w~ That player is not in the same group as you.");
                    return;
                }

                if (charToUninvite.GroupRank >= character.GroupRank)
                {
                    API.sendChatMessageToPlayer(player, Color.White,
                       "~r~[ERROR]~w~ You cannot uninvite the same or higher rank.");
                    return;
                }

                charToUninvite.Group = Group.None;
                charToUninvite.GroupId = 0;
                charToUninvite.GroupRank = 0;
                charToUninvite.Save();

                API.sendChatMessageToPlayer(player, Color.White,
                    "You have uninvited " + charToUninvite.CharacterName + " from your group: " + character.Group.Name);

                SendGroupMessage(player,
                    charToUninvite.CharacterName + " has left the group. (Uninvited by " + character.CharacterName + ")");
            }
        }

        [Command("setrank")]
        public void setrank_cmd(Client player, string id, int rank)
        {
            var  receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            Character sender = player.GetCharacter();
            GroupCommandPermCheck(sender, 5);

            Character member = API.getEntityData(receiver.handle, "Character");
            if (sender.GroupRank >= member.GroupRank && sender.GroupRank > rank)
            {
                var oldRank = member.GroupRank;
                if (oldRank > rank)
                {
                    API.sendChatMessageToPlayer(receiver,
                        "You have been demoted to " + member.Group.RankNames[rank] + " by " + sender.CharacterName + ".");
                }
                else
                {
                    API.sendChatMessageToPlayer(receiver,
                        "You have been promoted to " + member.Group.RankNames[rank] + " by " + sender.CharacterName +
                        ".");
                }
                API.sendChatMessageToPlayer(player,
                    "You have changed " + member.CharacterName + "'s rank to " + rank + " (was " + oldRank + ").");
                member.GroupRank = rank;
                member.Save();
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "You cannot set the rank of a higher ranking member or set someone to a rank above yours.");
            }
        }

        [Command("setdivision")]
        public void setdivision_cmd(Client player, string id, int divId)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            if (divId < 0 || divId > 5)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid division IDs are between 0 and 5.");
                return;
            }

            Character character = player.GetCharacter();
            GroupCommandPermCheck(character, 7);

            Character receiverChar = API.getEntityData(receiver.handle, "Character");

            receiverChar.Division = divId;
            receiverChar.DivisionRank = 1;
            receiverChar.Save();

            if (divId != 0)
            {
                API.sendChatMessageToPlayer(receiver, Color.White,
                    character.CharacterName + " has added you to the " + character.Group.Divisions[divId - 1] +
                    " division.");

                API.sendChatMessageToPlayer(player, Color.White, "You have added " + receiverChar.CharacterName + " to the " + character.Group.Divisions[divId - 1] + " division.");
            }
            else
            {
                API.sendChatMessageToPlayer(receiver, Color.White,
                    character.CharacterName + " has removed your position in a division.");

                API.sendChatMessageToPlayer(player, Color.White, "You have removed " + receiverChar.CharacterName + " from a division.");
            }
        }

        [Command("setdivisionrank")]
        public void setdivisionrank_cmd(Client player, string id, int rank)
        {
            var receiver = PlayerManager.ParseClient(id);
            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            if (rank < 1 || rank > 5)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid division ranks are between 1 and 5.");
                return;
            }

            Character character = player.GetCharacter();
            GroupCommandPermCheck(character, 7, true, 4);

            Character receiverChar = API.getEntityData(receiver.handle, "Character");

            if (receiverChar.DivisionRank <= character.DivisionRank && character.DivisionRank > rank)
            {

                if (rank > receiverChar.DivisionRank)
                {
                    API.sendChatMessageToPlayer(receiver, Color.White,
                        "You have been promoted in your division to rank " + rank + " by " + character.CharacterName);

                    API.sendChatMessageToPlayer(player, Color.White,
                        "You have promoted " + receiverChar.CharacterName + " in their division to rank " + rank);
                }
                else if (rank <= receiverChar.DivisionRank)
                {
                    API.sendChatMessageToPlayer(receiver, Color.White,
                        "You have been demoted in your division to rank " + rank + " by " + character.CharacterName);

                    API.sendChatMessageToPlayer(player, Color.White,
                        "You have demoted " + receiverChar.CharacterName + " in their division to rank " + rank);
                }

                receiverChar.DivisionRank = rank;
                receiverChar.Save();
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "You cannot set the rank of a higher ranking member or set someone to a rank above yours.");
            }
        }

        [Command("group", Alias = "g", GreedyArg = true)]
        public void group_cmd(Client player, string message)
        {

            if (GroupCommandPermCheck(API.getEntityData(player.handle, "Character"), 1)){

                Character character = player.GetCharacter();
                SendGroupMessage(player, "[G][" + character.GroupRank + "] " + GetRankName(character) + " " + character.CharacterName + " : " + "~w~" + message);
            }
        }

        [Command("radio", Alias = "r", GreedyArg = true)]
        public void radio_cmd(Client player, string message)
        {

            if (GroupCommandPermCheck(API.getEntityData(player.handle, "Character"), 1))
            {

                Character character = player.GetCharacter();

                if (character.RadioToggle == false)
                {
                    API.sendChatMessageToPlayer(player, "~r~Your radio is off.");
                    return;
                }

                var radioMsg = "~b~[RADIO][" + character.GroupRank + "] " + GetRankName(character) + " " +
                               character.CharacterName + " : " + "~w~" + message;

                ChatManager.NearbyMessage(player, 15, radioMsg, Color.Grey);
                SendRadioMessage(player, radioMsg);
            }
        }

        [Command("toggleradio", GreedyArg = true)]
        public void toggleradio_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.RadioToggle == false)
            {
                character.RadioToggle = true;
                API.sendChatMessageToPlayer(player, "~p~You turn your radio on.");
            }
            else
            {
                character.RadioToggle = false;
                API.sendChatMessageToPlayer(player, "~p~You turn your radio off.");
            }
        }

        [Command("accept")]
        public void accept_cmd(Client player, string option)
        {
            if (option == "groupinvitation")
            {
                Character character = player.GetCharacter();
                Character inviteSender = API.getEntityData(player.handle, "GroupInvitation");

                if (inviteSender == null)
                {
                    API.sendChatMessageToPlayer(player, Color.White, "You do not have an active group invitiation.");
                    return;
                }

                character.GroupId = inviteSender.GroupId ;
                character.Group = inviteSender.Group;
                character.Group.CommandType = inviteSender.Group.CommandType;
                character.GroupRank = 1;
                character.Save();

                API.sendChatMessageToPlayer(player, "You have joined " + inviteSender.Group.Name + ".");

                SendGroupMessage(player,
                    character.CharacterName + " has joined the group. (Invited by: " + inviteSender.CharacterName + ")");
            }
        }

        [Command("quitgroup")]
        public void quitgroup_cmd(Client player)
        {
            Character sender = player.GetCharacter();

            if(sender.GroupId == 0)
            {
                API.sendChatMessageToPlayer(player, "You are not in any group.");
                return;
            }

            SendGroupMessage(player, sender.CharacterName + " has left the group. (Quit)");

            sender.GroupId = 0;
            sender.Group = null;
            sender.GroupRank = 0;
            sender.Save();
            API.sendChatMessageToPlayer(player, "You have left " + sender.Group.Name + ".");
        }
        
        [Command("invite")]
        public void invite_cmd(Client player, string id)
        {
            var invited = PlayerManager.ParseClient(id);

            if (invited == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            Character sender = player.GetCharacter();
            GroupCommandPermCheck(sender, 6);

            Character invitedchar = API.getEntityData(invited.handle, "Character");
            API.setEntityData(invited.handle, "GroupInvitation", sender);

            API.sendChatMessageToPlayer(invited, Color.Pm, "You have been invited to " + sender.Group.Name + ". Type /accept groupinvitation.");
            API.sendChatMessageToPlayer(player, "You sent a group invitation to " + invitedchar.CharacterName + ".");
            
        }

        [Command("setrankname", GreedyArg = true)]
        public void setrankname_cmd(Client player, int rankId, string rankname)
        {
            Character character = player.GetCharacter();
            GroupCommandPermCheck(character, 5);
               
            
            if (rankId < 1 || rankId > 10)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid ranks are between 1 and 10.");
                return; 
            }

            character.Group.RankNames.RemoveAt(rankId - 1);
            character.Group.RankNames.Insert(rankId - 1, rankname);
            API.sendChatMessageToPlayer(player, "You have set Group ID " + character.Group.Id + " (" + character.Group.Name + ")'s Rank " + rankId + " to " + rankname + ".");
            character.Group.Save();
        }

        [Command("setdivisionname", GreedyArg = true)]
        public void setdivisionname_cmd(Client player, int divId, string divname)
        {
            Character character = player.GetCharacter();
            GroupCommandPermCheck(character, 5, true, 4);

          
            if (divId < 1 || divId > character.Group.Divisions.Count)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid division IDs are between 1 and 5.");
                return; 
            }

            character.Group.Divisions.RemoveAt(divId - 1);
            character.Group.Divisions.Insert(divId - 1, divname);
            API.sendChatMessageToPlayer(player, "You have set Group ID " + character.Group.Id + " (" + character.Group.Name + ")'s Division " + divId + " to " + divname + ".");
            character.Group.Save();
        }

        [Command("setdivisionrankname", GreedyArg = true)]
        public void setdivisonrankname_cmd(Client player, int divId, int rankId, string rankName)
        {
            Character character = player.GetCharacter();
            GroupCommandPermCheck(character, 5, true, 4);

            if (divId < 1 || divId > 5)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid divisions are between 1 and 5.");
                return;
            }

            if (rankId < 1 || rankId > 5)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid division ranks are between 1 and 5.");
                return;
            }

            divId--;
            rankId--;


            character.Group.DivisionRanks[divId].RemoveAt(rankId);
            character.Group.DivisionRanks[divId].Insert(rankId, rankName);
            character.Group.Save();

            API.sendChatMessageToPlayer(player, Color.White, "You have changed rank " + (rankId + 1) + "'s name in the " + character.Group.Divisions[divId] + " division to " + rankName);
        }

        [Command("creategroup", GreedyArg = true)]
        public void creategroup_cmd(Client player, int type, string name, string commandtype = "0")
        {

            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            if (type == 1 && int.Parse(commandtype) == 0)
            {
                player.sendChatMessage("Factions of type 1 must have a command type greater than 0");
                return;
            }

            var group = new Group();

            group.Name = name;
            group.Type = type;
            if (type != 1) { group.CommandType = 0; }
            else { group.CommandType = int.Parse(commandtype); }
            group.Insert();

            API.sendChatMessageToPlayer(player, Color.Grey, "You have created group " + group.Id + " ( " + group.Name + ", Type: " + group.Type + " ). Use /editgroup to edit it.");
            Groups = DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList();
        }

        [Command("setpaycheckbonus", GreedyArg = true)]
        public void setpaycheckbonus_cmd(Client player, string amount)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if(character.Group.CommandType == 0) { return; }

            GroupCommandPermCheck(character, 5);
            character.Group.FactionPaycheckBonus = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "You have set your faction's paycheck bonus to $" + amount + ".");
        }

        public static Group GetGroupById(int id)
        {
            if (id == 0 || id > Groups.Count)
                return Group.None;

            return (Group)Groups.ToArray().GetValue(id - 1);
        }

        public static void SendGroupMessage(Client player, string message, string color = Color.GroupChat)
        {
            Character sender = API.shared.getEntityData(player.handle, "Character");
            foreach (var c in PlayerManager.Players)
            {
                if(c.GroupId == sender.GroupId)
                {
                    API.shared.sendChatMessageToPlayer(c.Client, color, message);
                }
            }
        }

        public static void SendRadioMessage(Client player, string message, string color = Color.GroupChat)
        {
            Character sender = API.shared.getEntityData(player.handle, "Character");
            foreach (var c in PlayerManager.Players)
            {
                if (c.GroupId == sender.GroupId && c.RadioToggle == true)
                {
                    API.shared.sendChatMessageToPlayer(c.Client, color, message);
                }
            }
        }

        public string GetRankName(Character c, bool ignoreDivision = false)
        {
            if (ignoreDivision == false)
            {
                return c.DivisionRank != 0 ? GetDivisonRankName(c) : c.Group.RankNames[c.GroupRank - 1];
            }
            return c.GroupRank == 0 ? "None" : c.Group.RankNames[c.GroupRank - 1];
        }

        public string GetDivisonRankName(Character c)
        {
            return c.DivisionRank == 0 ? "None" : c.Group.DivisionRanks[c.Division - 1][c.DivisionRank - 1];
        }

        public static bool GroupCommandPermCheck(Character c, int rank, bool isDivisionCmd = false, int divisionRank = 1)
        {
            if(isDivisionCmd == false)
                if (c.Group != Group.None) return c.GroupRank >= rank;

            if (c.Group != Group.None) return c.DivisionRank >= divisionRank || c.GroupRank >= rank;

            API.shared.sendChatMessageToPlayer(c.Client, "You do not have permission to perform this command.");
            return false;
        }

        public void load_groups()
        {
            Groups = DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList();

            foreach (var g in Groups)
            {
                g.register_markerzones();
                g.UpdateMapIcon();
            }

            DebugManager.DebugMessage("Loaded " + Groups.Count + " groups from the database.");
        }
    }
}
