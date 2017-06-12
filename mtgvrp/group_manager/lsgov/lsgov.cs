using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using RoleplayServer.core;
using RoleplayServer.door_manager;
using RoleplayServer.player_manager;
using RoleplayServer.vehicle_manager;
using RoleplayServer.inventory;
using System;
using System.Timers;

namespace RoleplayServer.group_manager.lsgov
{
    class lsgov : Script
    {
        public lsgov()
        {
            API.onResourceStart += StartLsgov;
        }

        public void StartLsgov()
        {

        }

        public static int basepaycheck = 500;
        public static int taxationAmount = 4;
        public static int VIPBonusLevelOne = 10;
        public static int VIPBonusLevelTwo = 20;
        public static int VIPBonusLevelThree = 30;

        [Command("setvipbonus")]
        public void setvipbonus_cmd(Client player, string viplevel, string percentage)
        {
            Account account = API.shared.getEntityData(player.handle, "Account");

            if (account.AdminLevel < 6) { return; }

            switch (viplevel)
            {
                case "1":
                    VIPBonusLevelOne = int.Parse(percentage);
                    break;

                case "2":
                    VIPBonusLevelTwo = int.Parse(percentage);
                    break;

                case "3":
                    VIPBonusLevelThree = int.Parse(percentage);
                    break;
            }
            player.sendChatMessage("You have set VIP level " + viplevel + "'s paycheck bonus to " + percentage + "%.");
        }

        [Command("settax")]
        public void settax_cmd(Client player, string percentage)
        {

            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }
            taxationAmount = int.Parse(percentage);
        }

        [Command("setbasepaycheck", GreedyArg = true)]
        public void setbasepaycheck_cmd(Client player, string amount)
        {
            Character character = API.shared.getEntityData(player, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLSGov || character.GroupRank < 7) { return; }
            basepaycheck = int.Parse(amount);
            API.sendChatMessageToPlayer(player, "Base paycheck set to $" + amount + ".");
        }

    }
}
