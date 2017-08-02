using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.database_manager;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using MongoDB.Driver;

namespace mtgvrp.group_manager
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

        [Command("listgroups"), Help(HelpManager.CommandGroups.AdminLevel5, "List all groups in the server.")]
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

        [Command("setgroupmapicon", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Sets the blip icon of a group.", "Group id", "Map icon id. Get this from wiki.", "Map icon text.")]
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

        [Command("respawngroupvehicles"), Help(HelpManager.CommandGroups.AdminLevel5, "Respawns group vehicles of a group.", "Group id.")]
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
                VehicleManager.respawn_vehicle(v);
                API.setVehicleEngineStatus(v.NetHandle, false);
            }
            API.sendChatMessageToPlayer(player,
                vehiclesToRespawn.Count + " vehicles have been respawned for group: " + group.Name);
        }

 

        [Command("listgroupvehicles"), Help(HelpManager.CommandGroups.AdminLevel5, "List all group vehicles in a group.", "Group id")]
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

        [Command("uninvite"), Help(HelpManager.CommandGroups.GroupGeneral, "Cancel your group invite to someone.", "Id or name of player")]
        public void uninvite_cmd(Client player, string nameToFind)
        {
            var character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }

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

        [Command("setrank"), Help(HelpManager.CommandGroups.GroupGeneral, "Set someones rank in the group.", "Player id", "The rank you'd like to give.")]
        public void setrank_cmd(Client player, string id, int rank)
        {
            Character sender = player.GetCharacter();
            var  receiver = PlayerManager.ParseClient(id);
            if (sender.Group == Group.None || sender.GroupRank < 6)
            {
                return;
            }

            if (receiver == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }

            if (rank < 1 || rank > 10)
            {
                API.sendChatMessageToPlayer(player, Color.White, "Valid ranks are between 1 and 10.");
                return;
            }

            Character member = receiver.GetCharacter();
            if (sender.GroupRank >= member.GroupRank && sender.GroupRank > rank)
            {
                var oldRank = member.GroupRank;
                if (oldRank > rank)
                {
                    API.sendChatMessageToPlayer(receiver,
                        "You have been demoted to " + member.Group.RankNames[rank - 1] + " by " + sender.CharacterName + ".");
                }
                else
                {
                    API.sendChatMessageToPlayer(receiver,
                        "You have been promoted to " + member.Group.RankNames[rank - 1] + " by " + sender.CharacterName +
                        ".");
                }
                API.sendChatMessageToPlayer(player,
                    "You have changed " + member.CharacterName + "'s rank to " + (rank) + " (was " + (oldRank) + ").");
                member.GroupRank = rank;
                member.Save();
            }
            else
            {
                API.sendChatMessageToPlayer(player, Color.White, "You cannot set the rank of a higher ranking member or set someone to a rank above yours.");
            }
        }

        [Command("listranks"), Help(HelpManager.CommandGroups.GroupGeneral, "View all ranks in the group.")]
        public void listranks_cmd(Client player)
        {
            Character sender = player.GetCharacter();

            if (sender.Group == Group.None || sender.GroupRank < 6)
            {
                return;
            }

            player.sendChatMessage("=======================================");
            player.sendChatMessage($"Rank list for {sender.Group.Name}");
            player.sendChatMessage("=======================================");

            int i = 1;
            foreach (var rankName in sender.Group.RankNames)
            {
                player.sendChatMessage($"Rank: {i} | Name: {rankName}");
                i++;
            }
        }

        [Command("listdivisions"), Help(HelpManager.CommandGroups.GroupGeneral, "View all divisions in the group.")]
        public void listdivisions_cmd(Client player)
        {
            Character sender = player.GetCharacter();

            if (sender.Group == Group.None || sender.GroupRank < 6)
            {
                return;
            }

            player.sendChatMessage("=======================================");
            player.sendChatMessage($"Division list for {sender.Group.Name}");
            player.sendChatMessage("=======================================");

            int i = 1;
            foreach (var divisionName in sender.Group.Divisions)
            {
                player.sendChatMessage($"Division: {i} | Name: {divisionName}");
                i++;
            }
        }

        [Command("setdivision"), Help(HelpManager.CommandGroups.GroupGeneral, "Set someone in the group into a division.", "Player id", "Division id")]
        public void setdivision_cmd(Client player, string id, int divId)
        {
            Character character = player.GetCharacter();
            var receiver = PlayerManager.ParseClient(id);

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }

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

            Character receiverChar = receiver.GetCharacter();

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
                receiverChar.DivisionRank = 0;
                receiverChar.Save();
                API.sendChatMessageToPlayer(receiver, Color.White,
                    character.CharacterName + " has removed your position in a division.");

                API.sendChatMessageToPlayer(player, Color.White, "You have removed " + receiverChar.CharacterName + " from a division.");
            }
        }

        [Command("setdivisionrank"), Help(HelpManager.CommandGroups.GroupGeneral, "Set someones division rank.", "Player id", "Rank")]
        public void setdivisionrank_cmd(Client player, string id, int rank)
        {
            var receiver = PlayerManager.ParseClient(id);
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }

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

            Character receiverChar = receiver.GetCharacter();

            if (receiverChar.DivisionRank <= character.DivisionRank && character.DivisionRank > rank || character.GroupRank > 6)
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

        [Command("group", Alias = "g", GreedyArg = true), Help(HelpManager.CommandGroups.Chat | HelpManager.CommandGroups.GroupGeneral, "Talk in group OOC chat.", "The message")]
        public void group_cmd(Client player, string message)
        {

            if (GroupCommandPermCheck(API.getEntityData(player.handle, "Character"), 1)){

                Character character = player.GetCharacter();
                SendGroupMessage(player, "[G][" + character.GroupRank + "] " + GetRankName(character) + " #" + character.BadgeNumber + " " + character.CharacterName + " : " + " ~w~" + message);
                LogManager.Log(LogManager.LogTypes.GroupChat, $"[Group {character.Group.Name}][" + character.GroupRank + "] " + GetRankName(character) + " " + character.CharacterName + $"[{player.socialClubName}]" + " : " + message);
            }
        }

        [Command("radio", Alias = "r", GreedyArg = true), Help(HelpManager.CommandGroups.Chat | HelpManager.CommandGroups.GroupGeneral, "Talk in group radio IC chat.", "The message")]
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

                var radioMsg = "~b~[RADIO][" + character.GroupRank + "] " + GetRankName(character) + " #" + character.BadgeNumber + " " +
                               character.CharacterName + " : " + "~w~" + message;

                SendRadioMessage(player, radioMsg);

                LogManager.Log(LogManager.LogTypes.GroupChat, $"[Radio {character.Group.Name}][" + character.GroupRank + "] " + GetRankName(character) + " " + character.CharacterName + $"[{player.socialClubName}]" + " : " + message);
            }
        }

        [Command("toggleradio", GreedyArg = true), Help(HelpManager.CommandGroups.GroupGeneral, "Toggle the IC radio on/off")]
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

        [Command("accept"), Help(HelpManager.CommandGroups.GroupGeneral, "Accepts a group invitation", "Option: groupinvitation")]
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

                character.GroupId = inviteSender.GroupId;
                character.Group = inviteSender.Group;
                character.Group.CommandType = inviteSender.Group.CommandType;
                character.GroupRank = 1;
                character.Save();

                API.sendChatMessageToPlayer(player, "You have joined " + inviteSender.Group.Name + ".");

                SendGroupMessage(player,
                    character.CharacterName + " has joined the group. (Invited by: " + inviteSender.CharacterName +
                    ")");
                LogManager.Log(LogManager.LogTypes.GroupInvites, $"{character.CharacterName}[{player.GetAccount().AccountName}] has joined the group {character.Group.Name}. (Invited by: {inviteSender.CharacterName}[{inviteSender.Client.GetAccount().AccountName}])");
            }
        }
        
        [Command("listgroup"), Help(HelpManager.CommandGroups.GroupGeneral, "Lists current players online in the group.")]
        public void listgroup_cmd(Client player)
        {
            Character sender = player.GetCharacter();

            if (sender.Group == Group.None) { player.sendChatMessage("You are not in a group."); return; }
            player.sendChatMessage("===================================");
            player.sendChatMessage("Online Group Members:");
            player.sendChatMessage("===================================");

            foreach (var p in PlayerManager.Players)
            {
                if (p.Group == sender.Group)
                {
                    API.sendChatMessageToPlayer(player, p.CharacterName + " | Rank: " + p.GroupRank + " | Division: " + p.Division + " | Division Rank: " + p.DivisionRank);
                }
            }

            player.sendChatMessage("===================================");
        }

        [Command("quitgroup"), Help(HelpManager.CommandGroups.GroupGeneral, "Quits your current group.")]
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
            API.sendChatMessageToPlayer(player, "You have left " + sender.Group?.Name + ".");
        }
        
        [Command("invite"), Help(HelpManager.CommandGroups.GroupGeneral, "Invite someone to your group.", "The player id")]
        public void invite_cmd(Client player, string id)
        {
            Character sender = player.GetCharacter();

            if (sender.Group == Group.None || sender.GroupRank < 6)
            {
                return;
            }

            var invited = PlayerManager.ParseClient(id);

            if (invited == null)
            {
                API.sendChatMessageToPlayer(player, Color.White, "That player is not connected.");
                return;
            }


            Character invitedchar = invited.GetCharacter();
            API.setEntityData(invited.handle, "GroupInvitation", sender);

            API.sendChatMessageToPlayer(invited, Color.Pm, "You have been invited to " + sender.Group.Name + ". Type /accept groupinvitation.");
            API.sendChatMessageToPlayer(player, "You sent a group invitation to " + invitedchar.CharacterName + ".");
            LogManager.Log(LogManager.LogTypes.GroupInvites, $"{sender.CharacterName}[{player.GetAccount().AccountName}] has invited {invitedchar.CharacterName}[{invitedchar.Client.GetAccount().AccountName}] to their group. ({sender.Group.Name})");
        }

        [Command("setrankname", GreedyArg = true), Help(HelpManager.CommandGroups.GroupGeneral, "Sets the name of a rank in the group.", "Rank id", "The name of the rank")]
        public void setrankname_cmd(Client player, int rankId, string rankname)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }
               
            
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

        [Command("setdivisionname", GreedyArg = true), Help(HelpManager.CommandGroups.GroupGeneral, "Changes a division name.", "Div Id", "The name.")]
        public void setdivisionname_cmd(Client player, int divId, string divname)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }

          
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

        [Command("setdivisionrankname", GreedyArg = true), Help(HelpManager.CommandGroups.GroupGeneral, "Set division rank name", "Dev id", "Rank id", "Rank name")]
        public void setdivisonrankname_cmd(Client player, int divId, int rankId, string rankName)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 6)
            {
                return;
            }


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

        [Command("creategroup", GreedyArg = true), Help(HelpManager.CommandGroups.AdminLevel5, "Creates a new group", "Group Type", "Group command type [LSPD/LSNN, etc..]", "Group name.")]
        public void creategroup_cmd(Client player, int type, int commandtype, string name)
        {

            Account account = player.GetAccount();
            if (account.AdminLevel < 4)
                return;

            if (type == 1 && commandtype == 0)
            {
                player.sendChatMessage("Factions of type 1 must have a command type greater than 0");
                return;
            }

            var group = new Group();

            group.Name = name;
            group.Type = type;
            if (type != 1) { group.CommandType = 0; }
            else { group.CommandType = commandtype; }
            group.Insert();

            API.sendChatMessageToPlayer(player, Color.Grey, "You have created group " + group.Id + " ( " + group.Name + ", Type: " + group.Type + " ). Use /editgroup to edit it.");
            Groups = DatabaseManager.GroupTable.Find(Builders<Group>.Filter.Empty).ToList();
        }

        [Command("setpaycheckbonus", GreedyArg = true), Help(HelpManager.CommandGroups.GroupGeneral, "Sets the paycheck bonus of group memebers", "The bonus")]
        public void setpaycheckbonus_cmd(Client player, string amount)
        {

            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 8)
            {
                return;
            }

            character.Group.FactionPaycheckBonus = int.Parse(amount);
            character.Group.Save();
            API.sendChatMessageToPlayer(player, "You have set your faction's paycheck bonus to $" + amount + ".");
        }

        [Command("groupbalance"), Help(HelpManager.CommandGroups.GroupGeneral, "Checks the balance of the group.")]
        public void groupbalance_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.GroupRank < 8)
            {
                return;
            }

            player.sendChatMessage($"Your group is receiving {character.Group.FundingPercentage}% of government funds " +
                $"(${Properties.Settings.Default.governmentbalance * character.Group.FundingPercentage / 100}).");
        }

        public static Group GetGroupById(int id)
        {
            if (id == 0 || id > Groups.Count)
                return Group.None;

            return (Group)Groups.ToArray().GetValue(id - 1);
        }

        public static void SendGroupMessage(Client player, string message, string color = Color.GroupChat)
        {
            Character sender = player.GetCharacter();
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
            Character sender = player.GetCharacter();
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
