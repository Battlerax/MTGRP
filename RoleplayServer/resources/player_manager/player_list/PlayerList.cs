using System;
using System.Collections.Generic;
using GTANetworkServer;
using GTANetworkShared;

namespace RoleplayServer.resources.player_manager.player_list
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

                    List<string> player_list = new List<string>();
                    int type = Convert.ToInt32(arguments[0]);

                    foreach(Character c in PlayerManager.players)
                    {
                        if(type == 1)
                        {
                            Account a = API.getEntityData(c.client.handle, "Account");
                            if (a.admin_duty == 0)
                                continue;
                        }

                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        dic["name"] = c.character_name;
                        dic["id"] = PlayerManager.getPlayerId(c);
                        player_list.Add(API.toJson(dic));
                    }

                    API.triggerClientEvent(player, "send_player_list", player_list, (account.admin_level == 0) ? (false) : (true));
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
