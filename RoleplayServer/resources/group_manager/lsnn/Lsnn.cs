using GTANetworkServer;
using GTANetworkShared;
using System.Collections.Generic;
using RoleplayServer.resources.core;
using RoleplayServer.resources.player_manager;
using RoleplayServer.resources.vehicle_manager;
using System;

namespace RoleplayServer.resources.group_manager.lsnn
{
    class Lsnn : Script
    {
        public Lsnn()
        {
            API.onResourceStart += startLsnn;
        }

        public void startLsnn()
        {

        }

        public string headline = null;
        public int lottoSafe = 0;
        public bool IsBroadcasting = false;
        public bool CameraSet = false;
        public bool chopperCamToggle = false;
        public Vector3 CameraPosition = null;
        public Vector3 CameraRotation = null;

        [Command("broadcast")]
        public void broadcast_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (CameraSet == false)
            {
                API.sendChatMessageToPlayer(player, "You must place a camera before starting a broadcast.");
                return;
            }

            if (IsBroadcasting == true)
            {
                IsBroadcasting = false;
                API.sendChatMessageToPlayer(player, "~p~The broadcast has been stopped.");

                foreach (var c in API.getAllPlayers())
                {
                    Character receivercharacter = API.getEntityData(c, "Character");
                    if (receivercharacter.IsWatchingBroadcast)
                    {

                        API.triggerClientEvent(c, "unwatch_broadcast");
                        receivercharacter.IsWatchingBroadcast = false;
                        API.sendChatMessageToPlayer(c, "~p~The LSNN broadcast has been stopped.");
                    }
                }
                return;
            }
            API.sendChatMessageToPlayer(player, "Broadcast started.");
            API.sendChatMessageToAll("~p~An LSNN broadcast has been started. /watchbroadcast to tune in!");
            IsBroadcasting = true;

        }

