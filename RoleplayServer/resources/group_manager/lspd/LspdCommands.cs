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
            Vector3 ArrestPos = new Vector3(468.7845f, -1015.69f, 26.38641f);
            ColShape ArrestShape;
            ArrestShape = API.createCylinderColShape(ArrestPos, 2f, 3f);
        }

        [Command("cuff")] // cuff command
        public void cuff_cmd(Client player, Client target)
        {
            if (target == player){
                API.sendChatMessageToPlayer(player, "~r~You can't cuff yourself!");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(target)) > 16f)
            {
                API.sendChatMessageToPlayer(player, "You're too far away from ~b~" + target.name + "~w~ to arrest them.");
                return;
            }

            API.sendChatMessageToPlayer(player, "You have handcuffed ~b~" + target.name + "~w~.");
            API.sendChatMessageToPlayer(target, "You have been handcuffed by ~b~" + player.name + "~w~!");
            API.playPlayerAnimation(target, 1, "mp_arresting", "idle");
            API.setEntityData(target, "Tied", true);
        }

        [Command("uncuff")] // uncuff command
        public void uncuff_cmd(Client player, Client target)
        {
            if (target == player){
                API.sendChatMessageToPlayer(player, "~r~You can't cuff yourself!");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(target)) > 16f)
            {
                API.sendChatMessageToPlayer(player, "~r~You're too far away!");
                return;
            }

            if (API.getEntityData(target, "Tied") == false){
                API.sendChatMessageToPlayer(player, "This player is already uncuffed.");
            }

            API.sendChatMessageToPlayer(player, "~g~You have uncuffed " + target.name + ".");
            API.sendChatMessageToPlayer(target, "~g~You have been uncuffed by " + player.name + ".");
            API.stopPlayerAnimation(target);
            API.setEntityData(target, "Tied", false);
        }

        [Command("arrest")] // arrest command
        public void arrest_cmd(Client player, Client target, int time)
        {
            if (target == player)
            {
                API.sendChatMessageToPlayer(player, "~r~You can't arrest yourself!");
                return;
            }

            //check if crimes were recorded. If not, tell them to record crimes.


            if (!(bool)API.call("Lspd", "arrestPointCheck", player))
            {
                API.sendChatMessageToPlayer(player, "~r~You are too far away from the arrest point.");
                return;
            }

            API.sendChatMessageToPlayer(player, "You have arrested ~b~" + target.name + "~w~.");
            API.sendChatMessageToPlayer(target, "You have been arrested by ~b~" + target.name + "~w~.");
            GroupManager.SendGroupMessage(player, player.name + " has placed " + target.name + " under arrest.");
            API.call("Lspd", "jailControl", time);

            
        }

        [Command("frisk")]
        public void frisk_cmd(Client player, Client target)
        {
            if (target == player)
            {
                API.sendChatMessageToPlayer(player, "~r~You can't frisk yourself!");
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(target)) > 16f)
            {
                API.sendChatMessageToPlayer(player, "~r~You're too far away!");
                return;
            }

            //check items on player (first implement inv system).
        }

        [Command("detain")]
        public void detainPlayer(Client sender, Client target, int seatNumber)
        {
            if (API.getEntityData(target, "Tied") == false)
            {
                API.sendChatMessageToPlayer(sender, "Players must be tied/cuffed before you can detain them.");
                return;
            }
            if (seatNumber > 2)
            {
                API.sendChatMessageToPlayer(sender, "Seat number ranges from 0-2 (0 is the passenger seat).");
                return;
            }
            if (API.isPlayerInAnyVehicle(sender) == false)
            {
                API.sendChatMessageToPlayer(sender, "You must be in a vehicle.");
                return;
            }

            if (API.getEntityPosition(sender).DistanceToSquared(API.getEntityPosition(target)) > 20f)
            {
                API.sendChatMessageToPlayer(sender, "~r~You're too far away!");
                return;
            }

            API.sendChatMessageToPlayer(sender, "~g~You have detained " + target.name + " into a vehicle.");
            API.sendChatMessageToPlayer(target, "~g~You were detained by " + sender.name + " into a vehicle.");
            API.setPlayerIntoVehicle(target, sender.vehicle.handle, seatNumber);

        }

        [Command("shout", Alias = "s", GreedyArg = true)]
        public void shout_cmd(Client player, string text)
        {
            ChatManager.NearbyMessage(player, 25, PlayerManager.GetName(player) + " shouts: " + text);
        }
  
    }
}


/*
-------------------------------------
COMMANDS LIST:
-------------------------------------
- /stun
- /arrest
- /frisk
- /uncuff
- /cuff
- /detain
- /recordcrime
- /gate
- /radio
- /lspd
- /megaphone
- /recordcheck
- /elevator
- /door
- /ticket
- /confiscate
- /listmyfaction
- /fingerprint
- /backup
- /acceptbackup
- /cancelbackup
- /removeobject
- /deploy
- /breakin
- /dep
- /wanted
- /quitfaction
- /listranks
- /mdc
- /enterenf
- /lockenf
- /oocuncuff
- /removeplayer
- /checkeg
- /rappel
- /headphones
- /plantbug
- /removebug
- /tunein
- /ptazer
- /houseinfo
- /swatinv
- /givebadge
- /undercoverskins
- /togglefactionchat
- /gov
- /listbugs
- /removeallbugs
- /editpaychecks
- /invite
- /uninvite
- /motd
- /safelocation
- /changeranks
- /factionsafewithdraw
- /factionsafedeposit
- /factionsafebalance
- /changerank
- /disbandfaction
- /removewiretransfare
- /removeuninvite
*/