using System;
using System.Collections.Generic;
using System.Timers;
using GTANetworkAPI;
using mtgvrp.core;
using mtgvrp.inventory;
using mtgvrp.player_manager;
using mtgvrp.vehicle_manager;
using mtgvrp.core.Help;

namespace mtgvrp.group_manager.lsnn
{
    class Lsnn : Script
    {
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, byte type, string reason)
        {
            var character = player.GetCharacter();
            if (character == null) return;

            if (character.MicObject != null && NAPI.Entity.DoesEntityExist(character.MicObject))
                NAPI.Entity.DeleteEntity(character.MicObject);
        }

        public readonly Vector3 LsnnFrontDoor = new Vector3(-319.0662f, -609.8559f, 33.55819f);

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            LsnnFrontDoorShape = NAPI.ColShape.CreateCylinderColShape(LsnnFrontDoor, 2f, 3f);

            NAPI.Marker.CreateMarker(1, LsnnFrontDoor - new Vector3(0, 0, 1f), new Vector3(), new Vector3(),
                1f, new GTANetworkAPI.Color(100, 51, 153, 255), false);
        }

        public ColShape LsnnFrontDoorShape;
        public string Headline = "Los Santos News Network";
        public bool IsBroadcasting = false;
        public bool CameraSet = false;
        public bool ChopperCamToggle = false;
        public Entity Chopper;
        public Vector3 CameraPosition;
        public Vector3 CameraRotation;
        public Vector3 OffSet = new Vector3(0, 0, -3);
        public Timer ChopperRotation = new Timer();
        public int CameraDimension = 0;

        [Command("broadcast"), Help(HelpManager.CommandGroups.LSNN, "Start a broadcast.", null)]
        public void broadcast_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (CameraSet == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must place a camera before starting a broadcast.");
                return;
            }

            if (IsBroadcasting == true)
            {
                IsBroadcasting = false;
                NAPI.Chat.SendChatMessageToPlayer(player, "~p~The broadcast has been stopped.");

                foreach (var c in API.GetAllPlayers())
                {
                    if (c == null)
                        continue;

                    Character receivercharacter = c.GetCharacter();

                    if(receivercharacter == null)
                        continue;

                    if (receivercharacter.IsWatchingBroadcast)
                    {

                        NAPI.ClientEvent.TriggerClientEvent(c, "unwatch_broadcast");
                        receivercharacter.IsWatchingBroadcast = false;
                        NAPI.Chat.SendChatMessageToPlayer(player, "~p~" + character.rp_name() + " has stopped the broadcast.");
                    }
                    
                    if (receivercharacter.HasMic == true && receivercharacter.Group.CommandType != Group.CommandTypeLsnn)
                    {
                        receivercharacter.HasMic = false;
                        NAPI.Data.SetEntityData(c, "MicStatus", false);
                    }
                }
                return;
            }
            NAPI.Chat.SendChatMessageToPlayer(player, "Broadcast started.");
            API.SendChatMessageToAll("~p~" + character.rp_name() + " has started a broadcast. /watchbroadcast to tune in!");
            IsBroadcasting = true;

        }

        [Command("editheadline", GreedyArg = true), Help(HelpManager.CommandGroups.LSNN, "Edit the broadcast headline text.", new[] { "Text being displayed on the broadcast." })]
        public void editbanner_cmd(Player player, string text)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            Headline = text;
            NAPI.Chat.SendChatMessageToPlayer(player, "Headline edited.");
        }

        [Command("setcamera"), Help(HelpManager.CommandGroups.LSNN, "Set down a camera for broadcasting.", null)]
        public void setcamera_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (character.HasCamera == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You do not have a camera in your inventory.");
                return;
            }

            if (CameraSet == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "A camera has already been set.");
                return;
            }

            var pos = NAPI.Entity.GetEntityPosition(player);
            var angle = API.GetEntityRotation(player).Z;
            CameraPosition = XyInFrontOfPoint(pos, angle, 1) - new Vector3(0, 0, 0.5);
            CameraRotation = API.GetEntityRotation(player) + new Vector3(0, 0, 180);
            CameraDimension = (int)API.GetEntityDimension(player);
            NAPI.Notification.SendNotificationToPlayer(player, "A camera has been placed on your position.");
            ChatManager.NearbyMessage(player, 10, "~p~" + character.rp_name() + " sets down a news camera");
            API.CreateObject(API.GetHashKey("p_tv_cam_02_s"), CameraPosition, CameraRotation);
            character.HasCamera = false;
            CameraSet = true;
        }

        [Command("choppercam"), Help(HelpManager.CommandGroups.LSNN, "Toggle the chopper cam on/off", null )]
        public void choppercam_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            var vehicleHandle = NAPI.Player.GetPlayerVehicle(player);
            var veh = VehicleManager.GetVehFromNetHandle(vehicleHandle);

            if (character.Group.Id != veh.GroupId && veh.VehModel != VehicleHash.Maverick)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You must be in an LSNN chopper to use the chopper camera.");
                return;
            }

            if (CameraSet == true && ChopperCamToggle == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "A camera has already been set. /pickupcamera before using the chopper cam.");
                return;
            }

            if (ChopperCamToggle == true)
            {
                if (IsBroadcasting == true)
                {
                    foreach (var c in API.GetAllPlayers())
                    {
                        if (c == null)
                            continue;

                        Character receivercharacter = c.GetCharacter();
                        if (receivercharacter == null)
                            continue;

                        if (receivercharacter.IsWatchingBroadcast)
                        {

                            NAPI.ClientEvent.TriggerClientEvent(c, "unwatch_broadcast");
                            receivercharacter.IsWatchingBroadcast = false;
                            NAPI.Chat.SendChatMessageToPlayer(c, "~p~The LSNN camera has been turned off.");
                        }
                    }
                }

                NAPI.Notification.SendNotificationToPlayer(player, "The chopper camera has been turned ~r~off~w~.");
                ChatManager.NearbyMessage(player, 10, "~p~" + character.rp_name() + " has turned off the chopper cam.");
                CameraPosition = new Vector3();
                CameraRotation = new Vector3();
                CameraSet = false;
                ChopperCamToggle = false;
                ChopperRotation.Stop();
                return;
            }

            CameraSet = true;
            ChopperCamToggle = true;
            Chopper = NAPI.Player.GetPlayerVehicle(player);
            CameraPosition = NAPI.Entity.GetEntityPosition(Chopper) - new Vector3(0, 0, 3);
            CameraRotation = API.GetEntityRotation(Chopper);
            NAPI.Notification.SendNotificationToPlayer(player, "The chopper camera has been turned ~b~on~w~.");
            ChatManager.NearbyMessage(player, 10, "~p~" + character.rp_name() + " has turned on the chopper cam.");
            ChopperRotation = new Timer { Interval = 3000 };
            ChopperRotation.Elapsed += delegate { UpdateChopperRotation(player); };
            ChopperRotation.Start();
        }


        [Command("pickupcamera"), Help(HelpManager.CommandGroups.LSNN, "Pick up the a broadcast camera.", null)]
        public void pickupcamera_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (IsBroadcasting == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "A broadcast is in progress.");
            }

            if (CameraSet == false)
            {
                foreach (var p in API.GetAllPlayers())
                {
                    if (p == null)
                        continue;

                    Character c = p.GetCharacter();
                    if (c?.HasCamera == true)
                    {
                        NAPI.Chat.SendChatMessageToPlayer(player, "There are no cameras left to pick up.");
                        return;
                    }

                }
                foreach(var v in VehicleManager.Vehicles)
                {
                    if(v.GroupId == character.GroupId)
                    {
                        if (CameraSet == true && player.Position.DistanceTo(CameraPosition) > 2f && player.Position.DistanceTo(NAPI.Entity.GetEntityPosition(v.Entity)) < 3f)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, "You can only have one camera.");
                            return;
                        }

                        if (player.Position.DistanceTo(NAPI.Entity.GetEntityPosition(v.Entity)) < 3f)
                        {
                            NAPI.Chat.SendChatMessageToPlayer(player, "You grabbed a camera from the news vehicle.");
                            ChatManager.NearbyMessage(player, 10, "~p~" + character.rp_name() + " reaches into the news vehicle, pulling out a camera.");
                            character.HasCamera = true;
                            return;
                        }
                    }
                }
                NAPI.Chat.SendChatMessageToPlayer(player, "You are too far away from a news vehicle.");
                return;
            }

            var playerPos = NAPI.Entity.GetEntityPosition(player);
            NAPI.Notification.SendNotificationToPlayer(player, "You are carrying a camera.", true);
            ChatManager.NearbyMessage(player, 10, "~p~" + character.rp_name() + " picks up the news camera.");
            API.DeletePlayerWorldProp(player, API.GetHashKey("p_tv_cam_02_s"), playerPos, 2.5f); // CONV NOTE: adjust radius value
            character.HasCamera = true;
            CameraPosition = new Vector3();
            CameraRotation = new Vector3();
            CameraDimension = 0;
            CameraSet = false;
            }

        [Command("viewercount"), Help(HelpManager.CommandGroups.LSNN | HelpManager.CommandGroups.General, "Show the amount of viewers currently watching the broadcast.", null)]
        public void viewercount_cmd(Player player)
        {
            var count = 0;
            foreach (var c in PlayerManager.Players)
            {
                if (c.IsWatchingBroadcast == true)
                {
                    count += 1;
                }

            }
            NAPI.Chat.SendChatMessageToPlayer(player, count + " people are watching the broadcast.");
        }
        
        [Command("lotto"), Help(HelpManager.CommandGroups.LSNN, "Throw a lotto and show the winner. Players must buy lotto tickets.", null)]
        public void lotto_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            List<Character> haveLottoTickets = new List<Character>();
            var random = new Random();
            foreach (var c in PlayerManager.Players)
            {
                if (c.HasLottoTicket == true)
                {
                    haveLottoTickets.Add(c);
                    c.HasLottoTicket = false;
                }
            }
            if (haveLottoTickets.Count <= 2)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There are too few people taking part in the lottery!");
                return;
            }

            int index = random.Next(haveLottoTickets.Count);

            InventoryManager.GiveInventoryItem(haveLottoTickets[index], new Money(), character.Group.LottoSafe);
            NAPI.Chat.SendChatMessageToPlayer(player, "~p~ You pick a random name from the list of ticket owners..");
            API.SendChatMessageToAll("~p~The winner of the lotto is ~y~" + haveLottoTickets[index].rp_name() + "~p~. They won " + character.Group.LottoSafe + "!");

        }

        [Command("watchbroadcast"), Help(HelpManager.CommandGroups.LSNN | HelpManager.CommandGroups.General, "Start watching the broadcast.", null)]
        public void watchbroadcast_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (CameraPosition == null || CameraRotation == null || IsBroadcasting == false && character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "There is currently no live broadcast.");
                return;
            }


            var camPos = CameraPosition + new Vector3(0, 0, 0.94);
            var camRot = CameraRotation + new Vector3(-1, 0, 180);
            var focusX = CameraPosition.X;
            var focusY = CameraPosition.Y;
            var focusZ = CameraPosition.Z;

            if (character.IsWatchingBroadcast == true)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are already watching the broadcast.");
                return;
            }

            if (character.Group.CommandType == Group.CommandTypeLsnn && ChopperCamToggle == true)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "watch_chopper_broadcast", CameraPosition, CameraRotation, Headline, Chopper, OffSet, focusX, focusY, focusZ);
                character.IsWatchingBroadcast = true;
                return;
            }

            if (character.Group.CommandType == Group.CommandTypeLsnn && CameraSet == true)
            {
                NAPI.ClientEvent.TriggerClientEvent(player, "watch_broadcast", camPos, camRot, Headline, focusX, focusY, focusZ);
                character.IsWatchingBroadcast = true;
                return;
            }

            if (ChopperCamToggle == true)
            {
                
                NAPI.ClientEvent.TriggerClientEvent(player, "watch_chopper_broadcast", CameraPosition, CameraRotation, Headline, Chopper, OffSet, focusX, focusY, focusZ);
                player.TriggerEvent("freezePlayer", true);
                character.IsWatchingBroadcast = true;
                return;
            }

            NAPI.Entity.SetEntityDimension(player, (uint)CameraDimension);
            NAPI.Chat.SendChatMessageToPlayer(player, "You are watching the broadcast. Use /stopwatching to stop watching .");
            NAPI.ClientEvent.TriggerClientEvent(player, "watch_broadcast", camPos, camRot, Headline, focusX, focusY, focusZ);
            player.TriggerEvent("freezePlayer", true);
            character.IsWatchingBroadcast = true;
        }

        [Command("stopwatching"), Help(HelpManager.CommandGroups.LSNN | HelpManager.CommandGroups.General, "Stop watching the broadcast.", null)]
        public void stopwatching_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if(character.IsWatchingBroadcast == false)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are not watching any broadcasts.");
            }
            NAPI.ClientEvent.TriggerClientEvent(player, "unwatch_broadcast");
            player.TriggerEvent("freezePlayer", false);
            character.IsWatchingBroadcast = false;
        }

        [Command("mic"), Help(HelpManager.CommandGroups.LSNN | HelpManager.CommandGroups.General, "Toggle the use of a microphone. Speak normally to use it.", null)]
        public void mictoggle_cmd(Player player)
        {
            var playerPos = NAPI.Entity.GetEntityPosition(player);
            Character character = player.GetCharacter();

            if (character.HasMic == false && character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You do not have a microphone.");
                return;
            }

            if (NAPI.Data.GetEntityData(player, "MicStatus") != true)
            {
                NAPI.Notification.SendNotificationToPlayer(player, "You are speaking through a microphone.", true);
                NAPI.Data.SetEntityData(player, "MicStatus", true);
                character.MicObject = API.CreateObject(API.GetHashKey("p_ing_microphonel_01"), playerPos, new Vector3());
                API.AttachEntityToEntity(character.MicObject, player, "IK_R_Hand", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                return;
            }
            NAPI.Notification.SendNotificationToPlayer(player, "You are no longer speaking through a microphone.");
            NAPI.Data.SetEntityData(player, "MicStatus", false);
            if (character.MicObject != null && API.DoesEntityExist(character.MicObject))
                API.DeleteEntity(character.MicObject);
            character.MicObject = null;
        }

        [Command("givemic"), Help(HelpManager.CommandGroups.LSNN, "Give a microphone to a player.", new[] { "The target player ID." })]
        public void micpower_cmd(Player player, string id)
        {
            var target = PlayerManager.ParseClient(id);

            Character sendercharacter = player.GetCharacter();
            Character character = target.GetCharacter();

            if (sendercharacter.Group == Group.None || sendercharacter.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            if (player.Position.DistanceTo(target.Position) > 2f)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, "You are too far away from that player.");
                return;
            }

            if (character.HasMic == false)
            {
                character.HasMic = true;
                NAPI.Chat.SendChatMessageToPlayer(target, "You have been given a microphone. Use /mic to toggle it on/off.");
                NAPI.Chat.SendChatMessageToPlayer(player, "You have given a microphone to " + target.Name);
                return;
            }
            character.HasMic = false;
            NAPI.Chat.SendChatMessageToPlayer(target, "Microphone revoked. You can no longer use the microphone.");
            NAPI.Chat.SendChatMessageToPlayer(player, "Microphone removed from " + target.Name);

        }

        [Command("createarticle"), Help(HelpManager.CommandGroups.LSNN, "Start making an article.", null)]
        public void createarticle_cmd(Player player)
        {
            Character character = player.GetCharacter();

            if (character.Group == Group.None || character.Group.CommandType != Group.CommandTypeLsnn)
            {
                NAPI.Chat.SendChatMessageToPlayer(player, core.Color.White, "You must be a member of the LSNN to use that command.");
                return;
            }

            NAPI.Chat.SendChatMessageToPlayer(player, "Not yet implemented :(");
            //WILL BE IMPLEMENTING CEF UI FOR WRITING ARTICLES
            //OPTION TO INPUT TITLE AND TEXT
        }

        public void GetPositionInfrontOfEntity(Player player, double x, double y, double distance)
        {
            var playerRot = API.GetEntityRotation(player);
            x += (distance * Math.Sin(playerRot.Y));
            y += (distance * Math.Cos(playerRot.Y));
        }

        public static Vector3 XyInFrontOfPoint(Vector3 pos, float angle, float distance)
        {
            Vector3 ret = pos.Copy();
            ret.X += (distance * (float)Math.Sin(angle));
            ret.Y += (distance * (float)Math.Cos(angle));
            return ret;

        }

        public void UpdateChopperRotation(Player player)
        {
            Chopper = NAPI.Player.GetPlayerVehicle(player);
            CameraPosition = NAPI.Entity.GetEntityPosition(Chopper) - new Vector3(0, 0, 3);
            CameraRotation = API.GetEntityRotation(Chopper);
            var focusX = CameraPosition.X;
            var focusY = CameraPosition.Y;
            var focusZ = CameraPosition.Z;

            foreach (var p in API.GetAllPlayers())
            {
                if (p == null)
                    continue;

                Character character = p.GetCharacter();

                if(character == null)
                    continue;

                if (character.IsWatchingBroadcast)
                {
                    NAPI.ClientEvent.TriggerClientEvent(p, "update_chopper_cam", CameraPosition, CameraRotation, Headline, Chopper, OffSet, focusX, focusY, focusZ);
                }
            }
        }
    }
}
