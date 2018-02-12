using System;
using System.Collections.Generic;

using GTANetworkAPI;
using mtgvrp.AdminSystem;
using mtgvrp.core;

namespace mtgvrp.player_manager.player_list
{
    public class PlayerList : Script
    {
        public PlayerList()
        {
        }

        [RemoteEvent("fetch_player_list")]
        private void FetchPlayerList(Client player, params object[] arguments)
        {
            Account account = player.GetAccount();
            Character character = player.GetCharacter();

            var playerList = new List<string[]>();
            var type = Convert.ToInt32(arguments[0]);

            if (character == null || account == null)
                return;

            if (character.GroupId == 0 && type == 2)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You aren't in any group");
                return;
            }

            foreach (var c in PlayerManager.Players)
            {
                if (type == 1)
                {
                    Account a = c.Client.GetAccount();
                    if (a.AdminDuty == false)
                        continue;
                }

                if (type == 2)
                {
                    Character a = c.Client.GetCharacter();
                    if (a.GroupId != character.GroupId)
                        continue;
                }

                playerList.Add(new[] { c.CharacterName, PlayerManager.GetPlayerId(c).ToString() });
            }

            NAPI.ClientEvent.TriggerClientEvent(player, "send_player_list", NAPI.Util.ToJson(playerList.ToArray()), account.AdminLevel != 0);
        }

        [RemoteEvent("player_list_pm")]
        private void PlayerListPM(Client player, params object[] arguments)
        {
            ChatManager.pm_cmd(player, Convert.ToString(arguments[0]), Convert.ToString(arguments[1]));
        }

        [RemoteEvent("player_list_teleport")]
        private void PlayerListTeleport(Client player, params object[] arguments)
        {
            AdminCommands.goto_cmd(player, Convert.ToString(arguments[0]));
        }

        [RemoteEvent("player_list_spectate")]
        private void PlayerListSpectate(Client player, params object[] arguments)
        {
            AdminCommands.spec_cmd(player, Convert.ToString(arguments[0]));
        }

        [RemoteEvent("player_list_kick")]
        private void PlayerListKick(Client player, params object[] arguments)
        {
            AdminCommands.kick_cmd(player, Convert.ToString(arguments[0]), "PlayerList");
        }
    }
}