        [Command("editheadline")]
        public void editbanner_cmd(Client player, string text)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            headline = text;
        }

        [Command("setcamera")]
        public void setcamera_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (character.HasCamera == false)
            {
                API.sendChatMessageToPlayer(player, "You do not have a camera in your inventory."); //Once inv system is done this will be fixed to work with it properly.
                return;
            }

            var pos = API.getEntityPosition(player.handle);
            var angle = API.getEntityRotation(player.handle).Z;
            CameraPosition = XYInFrontOfPoint(pos, angle, 1) - new Vector3(0, 0, 0.1);
            var camRot = API.getEntityRotation(player.handle) + new Vector3(0, 0, 180);
            API.sendNotificationToPlayer(player, "A camera has been placed on your position.");
            var camera = API.createObject(API.getHashKey("p_tv_cam_02_s"), CameraPosition, CameraRotation);
            API.triggerClientEvent(player, "cam_drop");
            character.HasCamera = false;
            CameraSet = true;
        }

        [Command("choppercam")]
        public void choppercam_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            //TODO: CHECK IF IN LSNN CHOPPER
            var vehicleHandle = API.getPlayerVehicle(player);
            var veh = VehicleManager.GetVehFromNetHandle(vehicleHandle);
            if (character.Group.Id != veh.GroupId && veh.VehModel != VehicleHash.Maverick)
            {
                API.sendChatMessageToPlayer(player, "You must be in an LSNN chopper to use the chopper camera.");
                return;
            }

            if (CameraSet == true)
            {
                API.sendChatMessageToPlayer(player, "A camera has already been set.");
                return;
            }

            if (chopperCamToggle == true)
            {
                API.sendNotificationToPlayer(player, "The chopper camera has been turned ~r~off~w~.");
                CameraSet = false;
                chopperCamToggle = false;
                return;
            }

            CameraSet = true;
            chopperCamToggle = true;
            var chopper = API.getPlayerVehicle(player);
            CameraPosition = API.getEntityPosition(chopper) - new Vector3(0, 0, 3);
            CameraRotation = API.getEntityRotation(chopper);
            API.sendNotificationToPlayer(player, "The chopper camera has been turned ~b~on~w~.");
        }

        [Command("pickupcamera")]
        public void pickupcamera_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (IsBroadcasting == true)
            {
                API.sendChatMessageToPlayer(player, "A broadcast is in progress.");
            }

            if (CameraSet == false)
            {
                foreach (var p in API.getAllPlayers())
                {
                    Character c = API.getEntityData(p, "Character");
                    if (c.HasCamera == true)
                    {
                        API.sendChatMessageToPlayer(player, "There are no cameras left to pick up.");
                        return;
                    }

                }
                foreach(var v in VehicleManager.Vehicles)
                {
                    if(v.GroupId == character.GroupId)
                    {
                        if (CameraSet == true && player.position.DistanceTo(CameraPosition) > 2f && player.position.DistanceTo(API.getEntityPosition(v.NetHandle)) < 3f)
                        {
                            API.sendChatMessageToPlayer(player, "You can only have one camera.");
                            return;
                        }

                        if (player.position.DistanceTo(API.getEntityPosition(v.NetHandle)) < 3f)
                        {
                            API.sendChatMessageToPlayer(player, "You grabbed a camera from the news vehicle.");
                            character.HasCamera = true;
                            return;
                        }
                    }
                }
                API.sendChatMessageToPlayer(player, "You are too far away from a news vehicle.");
            }

            var playerPos = API.getEntityPosition(player);
            API.sendNotificationToPlayer(player, "You are carrying a heavy camera", true);
            API.triggerClientEvent(player, "cam_carry");
            API.deleteObject(player, playerPos, API.getHashKey("p_tv_cam_02_s"));
            character.HasCamera = true;
            CameraSet = false;
            }

        [Command("viewercount")]
        public void viewercount_cmd(Client player)
        {
            var count = 0;
            foreach (var c in PlayerManager.Players)
            {
                if (c.IsWatchingBroadcast == true)
                {
                    count += 1;
                }

            }
            API.sendChatMessageToPlayer(player, count + " people are watching the broadcast.");
        }
        
        [Command("lotto")]
        public void lotto_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            //MONEY FROM LOTTO TICKETS GO DIRECTLY TO LSNN LOTTO SAFE (COMPLETE WHEN 24/7s ARE IMPLEMENTED!)
            List<string> haveLottoTickets = new List<string>();
            var random = new Random();
            foreach (var c in PlayerManager.Players)
            {
                if (c.HasLottoTicket == true)
                {
                    haveLottoTickets.Add(c.CharacterName);
                }
            }
            if (haveLottoTickets.Count <= 2)
            {
                API.sendChatMessageToPlayer(player, "There are too few people taking part in the lottery!");
                return;
            }

            int index = random.Next(haveLottoTickets.Count);
            API.sendChatMessageToPlayer(player, "~p~ You pick a random name from the list of ticket owners..");
            API.sendChatMessageToAll("~p~The winner of the lotto is ~y~" + haveLottoTickets[index] + "~p~!");
        }

        [Command("watchbroadcast")]
        public void watchbroadcast_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            var camPos = CameraPosition + new Vector3(0, 0, 0.94);
            var camRot = CameraRotation + new Vector3(-1, 0, 180);

            if (character.IsWatchingBroadcast == true)
            {
                API.sendChatMessageToPlayer(player, "You are already watching the broadcast.");
                return;
            }

            if (character.Group.CommandType == Group.CommandTypeLsnn && CameraSet == true)
            {
                API.triggerClientEvent(player, "watch_broadcast", camPos, camRot, headline);
                character.IsWatchingBroadcast = true;
                return;
            }
       
            if (IsBroadcasting == false)
            {
                API.sendChatMessageToPlayer(player, "There is currently no live broadcast.");
                return;
            }

            API.triggerClientEvent(player, "watch_broadcast", camPos, camRot, headline);
            //API.sendNativeToPlayer(player, 0xBB7454BAFF08FE25, camPos, camRot, 0, 0, 0);
            API.freezePlayer(player, true);
            character.IsWatchingBroadcast = true;
        }

        [Command("stopwatching")]
        public void stopwatching_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if(character.IsWatchingBroadcast == false)
            {
                API.sendChatMessageToPlayer(player, "You are not watching any broadcasts.");
            }
            API.triggerClientEvent(player, "unwatch_broadcast");
            API.sendNativeToPlayer(player, 0x31B73D1EA9F01DA2);
            API.freezePlayer(player, false);
            character.IsWatchingBroadcast = false;
        }

        [Command("mic")]
        public void mictoggle_cmd(Client player)
        {
            var playerPos = API.getEntityPosition(player);
            Character character = API.getEntityData(player.handle, "Character");

            if (character.HasMic == false && character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, "You do not have a microphone.");
                return;
            }

            if (API.getEntityData(player, "MicStatus") != true)
            {
                API.sendNotificationToPlayer(player, "You are speaking through a microphone.", true);
                API.setEntityData(player, "MicStatus", true);
                var microphone = API.createObject(API.getHashKey("p_ing_microphonel_01"), playerPos, new Vector3());
                API.attachEntityToEntity(microphone, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                return;
            }
            API.sendNotificationToPlayer(player, "You are no longer speaking through a microphone.");
            API.setEntityData(player, "MicStatus", false);
            API.deleteObject(player, playerPos, API.getHashKey("p_ing_microphonel_01"));

        }

        [Command("givemic")]
        public void micpower_cmd(Client player, string id)
        {
            var target = PlayerManager.ParseClient(id);

            Character sendercharacter = API.getEntityData(player.handle, "Character");
            Character character = API.getEntityData(target.handle, "Character");

            if (sendercharacter.Group == Group.None || sendercharacter.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (character.HasMic == false)
            {
                character.HasMic = true;
                API.sendChatMessageToPlayer(target, "You have been given a microphone. Use /mic to toggle it on/off.");
                API.sendChatMessageToPlayer(player, "You have given a microphone to " + target.name);
                return;
            }
            character.HasMic = false;
            API.sendChatMessageToPlayer(target, "Microphone revoked. You can no longer use the microphone.");
            API.sendChatMessageToPlayer(player, "Microphone removed from " + target.name);

        }

        [Command("createarticle")]
        public void createarticle_cmd(Client player)
        {
            Character character = API.getEntityData(player.handle, "Character");

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                API.sendChatMessageToPlayer(player, Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            API.sendChatMessageToPlayer(player, "Not yet implemented :(");
            //WILL BE IMPLEMENTING CEF UI FOR WRITING ARTICLES
            //OPTION TO INPUT TITLE AND TEXT
        }

        public void getPositionInfrontOfEntity(Client player, double x, double y, double distance)
        {
            var playerRot = API.getEntityRotation(player);
            x += (distance * Math.Sin(playerRot.Y));
            y += (distance * Math.Cos(playerRot.Y));
        }

        public static Vector3 XYInFrontOfPoint(Vector3 pos, float angle, float distance)
        {
            Vector3 ret = pos.Copy();
            ret.X += (distance * (float)Math.Sin(angle));
            ret.Y += (distance * (float)Math.Cos(angle));
            return ret;

        }
    }
}
