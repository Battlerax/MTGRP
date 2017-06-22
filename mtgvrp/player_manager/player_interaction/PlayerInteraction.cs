using System;
using System.Timers;
using GTANetworkServer;
using GTANetworkShared;
using mtgvrp.core;

namespace mtgvrp.player_manager.player_interaction
{
    class PlayerInteraction : Script
    {
        public PlayerInteraction()
        {
            API.onClientEventTrigger += (player, name, arguments) =>
            {
                switch (name)
                {
                    case "player_interaction_menu":
                    {

                        var option = Convert.ToString(arguments[0]);
                        var interactHandle = (NetHandle) arguments[1];

                        Client interactClient = API.getPlayerFromHandle(interactHandle);
                        Character interactCharacter = API.getEntityData(interactClient.handle, "Character");

                        Character character = API.getEntityData(player.handle, "Character");

                        switch (option)
                        {
                            case "follow":
                            {
                                character.FollowingPlayer = interactCharacter;
                                character.FollowingTimer = new Timer() { Interval = 1000 };
                                character.FollowingTimer.Elapsed += delegate { FollowPlayer(character, false); };
                                character.FollowingTimer.Start();
                                break;
                            }
                            case "view_description":
                            {
                                API.sendChatMessageToPlayer(player, "Not yet implemented. =(");
                                break;
                            }
                            case "cuff":
                            {
                                if (interactCharacter.IsCuffed == false)
                                {

                                    var isStunned = API.fetchNativeFromPlayer<bool>(interactClient,
                                        Hash.IS_PED_BEING_STUNNED, interactHandle, 0);
                                   
                                    if (interactCharacter.AreHandsUp == false && isStunned == false)
                                    {
                                        API.sendChatMessageToPlayer(player, Color.White,
                                            "You cannot cuff a player unless their hands are up or they are stunned.");
                                        return;
                                    }

                                    if (player.position.DistanceTo(interactCharacter.Client.position) > 3)
                                    {
                                        API.sendChatMessageToPlayer(player, Color.White,
                                            "You are too far away to handcuff that player.");
                                        return;
                                    }

                                    API.sendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, interactHandle, true);
                                    interactCharacter.IsCuffed = true;
                                    API.playPlayerAnimation(interactCharacter.Client, (int)(1 << 0 | 1 << 4 | 1 << 5),
                                        "mp_arresting", "idle");

                                    ChatManager.RoleplayMessage(player,
                                        "places handcuffs onto " + interactCharacter.rp_name(), ChatManager.RoleplayMe);

                                }
                                else
                                {
                                    if (player.position.DistanceTo(interactCharacter.Client.position) > 3)
                                    {
                                        API.sendChatMessageToPlayer(player, Color.White,
                                            "You are too far away to unhandcuff that player.");
                                        return;
                                    }

                                    API.sendNativeToAllPlayers(Hash.SET_ENABLE_HANDCUFFS, interactHandle, false);
                                    interactCharacter.IsCuffed = false;
                                    API.stopPlayerAnimation(interactCharacter.Client);

                                    ChatManager.RoleplayMessage(player,
                                        "removes the handcuffs from " + interactCharacter.rp_name(), ChatManager.RoleplayMe);
                                }

                                break;
                            }
                            case "drag":
                            {
                                if (player.position.DistanceTo(interactCharacter.Client.position) > 3)
                                {
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "You are too far away from that player.");
                                    return;
                                }

                                if (interactCharacter.FollowingPlayer == Character.None)
                                {
                                    interactCharacter.FollowingPlayer = character;
                                    interactCharacter.IsBeingDragged = true;
                                    interactCharacter.FollowingTimer = new Timer() {Interval = 1000};
                                    interactCharacter.FollowingTimer.Elapsed +=  delegate { FollowPlayer(interactCharacter, true); };
                                    interactCharacter.FollowingTimer.Start();

                                    ChatManager.RoleplayMessage(player, "grabs " + interactCharacter.rp_name() + " and begins to drag them.", ChatManager.RoleplayMe);
                                }
                                else if (interactCharacter.FollowingPlayer == character)
                                {
                                    interactCharacter.FollowingTimer.Stop();
                                    interactCharacter.IsBeingDragged = false;
                                    interactCharacter.FollowingPlayer = Character.None;
                                    ChatManager.RoleplayMessage(player, "lets go of " + interactCharacter.rp_name(),
                                        ChatManager.RoleplayMe);
                                    API.sendChatMessageToPlayer(player, "You have stopped dragging your target.");
                                }
                                else
                                {
                                    API.sendChatMessageToPlayer(player, Color.White,
                                        "That player is already being dragged by someone else.");
                                }
                                break;
                            }
                        }

                        break;
                    }
                    case "cancel_following":
                    {
                        Character character = API.getEntityData(player.handle, "Character");
                        if (character.FollowingPlayer != Character.None && character.IsBeingDragged == false)
                        {

                            character.FollowingTimer.Stop();
                            character.FollowingPlayer = Character.None;
                            API.sendChatMessageToPlayer(player, "You have stopped following your target.");
                        }
                        break;
                    }
                }
            };
        }

        [Command("detain", GreedyArg = true)]
        public void DetainPlayer(Client player, string id, string seatNumber)
        {

            var receiver = PlayerManager.ParseClient(id);
            Character character = API.getEntityData(receiver.handle, "Character");
    
            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (character.IsCuffed == false && character.IsTied == false)
            {
                API.sendChatMessageToPlayer(player, "Players must be tied/cuffed before you can detain them.");
                return;
            }

            if (int.Parse(seatNumber) > 2)
            {
                API.sendChatMessageToPlayer(player, "Seat number ranges from 0-2 (0 is the passenger seat).");
                return;
            }
            if (API.isPlayerInAnyVehicle(player) == false)
            {
                API.sendChatMessageToPlayer(player, "You must be in a vehicle.");
                return;
            }

            if (API.getEntityPosition(player).DistanceToSquared(API.getEntityPosition(receiver)) > 10f)
            {
                API.sendChatMessageToPlayer(player, "~r~You're too far away!");
                return;
            }

            API.setPlayerIntoVehicle(receiver, API.getPlayerVehicle(player), int.Parse(seatNumber));
            API.sendChatMessageToPlayer(player, "~g~You have detained " + receiver.name + " into a vehicle.");
            API.sendChatMessageToPlayer(receiver, "~g~You were detained by " + player.name + " into a vehicle.");


        }

        [Command("eject", GreedyArg = true)]
        public void ejectPlayer(Client player, string id)
        {
            var receiver = PlayerManager.ParseClient(id);

            if (receiver == null)
            {
                API.sendNotificationToPlayer(player, "~r~ERROR:~w~ Invalid player entered.");
                return;
            }

            if (API.isPlayerInAnyVehicle(player) == false || API.getPlayerVehicleSeat(player) != 0)
            {
                API.sendChatMessageToPlayer(player, "You must be in the front seat of a vehicle to eject another player.");
                return;
            }

            if (API.isPlayerInAnyVehicle(receiver) == false)
            {
                API.sendChatMessageToPlayer(player, "Players must be in a vehicle to be ejected from a vehicle.");
                return;
            }

            if (API.getPlayerVehicle(player) != API.getPlayerVehicle(receiver))
            {
                API.sendChatMessageToPlayer(player, "You must be in the same vehicle as another player to eject them.");
                return;
            }

            API.warpPlayerOutOfVehicle(receiver);
            API.sendChatMessageToPlayer(player, "You have ejected ~b~" + receiver.name + "~w~ from your vehicle.");
            API.sendChatMessageToPlayer(receiver, "~b~" + player.name + "~w~ has ejected you from their vehicle.");
        }
 
        public void FollowPlayer(Character c, bool isDrag)
        {
            API.sendNativeToAllPlayers(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, c.Client.handle,
                                    c.FollowingPlayer.Client.handle, -1.0, 0.0, 0.0, 1, 1050, 2, true);
            if (isDrag == false)
            {
                API.triggerClientEvent(c.Client, "player_interact_subtitle",
                    "You are following " + c.FollowingPlayer.rp_name() + ". Press SPACE to stop following.");
            }
            else
            {
                API.triggerClientEvent(c.Client, "player_interact_subtitle",
                    "You are being dragged by " + c.FollowingPlayer.rp_name() + ".");
            }
        }
    }
}
