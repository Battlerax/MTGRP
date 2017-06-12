using GTANetworkServer;
using RoleplayServer.player_manager;


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
                    character.Group.VIPBonusLevelOne = int.Parse(percentage);
                    break;

                case "2":
                    character.Group.VIPBonusLevelTwo = int.Parse(percentage);
                    break;

                case "3":
                    character.Group.VIPBonusLevelThree = int.Parse(percentage);
                    break;
            }
            player.sendChatMessage("You have set VIP level " + viplevel + "'s paycheck bonus to " + percentage + "%.");
        }

        //SET TAXATION FOR PAYCHECKS AS MAYOR/OFFICIAL
        [Command("settax")]
        public void settax_cmd(Client player, string percentage)
        {

            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }
            character.Group.taxationAmount = int.Parse(percentage);
        }

        //SET BASE PAYCHECK AS MAYOR/OFFICIAL
        [Command("setbasepaycheck", GreedyArg = true)]
        public void setbasepaycheck_cmd(Client player, string amount)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }
            character.Group.basepaycheck = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "Base paycheck set to $" + amount + ".");
        }

        //GOVERNMENT ANNOUNCEMENT AS MAYOR OR HIGH RANKING LSPD
        [Command("gov", GreedyArg = true)]
        public void gov_cmd(Client player, string text)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }

            foreach(var receiver in PlayerManager.Players)
            {
                API.sendChatMessageToPlayer(receiver.Client, "[Government] " + character.CharacterName + " says: " + text);
            }
        }

    }
}
