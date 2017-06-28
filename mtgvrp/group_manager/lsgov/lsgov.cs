using GTANetworkServer;
using mtgvrp.player_manager;
using mtgvrp.group_manager;
using mtgvrp.Properties;


namespace RoleplayServer.group_manager.lsgov
{
    class lsgov : Script
    {
        public lsgov()
        {

        }

        //SET VIP BONUS PERCENTAGE (ONLY FOR ADMINS)
        [Command("setvipbonus")]
        public void setvipbonus_cmd(Client player, string viplevel, string percentage)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");
            Character character = API.shared.getEntityData(player, "Character");

            if (account.AdminLevel < 6) { return; }

            switch (viplevel)
            {
                case "1":
                    Settings.Default.vipbonuslevelone = int.Parse(percentage);
                    break;

                case "2":
                    Settings.Default.vipbonusleveltwo = int.Parse(percentage);
                    break;

                case "3":
                    Settings.Default.vipbonuslevelthree = int.Parse(percentage);
                    break;
            }
            player.sendChatMessage("You have set VIP level " + viplevel + "'s paycheck bonus to " + percentage + "%.");
        }

        //SET TAXATION FOR PAYCHECKS AS MAYOR/OFFICIAL
        [Command("settax")]
        public void settax_cmd(Client player, string percentage)
        {

            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.GroupRank < 7) { return; }
            Settings.Default.taxationamount = int.Parse(percentage);
        }

        //SET BASE PAYCHECK AS MAYOR/OFFICIAL
        [Command("setbasepaycheck", GreedyArg = true)]
        public void setbasepaycheck_cmd(Client player, string amount)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.GroupRank < 7) { return; }
            Settings.Default.basepaycheck = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "Base paycheck set to $" + amount + ".");
        }

        //GOVERNMENT ANNOUNCEMENT AS MAYOR OR HIGH RANKING LSPD
        [Command("gov", GreedyArg = true)]
        public void gov_cmd(Client player, string text)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            foreach (var receiver in PlayerManager.Players)
            {
                API.sendChatMessageToPlayer(receiver.Client, "[Government] " + character.CharacterName + " says: " + text);
            }
        }

        [Command("managebudget")]
        public void budget_cmd(Client player)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            player.sendChatMessage("======================================");
            player.sendChatMessage("Los Santos Government - Budget Manager");
            player.sendChatMessage("======================================");
            player.sendChatMessage($"~r~Government Balance: ~w~${character.Group.GroupBalance}");
            player.sendChatMessage($"~y~Organization Funding:");
            //All faction budget information
            int i = 0;
            foreach (var group in GroupManager.Groups)
            {
                if (group == character.Group) { continue; }
                player.sendChatMessage($"~p~Group ID: ~w~{i} | ~b~{group.Name}~w~ | ~r~Funding:~w~ {group.FundingPercentage}% | ~g~Balance: ${group.GroupBalance}");
                i++;
            }
        }

        [Command("setfunding")]
        public void setfunding_cmd(Client player, string groupid, string percentage)
        {
            Character character = API.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            if(int.Parse(groupid) > GroupManager.Groups.Count) { player.sendChatMessage("Invalid group ID."); return; }
            Group group = GroupManager.Groups[int.Parse(groupid)];
            //finish
        }
        //DEPLOY A PODIUM AS MAYOR OR HIGH RANKING LSPD
        [Command("deploypodium")]
        public void deploypodium_cmd(Client player)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            //DEPLOY A PODIUM WHEN MAPPING IS READY
        }

        [Command("pickuppodium")]
        public void pickuppodium_cmd(Client player)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            //PICKUP PODIUM WHEN MAPPING IS READY
        }

    }
}