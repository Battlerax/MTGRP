using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkServer;
using GTANetworkShared;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.core;

namespace RoleplayServer.resources.group_manager.lspd
{
    class LspdCommands : Script
    {
        public LspdCommands()
        {

        }
        [Command("recordcrimes", GreedyArg = true)]
        public void recordcrimes_cmd(Client player, string id, string crime)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.getEntityData(receiver.handle, "Character");

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }
            API.sendNotificationToPlayer(player, "You have recorded " + receiver.name + " for committing a crime.");
            API.sendNotificationToPlayer(receiver, player.name + " has recorded a crime you committed: ~r~" + crime + "~w~.");
            API.setEntityData(receiver, "crimesRecorded", true);
            character.playerCrimes += 1;

        }
        [Command("arrest", GreedyArg = true)] // arrest command
        public void arrest_cmd(Client player, string id, int time)
        {

            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");
            Character receiverCharacter = API.getEntityData(receiver.handle, "Character");

            if (character.GroupId != 1) {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            if(API.getEntityData(receiver, "crimesRecorded") == false)
            {
                API.sendChatMessageToPlayer(player, "You must record this player's crimes before they can be arrested.");
            }
            if (character.GroupId == receiverCharacter.GroupId)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You cannot arrest a member of the LSPD.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (!(bool)API.call("Lspd", "arrestPointCheck", player))
            {
                API.sendNotificationToPlayer(player, "~r~You are too far away from the arrest point.");
                return;
            }

            API.sendNotificationToPlayer(player, "You have arrested ~b~" + receiver.name + "~w~.");
            API.sendNotificationToPlayer(receiver, "You have been arrested by ~b~" + player.name + "~w~.");
            GroupManager.SendGroupMessage(player, player.name + " has placed " + receiver.name + " under arrest.");
            API.setEntityData(receiver, "crimesRecorded", false);
            API.call("Lspd", "jailControl", time);
        }

        [Command("frisk", GreedyArg = true)]
        public void frisk_cmd(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            Character character = API.getEntityData(player.handle, "Character");

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (receiver == player)
            {
                API.sendNotificationToPlayer(player, "~r~You can't frisk yourself!");
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 16f)
            {
                API.sendNotificationToPlayer(player, "~r~You're too far away!");
                return;
            }

            //check items on player (first implement inv system.
        }

        [Command("megaphone", Alias = "mp", GreedyArg = true)]
        public void megaphone_cmd(Client player, string text)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.GroupId != 1)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ You must be a member of the LSPD to use this command.");
                return;
            }

            ChatManager.NearbyMessage(player, 30, PlayerManager.GetName(player) + " [MEGAPHONE]: " + text);
        }
  
    }
}