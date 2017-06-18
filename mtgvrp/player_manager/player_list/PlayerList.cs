using System;
using System.Collections.Generic;
using GTANetworkServer;
using mtgvrp.AdminSystem;
using mtgvrp.core;

namespace mtgvrp.player_manager.player_list
{
    public class PlayerList : Script
    {
        public PlayerList()
        {
            API.onClientEventTrigger += API_onClientEventTrigger;
        }

        private void API_onClientEventTrigger(Client player, string eventName, params object[] arguments)
        {
            switch (eventName)
            {
                case "fetch_player_list":

                    Account account = API.getEntityData(player.handle, "Account");

                    var playerList = new List<string>();
                    var type = Convert.ToInt32(arguments[0]);

                    foreach(var c in PlayerManager.Players)
                    {
                        if(type == 1)
                        {
                            Account a = API.getEntityData(c.Client.handle, "Account");
                            if (a.AdminDuty == false)
                                continue;
                        }

                        var dic = new Dictionary<string, object>
                        {
                            ["name"] = c.CharacterName,
                            ["id"] = PlayerManager.GetPlayerId(c)
                        };
                        playerList.Add(API.toJson(dic));
                    }

                    API.triggerClientEvent(player, "send_player_list", playerList, account.AdminLevel != 0);
                    break;
                case "player_list_pm":
                    ChatManager.pm_cmd(player, Convert.ToString(arguments[0]), Convert.ToString(arguments[1]));
                    break;
                case "player_list_teleport":
                    AdminCommands.goto_cmd(player, Convert.ToString(arguments[0]));
                    break;

                case "player_list_spectate":
                    AdminCommands.spec_cmd(player, Convert.ToString(arguments[0]));
                    break;

                case "player_list_kick":

                    break;
            }
        }
    }
}
