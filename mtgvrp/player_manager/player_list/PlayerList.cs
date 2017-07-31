using System;
using System.Collections.Generic;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server.Elements;
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
                    Character character = player.GetCharacter();

                    var playerList = new List<string[]>();
                    var type = Convert.ToInt32(arguments[0]);

                    if (character.GroupId == 0 && type == 2)
                    {
                        API.sendNotificationToPlayer(player, "You aren't in any group");
                        return;
                    }

                    foreach (var c in PlayerManager.Players)
                    {
                        if(type == 1)
                        {
                            Account a = API.getEntityData(c.Client.handle, "Account");
                            if (a.AdminDuty == false)
                                continue;
                        }

                        if (type == 2)
                        {
                            Character a = c.Client.GetCharacter();
                            if (a.GroupId == character.GroupId && character.GroupId != 0)
                                continue;
                        }

                        playerList.Add(new [] { c.CharacterName , PlayerManager.GetPlayerId(c).ToString() });
                    }

                    Init.SendEvent(player, "send_player_list", API.toJson(playerList.ToArray()), account.AdminLevel != 0);
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
                    AdminCommands.kick_cmd(player, Convert.ToString(arguments[0]), "PlayerList");
                    break;
            }
        }
    }
}
