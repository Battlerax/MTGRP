using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using mtgvrp.core;
using mtgvrp.core.Help;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.Properties;

namespace mtgvrp.group_manager.lsgov
{
    class lsgov : Script
    {
        public lsgov()
        {

        }

        //SET VIP BONUS PERCENTAGE (ONLY FOR ADMINS)
        [Command("setvipbonus"), Help(HelpManager.CommandGroups.Gov, "Set the VIP bonus for paychecks.", new[] { "The VIP level being changed.", "VIP bonus percentage"})]
        public void setvipbonus_cmd(Client player, string viplevel, string percentage)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

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
        [Command("settax"), Help(HelpManager.CommandGroups.Gov, "Set the tax percentage for paychecks.", new[] { "Percentage being deducted from paychecks." })]
        public void settax_cmd(Client player, string percentage)
        {

            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.GroupRank < 7) { return; }
            Settings.Default.taxationamount = int.Parse(percentage);
        }

        //SET BASE PAYCHECK AS MAYOR/OFFICIAL
        [Command("setbasepaycheck", GreedyArg = true), Help(HelpManager.CommandGroups.Gov, "Set the base paycheck.", new[] { "Base paycheck amount." })]
        public void setbasepaycheck_cmd(Client player, string amount)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.GroupRank < 7) { return; }
            Settings.Default.basepaycheck = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "Base paycheck set to $" + amount + ".");
        }

        //GOVERNMENT ANNOUNCEMENT AS MAYOR OR HIGH RANKING LSPD
        [Command("gov", GreedyArg = true), Help(HelpManager.CommandGroups.Gov, "Speak publically to everyone as the government.", new[] { "Message to be sent" })]
        public void gov_cmd(Client player, string text)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.Group.CommandType != Group.CommandTypeLspd || character.GroupRank < 7) { return; }

            foreach (var receiver in PlayerManager.Players)
            {
                API.sendChatMessageToPlayer(receiver.Client, "[Government] " + character.CharacterName + " says: " + text);
            }
        }

        [Command("managebudget"), Help(HelpManager.CommandGroups.Gov, "Manage the government budget (factions, stores, etc.)", null)]
        public void managebudget_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            player.sendChatMessage("======================================");
            player.sendChatMessage("Los Santos Government - Budget Manager");
            player.sendChatMessage("======================================");
            player.sendChatMessage($"~r~Government Balance: ~w~${Settings.Default.governmentbalance}");
            player.sendChatMessage($"~y~Organization Funding:");
            //All faction budget information
            int i = 0;
            foreach (var group in GroupManager.Groups)
            {
                player.sendChatMessage($"~p~Group ID: ~w~{i} | ~b~{group.Name}~w~ | ~r~Funding:~w~ {group.FundingPercentage}% | " +
                    $"~g~Balance: ${Settings.Default.governmentbalance * group.FundingPercentage/100}");
                i++;
            }
        }

        [Command("setfunding"), Help(HelpManager.CommandGroups.Gov, "Set the funding for a specific group.", new[] { "Target group ID", "Percentage of funds being given." })]
        public void setfunding_cmd(Client player, string groupid, string percentage)
        {
            Character character = player.GetCharacter();
            Account account = player.GetAccount();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            if (!(int.Parse(groupid) < GroupManager.Groups.Count))
            {
                player.sendChatMessage("Invalid group ID.");
                return;
            }

            Group group = GroupManager.Groups[int.Parse(groupid)];

            if (int.Parse(percentage) == -1)
            {
                if (account.AdminLevel < 6) { player.sendChatMessage("You don't have permission to do that."); return; }

                player.sendChatMessage($"You have set ~b~{group.Name}~w~'s funding to ~r~infinite~w~.");
                group.FundingPercentage = int.Parse(percentage);
            }

            group.FundingPercentage = int.Parse(percentage);
            group.Save();
            player.sendChatMessage($"You have set ~b~{group.Name}~w~'s funding to ~r~{percentage}%~w~.");
        }

        [Command("setgovbalance"), Help(HelpManager.CommandGroups.Gov, "Set the government balance (Admin only)", new[] { "The amount being set." })]
        public void setgovbalance_cmd(Client player, string amount)
        {
            Account account = player.GetAccount();

            if (account.AdminLevel < 6) { return; }

            Settings.Default.governmentbalance = int.Parse(amount);
            player.sendChatMessage($"You have set the government balance to ${amount}.");
        }

        //DEPLOY A PODIUM AS MAYOR OR HIGH RANKING LSPD
        [Command("deploypodium"), Help(HelpManager.CommandGroups.Gov, "Deploy a podium outside the city hall.", null)]
        public void deploypodium_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.Group.CommandType != Group.CommandTypeLspd || character.GroupRank < 7) { return; }

            //DEPLOY A PODIUM WHEN MAPPING IS READY
        }

        [Command("pickuppodium"), Help(HelpManager.CommandGroups.Gov, "Remove the podium from outside the city hall.", null)]
        public void pickuppodium_cmd(Client player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov && character.Group.CommandType != Group.CommandTypeLspd || character.GroupRank < 7) { return; }

            //PICKUP PODIUM WHEN MAPPING IS READY
        }


        [Command("showid"), Help(HelpManager.CommandGroups.Gov, "Show your ID to a player.", new [] {"Target player ID or name." })]
        public void ShowId(Client player, string target)
        {
            var targetPlayer = PlayerManager.ParseClient(target);
            if (targetPlayer == null)
            {
                API.sendChatMessageToPlayer(player, "That player is not online.");
                return;
            }

            var c = player.GetCharacter();

            if (InventoryManager.DoesInventoryHaveItem<IdentificationItem>(c).Length == 0)
            {
                API.sendChatMessageToPlayer(player, "You don't have an identification.");
                return;
            }

            if (targetPlayer.position.DistanceTo(player.position) > 3.0)
            {
                API.sendChatMessageToPlayer(player, "The player must be near you.");
                return;
            }

            API.sendChatMessageToPlayer(targetPlayer, " [************** Identification **************]");
            API.sendChatMessageToPlayer(targetPlayer, $"* Name: ~h~{c.rp_name()}~h~ | Age: ~h~{c.Age}~h~");
            API.sendChatMessageToPlayer(targetPlayer, $"* DOB: ~h~{c.Birthday}~h~ | Birth Place: ~h~{c.Birthplace}~h~");
            API.sendChatMessageToPlayer(targetPlayer, " [********************************************]");

            ChatManager.RoleplayMessage(player, "shows his id to " + targetPlayer.GetCharacter().rp_name(), ChatManager.RoleplayMe);
        }
    }
}